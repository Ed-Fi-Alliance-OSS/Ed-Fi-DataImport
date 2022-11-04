// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CsvHelper;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DataImport.Web.Features.DataMaps
{
    public class UploadCsvFile
    {
        public class Command : IRequest<CsvData>
        {
            public IFormFile FileBase { get; set; }
            public int? PreprocessorId { get; set; }
            public int? ApiServerId { get; set; }
            public string Attribute { get; set; }
        }

        public class CommandHandler : RequestHandler<Command, CsvData>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dbContext;
            private readonly IPowerShellPreprocessorService _preprocessorService;
            private readonly IExternalPreprocessorService _externalPreprocessorService;
            private List<LogMessageViewModel> _preprocessorLogMessages;

            public CommandHandler(ILogger<UploadCsvFile> logger, DataImportDbContext dbContext, IPowerShellPreprocessorService preprocessorService, IExternalPreprocessorService externalPreprocessorService)
            {
                _logger = logger;
                _dbContext = dbContext;
                _preprocessorService = preprocessorService;
                _externalPreprocessorService = externalPreprocessorService;
            }

            protected override CsvData Handle(Command request)
            {
                var uploadCsvFile = request.FileBase;
                _preprocessorLogMessages = new List<LogMessageViewModel>();
                string csvError;
                string cacheKey = Guid.NewGuid().ToString();

                try
                {
                    if (uploadCsvFile.Length <= 0 && !request.PreprocessorId.HasValue) return new CsvData
                    {
                        CsvError = "File is empty."
                    };

                    Stream inputStream = GetInputStream(request, cacheKey);

                    var csvConfg = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        TrimOptions = CsvHelper.Configuration.TrimOptions.InsideQuotes,
                        MissingFieldFound = null
                    };

                    var csvDataTable = new DataTable();

                    const int linesToRead = 5;
                    var linesRead = 1;

                    string[] columnHeaders;

                    using (var streamReader = new StreamReader(inputStream))
                    {
                        using (var csvHelper = new CsvReader(streamReader, csvConfg))
                        {
                            csvHelper.Read();
                            csvHelper.ReadHeader();

                            columnHeaders = csvHelper.HeaderRecord;

                            csvDataTable.Columns.AddRange(columnHeaders.Select(x => new DataColumn(x)).ToArray());

                            csvHelper.Read();

                            do
                            {
                                var row = csvDataTable.NewRow();

                                foreach (DataColumn column in csvDataTable.Columns)
                                {
                                    if (csvHelper.TryGetField(column.DataType, column.ColumnName, out var field))
                                    {
                                        row[column.ColumnName] = field;
                                    }
                                }

                                csvDataTable.Rows.Add(row);

                                linesRead++;

                            } while (csvHelper.Read() && linesRead <= linesToRead);
                        }
                    }

                    return new CsvData
                    {
                        ColumnHeaders = columnHeaders,
                        TablePreview = csvDataTable,
                        PreprocessorLogMessages = _preprocessorLogMessages
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Uploading File in Data Mapper");
                    csvError = ex.Message;
                }
                finally
                {
                    DataImportCacheManager.DestroyCache(cacheKey);
                }

                return new CsvData
                {
                    PreprocessorLogMessages = _preprocessorLogMessages,
                    CsvError = csvError
                };
            }

            private Stream GetInputStream(Command request, string cacheKey)
            {
                if (!request.PreprocessorId.HasValue)
                {
                    return request.FileBase.OpenReadStream();
                }

                var preprocessor = _dbContext.Scripts.Single(x => x.Id == request.PreprocessorId.Value);
                if (preprocessor.ScriptType == ScriptType.CustomFileProcessor)
                {
                    var options = new ProcessOptions
                    {
                        OdsConnectionSettings = request.ApiServerId.HasValue ? _dbContext.ApiServers.Include(x => x.ApiVersion).Single(x => x.Id == request.ApiServerId.Value) : null,
                        RequiresOdsConnection = preprocessor.RequireOdsApiAccess,
                        IsDataMapPreview = true,
                        CacheIdentifier = cacheKey,
                        MapAttribute = preprocessor.HasAttribute ? request.Attribute : null,
                        FileName = request.FileBase.FileName,
                        UsePowerShellWithNoRestrictions = preprocessor.ShouldRunPowerShellWithNoRestrictions()
                    };

                    options.ProcessMessageLogged += Options_ProcessMessageLogged;

                    return _preprocessorService.ProcessStreamWithScript(preprocessor.ScriptContent, request.FileBase.OpenReadStream(), options);
                }
                if (preprocessor.ScriptType == ScriptType.ExternalFileProcessor)
                {
                    return _externalPreprocessorService.ProcessStreamWithExternalProcess(preprocessor.ExecutablePath, preprocessor.ExecutableArguments, request.FileBase.OpenReadStream());
                }
                throw new NotImplementedException($"File Processing for script type {preprocessor.ScriptType} is invalid or not yet implemented.");
            }

            private void Options_ProcessMessageLogged(object sender, ProcessMessageEventArgs e)
            {
                _preprocessorLogMessages.Add(new LogMessageViewModel
                {
                    Message = e.Message,
                    Level = e.Level
                });
            }
        }

        public class CsvData
        {
            public string[] ColumnHeaders { get; set; }
            public DataTable TablePreview { get; set; }
            public List<LogMessageViewModel> PreprocessorLogMessages { get; set; }
            public string CsvError { get; set; }
        }
    }
}