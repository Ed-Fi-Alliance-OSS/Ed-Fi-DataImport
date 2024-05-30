// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Logging;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.Environment;
using File = DataImport.Models.File;
using LogLevels = DataImport.Common.Enums.LogLevel;
using DataImport.Common.ExtensionMethods;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class FileProcessor
    {
        private class FileResponse
        {
            public int Errors { get; set; }
            public int Exists { get; set; }
            public int Success { get; set; }
            public string Message { get; set; }
        }

        private enum RowResult
        {
            Success,
            Exist,
            Error,
            Duplicate
        }

        public class Command : IRequest
        {
            public bool CheckMetadata { get; set; } = true;

            public IOdsApi OdsApi { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dbContext;
            private readonly IPowerShellPreprocessorService _powerShellPreprocessorService;
            private readonly IExternalPreprocessorService _externalPreprocessorService;
            private readonly ConcurrencySettings _concurrencySettings;
            private readonly ConcurrentBag<string> _alreadyProcessedResources;
            private LookupCollection _mappingLookups;
            private readonly Dictionary<int, List<FileResponse>> _fileResponses;
            private readonly IFileService _fileService;
            private readonly List<string> _ingestionLogLevels;

            public CommandHandler(ILogger<FileProcessor> logger, IOptions<AppSettings> options, DataImportDbContext dbContext, ResolveFileService fileServices, IPowerShellPreprocessorService powerShellPreprocessorService, IExternalPreprocessorService externalPreprocessorService, IOptions<ConcurrencySettings> concurrencySettings)
            {
                _logger = logger;
                _powerShellPreprocessorService = powerShellPreprocessorService;
                _externalPreprocessorService = externalPreprocessorService;
                _concurrencySettings = concurrencySettings.Value;
                _dbContext = dbContext;
                _alreadyProcessedResources = new ConcurrentBag<string>();
                _fileResponses = new Dictionary<int, List<FileResponse>>();
                _fileService = fileServices(options.Value.FileMode);
                _ingestionLogLevels = LogLevels.GetValidList(options.Value.MinimumLevelIngestionLog);
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                List<Agent> enabledAgentsWithFilesToProcess;
                var apiServerId = request.OdsApi.Config.ApiServerId;
                var mappingLookups = await _dbContext.Lookups.ToArrayAsync(cancellationToken);

                _mappingLookups = new LookupCollection(mappingLookups);

                enabledAgentsWithFilesToProcess = await _dbContext.Agents
                    .Include(x => x.ApiServer)
                    .ThenInclude(x => x.ApiVersion)
                    .Include(x => x.DataMapAgents)
                    .ThenInclude(y => y.DataMap)
                    .ThenInclude(d => d.FileProcessorScript)
                    .Include(x => x.RowProcessor)
                    .Where(agent => agent.ApiServerId == apiServerId && agent.Enabled && agent.Archived == false && agent.Files.Any(file => file.Status == FileStatus.Uploaded || file.Status == FileStatus.Retry))
                    .OrderBy(agent => agent.RunOrder == null)
                    .ThenBy(agent => agent.RunOrder)
                    .ThenBy(agent => agent.Id)
                    .ToListAsync(cancellationToken);

                if (!enabledAgentsWithFilesToProcess.Any())
                {
                    _logger.LogInformation("No files found for processing for API Connection '{connection}'. If you expect a file to be processed, check the file status and ensure the agent is enabled.", request.OdsApi.Config.Name);
                    return;
                }

                if (request.CheckMetadata)
                {
                    foreach (var agent in enabledAgentsWithFilesToProcess)
                    {
                        foreach (var dataMap in agent.DataMapAgents.Select(x => x.DataMap).ToList())
                            ThrowIfMetadataIsIncompatible(dataMap);
                    }
                }

                foreach (var agent in enabledAgentsWithFilesToProcess)
                {
                    if (!agent.DataMapAgents.Any())
                    {
                        _logger.LogError("The '{Agent}' Agent is active with files to process, but has no Data Maps " +
                                  "associated with it. Because there is no mapping work to perform, the Agent cannot " +
                                  "process files. Because an active agent cannot process files, no work will be performed. " +
                                  "Correct the definition of this Agent by associating it with a Data Map.", agent.Name);
                        return;
                    }
                }

                foreach (var agent in enabledAgentsWithFilesToProcess)
                {
                    _fileResponses.Clear();

                    List<File> readyToProcessFiles;
                    readyToProcessFiles = _dbContext.Files.Where(file =>
                            file.AgentId == agent.Id &&
                            (file.Status == FileStatus.Uploaded || file.Status == FileStatus.Retry))
                        .ToList();

                    var mapsToProcessWith =
                        agent.DataMapAgents.OrderBy(x => x.ProcessingOrder).Select(x => x.DataMap).ToList();

                    foreach (var dataMap in mapsToProcessWith)
                    {
                        foreach (var file in readyToProcessFiles)
                        {
                            _alreadyProcessedResources.Clear();

                            _logger.LogInformation("Processing file: {file}. URL: {ulr}. DataMap: {datamap}", file.FileName, file.Url, dataMap.Name);

                            await UpdateStatus(file.Id, FileStatus.Transforming);

                            await TransformAndProcessEachRowUsingDataMap(file, dataMap, request.OdsApi, agent);
                        }
                    }

                    foreach (var fileResponse in _fileResponses)
                    {
                        var status = fileResponse.Value.Any(x => x.Errors > 0)
                            ? FileStatus.ErrorLoading
                            : FileStatus.Loaded;

                        var message = string.Join(NewLine + NewLine, fileResponse.Value.Select(x => x.Message));

                        await UpdateStatus(fileResponse.Key, status, message);
                    }

                    agent.LastExecuted = DateTimeOffset.Now;

                    DataImportCacheManager.DestroyCache(agent.Id.ToString(CultureInfo.InvariantCulture));
                }
            }

            private async Task TransformAndProcessEachRowUsingDataMap(File file, DataMap dataMap, IOdsApi odsApi, Agent agent)
            {
                string tempCsvFilePath = null;
                var successCount = 0;
                var existsCount = 0;
                var errorCount = 0;
                var ingestionLogs = new ConcurrentBag<IngestionLogMarker>();
                try
                {
                    var (downloadedFilePath, importRows) = await GetRowsToImport(file, agent, dataMap);
                    tempCsvFilePath = downloadedFilePath;
                    var parallelOptions = new ParallelOptions();
                    if (_concurrencySettings.LimitConcurrentApiPosts)
                        parallelOptions.MaxDegreeOfParallelism = _concurrencySettings.MaxConcurrentApiPosts;

                    await Parallel.ForEachAsync(importRows, parallelOptions, async (row, token) =>
                    {
                        if (row.Number == 1)
                            ValidateHeadersAndLookUps(file, dataMap, row.Content.Keys);

                        _logger.LogDebug("Transforming {path} row {row}", dataMap.ResourcePath, row.Number);
                        var (rowPostResponse, log) = await MapAndProcessCsvRow(odsApi, row.Content, dataMap, row.Number, file);

                        /// Tenant and Context are valid for Ods Api 7.x only. These values are null for other versions.
                        log.Tenant = agent.ApiServer.Tenant;
                        log.Context = agent.ApiServer.Context;

                        if (log != null && _ingestionLogLevels.Contains(log.Level))
                            WriteLog(log);

                        if (rowPostResponse == RowResult.Success)
                            Interlocked.Add(ref successCount, 1);
                        else if (rowPostResponse == RowResult.Exist)
                            Interlocked.Add(ref existsCount, 1);
                        else if (rowPostResponse == RowResult.Error)
                            Interlocked.Add(ref errorCount, 1);
                    });

                    _logger.LogInformation("Finished processing file: {file}. URL: {url}. DataMap: {datamap}", file.FileName, file.Url, dataMap.Name);

                    var fileResponse = new FileResponse
                    {
                        Success = successCount,
                        Exists = existsCount,
                        Errors = errorCount,
                        Message = $"Using DataMap: {dataMap.Name}, " +
                                  $"File has {file.Rows} rows, API calls processed:  " +
                                  $"Success: {successCount}, " +
                                  $"Exists: {existsCount}, " +
                                  $"Error: {errorCount}",
                    };

                    AddOrUpdateFileResponses(file, fileResponse);
                }
                catch (Exception ex)
                {
                    if (ex.GetType().Name == "DirectoryNotFoundException")
                    {
                        var errorMessage = "File or folder not found";
                        _logger.LogError("Error processing file: {File}. URL: {Url}. DataMap: {Datamap}. Details: {Details}.", file.FileName, file.Url, dataMap.Name, errorMessage);
                    }
                    else
                    {
                        _logger.LogError(ex, "Error processing file: {File}. URL: {Url}. DataMap: {Datamap}", file.FileName, file.Url, dataMap.Name);
                    }
                    var fileResponse = new FileResponse
                    {
                        Success = successCount,
                        Exists = existsCount,
                        Errors = errorCount + 1,
                        Message = $"Using DataMap: {dataMap.Name}, " +
                                  "Exception logged during file processing, " +
                                  $"File has {file.Rows} rows, API calls processed:  " +
                                  $"Success: {successCount}, " +
                                  $"Exists: {existsCount}, " +
                                  $"Error: {errorCount + 1}",
                    };

                    AddOrUpdateFileResponses(file, fileResponse);
                }
                finally
                {
                    if (tempCsvFilePath != null)
                        System.IO.File.Delete(tempCsvFilePath);
                }
            }

            private ITabularData GetTabularData(string tempCsvFilePath, Script rowProcessorScript, ApiServer apiServer, int agentId, DataMap dataMap, string fileFileName)
            {
                Script preprocessor = dataMap.FileProcessorScript;
                CsvTabularData csvTabularData;
                if (preprocessor != null)
                {
                    using (var fileStream = System.IO.File.OpenRead(tempCsvFilePath))
                    {
                        var output = preprocessor.ScriptType switch
                        {
                            ScriptType.CustomFileProcessor => _powerShellPreprocessorService.ProcessStreamWithScript(preprocessor.ScriptContent, fileStream,
                                CreateOptionsForPreprocessor(preprocessor, apiServer, agentId, dataMap.Attribute, fileFileName)),
                            ScriptType.ExternalFileProcessor => _externalPreprocessorService.ProcessStreamWithExternalProcess(preprocessor.ExecutablePath, preprocessor.ExecutableArguments, fileStream),
                            _ => throw new NotImplementedException($"File Processing for for script type {preprocessor.ScriptType} is invalid or not implemented.")
                        };

                        csvTabularData = new CsvTabularData(output);
                    }
                }
                else
                {
                    csvTabularData = new CsvTabularData(tempCsvFilePath);
                }

                if (rowProcessorScript != null)
                {
                    var options = CreateOptionsForPreprocessor(rowProcessorScript, apiServer, agentId, dataMap.Attribute, fileFileName);
                    return new PowershellTabularData(_powerShellPreprocessorService, rowProcessorScript.ScriptContent, csvTabularData, options);
                }

                return csvTabularData;
            }

            private ProcessOptions CreateOptionsForPreprocessor(Script preprocessor, ApiServer apiServer, int agentId, string dataMapAttribute, string fileName)
            {
                var options = new ProcessOptions
                {
                    RequiresOdsConnection = preprocessor.RequireOdsApiAccess,
                    OdsConnectionSettings = apiServer,
                    IsDataMapPreview = false,
                    CacheIdentifier = agentId.ToString(CultureInfo.InvariantCulture),
                    MapAttribute = preprocessor.HasAttribute ? dataMapAttribute : null,
                    FileName = fileName,
                    UsePowerShellWithNoRestrictions = preprocessor.ShouldRunPowerShellWithNoRestrictions()
                };

                options.ProcessMessageLogged += Options_ProcessMessageLogged;

                return options;
            }

            private void Options_ProcessMessageLogged(object sender, ProcessMessageEventArgs e)
            {
                _logger.Log(e.Level, e.Message);
            }

            private void ValidateHeadersAndLookUps(File file, DataMap dataMap, IEnumerable<string> headers)
            {
                var mappedColumns = dataMap.ReferencedColumns();
                var mappedLookups = dataMap.ReferencedLookups();
                var unmappedColumns = mappedColumns.Except(headers).ToList();
                var missingLookUps = mappedLookups.Except(_mappingLookups.SourceTables()).ToList();
                var validationError = string.Empty;
                var fileName = file.FileName;

                if (unmappedColumns.Any())
                {
                    validationError = $"File \'{fileName}\' can not be processed using data map \'{dataMap.Name}\', " +
                                      $"input file missing columns: {string.Join(", ", unmappedColumns)}.";
                }

                if (missingLookUps.Any())
                {
                    validationError += NewLine + $"File \'{fileName}\' can not be processed using data map \'{dataMap.Name}\', " +
                                      $"missing lookups: {string.Join(", ", missingLookUps)}.";
                }

                if (!string.IsNullOrEmpty(validationError))
                {
                    var fileResponse = new FileResponse
                    {
                        Errors = 1,
                        Message = validationError
                    };

                    AddOrUpdateFileResponses(file, fileResponse);
                    throw new Exception(validationError);
                }
            }

            private void ThrowIfMetadataIsIncompatible(DataMap dataMap)
            {
                if (dataMap.MetadataIsIncompatible(_dbContext, out string errorDetails))
                {
                    throw new Exception(
                        $"Cannot insert data for Data Map ID {dataMap.Id} because its " +
                        $"'{dataMap.ResourcePath}' resource metadata differs from that " +
                        "of the target ODS API. The Data Map may need to be redefined " +
                        $"against this ODS API version. {errorDetails}");
                }
            }

            private Task<(RowResult, IngestionLogMarker)> MapAndProcessCsvRow(IOdsApi odsApi, Dictionary<string, string> currentRow, DataMap map, int rowNum, File file)
            {
                MappedResource mappedRow = null;

                try
                {
                    mappedRow = TransformCsvRow(map, currentRow, rowNum, file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error transforming {map.ResourcePath} row {rowNum}");
                    var rowWithError = mappedRow ?? new MappedResource()
                    {
                        ResourcePath = map?.ResourcePath,
                        Metadata = map?.Metadata,
                        AgentId = file.AgentId,
                        AgentName = file.Agent?.Name,
                        ApiServerName = file.Agent?.ApiServer?.Name,
                        ApiVersion = file.Agent?.ApiServer?.ApiVersion?.Version.ToString(),
                        FileName = file.FileName,
                        RowNumber = rowNum,
                        EducationOrganizationId = GetEdOrgId(currentRow, map.SelectedIngestionLogEdOrgIdColumn)
                    };
                    return Task.FromResult((RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, rowWithError, $"{odsApi.Config.ApiUrl}{map.ResourcePath}", null, ex.Message)));
                }

                return mappedRow == null
                    ? Task.FromResult((RowResult.Error, (IngestionLogMarker) null))
                    : map.IsDeleteOperation
                        ? map.IsDeleteByNaturalKey
                            ? DeleteMappedRowByNaturalKey(odsApi, mappedRow, map.ResourcePath)
                            : DeleteMappedRowById(odsApi, mappedRow, map.ResourcePath)
                        : PostMappedRow(odsApi, mappedRow, map.ResourcePath);
            }

            private MappedResource TransformCsvRow(DataMap dataMap, Dictionary<string, string> currentRow, int rowNum, File file)
            {
                var resourceMapper = new ResourceMapper(_logger, dataMap, _mappingLookups);

                var mappedRowJson = dataMap.IsDeleteOperation && !dataMap.IsDeleteByNaturalKey
                    ? resourceMapper.ApplyMapForDeleteByIdOperation(currentRow)
                    : resourceMapper.ApplyMap(currentRow);

                return new MappedResource
                {
                    ResourcePath = dataMap.ResourcePath,
                    Metadata = dataMap.Metadata,
                    Value = mappedRowJson,
                    AgentId = file.AgentId,
                    AgentName = file.Agent?.Name,
                    ApiServerName = file.Agent?.ApiServer?.Name,
                    ApiVersion = file.Agent?.ApiServer?.ApiVersion?.Version.ToString(),
                    FileName = file.FileName,
                    RowNumber = rowNum,
                    EducationOrganizationId = GetEdOrgId(currentRow, dataMap.SelectedIngestionLogEdOrgIdColumn, mappedRowJson)
                };
            }

            private int? GetEdOrgId(Dictionary<string, string> currentRow, string selectedIngestionLogEdOrgIdColumn, JToken mappedRowJson = null)
            {
                int? edOrgId = null;
                if (mappedRowJson != null)
                    edOrgId = Helper.GetEdOrgIdFromJsonTransformed(mappedRowJson);

                if (!edOrgId.HasValue)
                    edOrgId = Helper.GetEdOrgIdFromCsv(currentRow, selectedIngestionLogEdOrgIdColumn);

                return edOrgId;
            }

            private async Task<(RowResult Error, IngestionLogMarker)> PostMappedRow(IOdsApi odsApi, MappedResource mappedRow, string resourcePath)
            {
                var endpointUrl = $"{odsApi.Config.ApiUrl.TrimEnd('/')}{resourcePath}";

                if (RowHasAlreadyBeenProcessed(mappedRow, endpointUrl))
                    return (RowResult.Duplicate, null);

                var postInfo = $"{mappedRow.ResourcePath} row {mappedRow.RowNumber}";

                _logger.LogDebug("Posting {post}", postInfo);

                OdsResponse odsResponse;
                try
                {
                    odsResponse = await odsApi.Post(mappedRow.Value.ToString(), endpointUrl, postInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "POST failed for resource: {url}, Row Number: {row}", endpointUrl, mappedRow.RowNumber);
                    return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl));
                }

                switch (odsResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return (RowResult.Exist, new IngestionLogMarker(IngestionResult.Success, LogLevels.Information, mappedRow, endpointUrl, null, null, odsResponse.StatusCode));
                    case HttpStatusCode.Created:
                        return (RowResult.Success, new IngestionLogMarker(IngestionResult.Success, LogLevels.Information, mappedRow, endpointUrl, null, null, odsResponse.StatusCode));
                    default:
                        _logger.LogError($"POST returned unexpected HTTP status: {endpointUrl}, Row Number: {mappedRow.RowNumber}, Status: {odsResponse.StatusCode}, Error: {odsResponse.Content}");
                        return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl, null, null, odsResponse.StatusCode, odsResponse.Content));
                }
            }

            private async Task<(RowResult Error, IngestionLogMarker)> DeleteMappedRowById(IOdsApi odsApi, MappedResource mappedRow, string resourcePath)
            {
                var endpointUrl = $"{odsApi.Config.ApiUrl.TrimEnd('/')}{resourcePath}";

                if (RowHasAlreadyBeenProcessed(mappedRow, endpointUrl))
                    return (RowResult.Duplicate, null);

                var id = mappedRow.Value.SelectToken("Id").Value<string>();

                _logger.LogDebug("Deleting {id}", id);

                OdsResponse odsResponse;
                try
                {
                    odsResponse = await odsApi.Delete(id, endpointUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "POST failed for resource: {url}, Row Number: {row}", endpointUrl, mappedRow.RowNumber);
                    return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl));
                }

                switch (odsResponse.StatusCode)
                {
                    case HttpStatusCode.NoContent:
                        return (RowResult.Success, new IngestionLogMarker(IngestionResult.Success, LogLevels.Information, mappedRow, endpointUrl, null, null, odsResponse.StatusCode));
                    default:
                        _logger.LogError($"DELETE returned unexpected HTTP status: {endpointUrl}, Row Number: {mappedRow.RowNumber}, Status: {odsResponse.StatusCode}, Error: {odsResponse.Content}");
                        return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl, null, null, odsResponse.StatusCode, odsResponse.Content));
                }
            }

            private async Task<(RowResult Error, IngestionLogMarker)> DeleteMappedRowByNaturalKey(IOdsApi odsApi, MappedResource mappedRow, string resourcePath)
            {
                var endpointUrl = $"{odsApi.Config.ApiUrl.TrimEnd('/')}{resourcePath}";

                if (RowHasAlreadyBeenProcessed(mappedRow, endpointUrl))
                    return (RowResult.Duplicate, null);

                var deleteInfo = $"{mappedRow.ResourcePath} row {mappedRow.RowNumber}";

                _logger.LogDebug("Deleting {post}", deleteInfo);

                OdsResponse odsResponse;
                try
                {
                    odsResponse = await odsApi.PostAndDelete(mappedRow.Value.ToString(), endpointUrl, deleteInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "POST and DELETE failed for resource: {url}, Row Number: {row}", endpointUrl, mappedRow.RowNumber);
                    return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl));
                }

                switch (odsResponse.StatusCode)
                {
                    case HttpStatusCode.NoContent:
                        return (RowResult.Success, new IngestionLogMarker(IngestionResult.Success, LogLevels.Information, mappedRow, endpointUrl, null, null, odsResponse.StatusCode));
                    default:
                        _logger.LogError($"DELETE returned unexpected HTTP status: {endpointUrl}, Row Number: {mappedRow.RowNumber}, Status: {odsResponse.StatusCode}, Error: {odsResponse.Content}");
                        return (RowResult.Error, new IngestionLogMarker(IngestionResult.Error, LogLevels.Error, mappedRow, endpointUrl, null, null, odsResponse.StatusCode, odsResponse.Content));
                }
            }

            private bool RowHasAlreadyBeenProcessed(MappedResource mappedResource, string endPoint)
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("Endpoint: " + endPoint);
                strBuilder.AppendLine(mappedResource.Value.ToString());

                using (var md5Algorithm = MD5.Create())
                {
                    var utf32Bytes = Encoding.UTF32.GetBytes(strBuilder.ToString());
                    var computedHash = md5Algorithm.ComputeHash(utf32Bytes);
                    var base64ResultingHash = Convert.ToBase64String(computedHash);

                    if (_alreadyProcessedResources.Contains(base64ResultingHash))
                        return true;

                    _alreadyProcessedResources.Add(base64ResultingHash);
                    return false;
                }
            }

            private void AddOrUpdateFileResponses(File file, FileResponse fileResponse)
            {
                if (_fileResponses.ContainsKey(file.Id))
                    _fileResponses[file.Id].Add(fileResponse);
                else
                    _fileResponses[file.Id] = new List<FileResponse> { fileResponse };
            }

            private async Task UpdateStatus(int fileId, FileStatus status, string message = null)
            {
                var file = _dbContext.Files.Include(file => file.Agent).Single(x => x.Id == fileId);

                file.Status = status;
                file.UpdateDate = DateTimeOffset.Now;

                if (message != null)
                    file.Message = message;

                switch (file.Status)
                {
                    case FileStatus.Loaded:
                        if (file.Agent.GetActionFileCode() == AgentActionsFile.DeleteOnSuccessful || file.Agent.GetActionFileCode() == AgentActionsFile.AlwaysDelete)
                            await _fileService.Delete(file);
                        break;
                    default:
                        if (file.Status.CanBeRetried() && file.Agent.GetActionFileCode() == AgentActionsFile.AlwaysDelete)
                        {
                            await _fileService.Delete(file);
                            file.Status = FileStatus.Canceled;
                        }
                        break;

                }
                await _dbContext.SaveChangesAsync();
            }

            private void WriteLog(IngestionLogMarker marker)
            {
                var model = new IngestionLog
                {
                    Date = marker.Date,
                    Result = marker.Result,
                    RowNumber = marker.MappedResource?.RowNumber.ToString(),
                    EndPointUrl = marker.EndpointUrl,
                    HttpStatusCode = marker.StatusCode?.ToString(),
                    Data = marker.MappedResource?.Value?.ToString(),
                    OdsResponse = marker.OdsResponse,
                    Level = marker.Level,
                    Operation = "TransformingData",
                    Process = "DataImport.Server.TransformLoad",
                    FileName = marker.MappedResource?.FileName,
                    AgentName = marker.MappedResource?.AgentName,
                    ApiServerName = marker.MappedResource?.ApiServerName,
                    ApiVersion = marker.MappedResource?.ApiVersion,
                    Tenant = marker.Tenant,
                    Context = marker.Context,
                    EducationOrganizationId = marker.MappedResource?.EducationOrganizationId,
                };
                _logger.LogToTable($"Writing in IngestionLog for row: {model.RowNumber}", model, "IngestionLog");
            }

            private async Task<(string, IEnumerable<ImportRow>)> GetRowsToImport(File file, Agent agent, DataMap dataMap)
            {
                var tempDownloadedPath = await _fileService.Download(file);
                var rowProcessorScript = agent.RowProcessorScriptId.HasValue ? agent.RowProcessor : null;

                using var table = GetTabularData(tempDownloadedPath, rowProcessorScript, agent.ApiServer, agent.Id, dataMap, file.FileName);

                var importRows = table.GetRows()
                    .Select((row, index) => new ImportRow { Number = index + 1, Content = row })
                    .ToList();

                return (tempDownloadedPath, importRows);
            }
        }

        public class ImportRow
        {
            public int Number { get; set; }
            public Dictionary<string, string> Content { get; set; }
        }

        public class IngestionLogMarker
        {
            public IngestionResult Result { get; }
            public string Level { get; }
            public MappedResource MappedResource { get; }
            public string EndpointUrl { get; }
            public HttpStatusCode? StatusCode { get; }
            public string OdsResponse { get; }
            public DateTimeOffset Date { get; }
            public string Tenant { get; set; }
            public string Context { get; set; }

            public IngestionLogMarker(IngestionResult result, string level, MappedResource mappedResource, string endpointUrl, string tenant = null, string context = null, HttpStatusCode? statusCode = null, string odsResponse = null)
            {
                Result = result;
                Level = level;
                MappedResource = mappedResource;
                EndpointUrl = endpointUrl;
                StatusCode = statusCode;
                OdsResponse = odsResponse;
                Date = DateTimeOffset.Now;
                Tenant = tenant;
                Context = context;
            }
        }
    }
}
