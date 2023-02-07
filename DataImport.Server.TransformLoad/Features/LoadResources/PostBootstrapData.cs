// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Common.ExtensionMethods;
using DataImport.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class PostBootstrapData
    {
        public class Command : IRequest<Response>
        {
            public bool CheckMetadata { get; set; } = true;

            public IOdsApi OdsApi { get; set; }
        }

        public class Response
        {
            public bool Success { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger<PostBootstrapData> _logger;
            private readonly DataImportDbContext _dbContext;

            public CommandHandler(ILogger<PostBootstrapData> logger, DataImportDbContext dbContext)
            {
                _logger = logger;
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    await InsertBootstrapData(request.OdsApi, request.CheckMetadata);
                    return new Response { Success = true };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading bootstrap data. Data import cannot proceed.");
                    return new Response { Success = false };
                }
            }

            private async Task InsertBootstrapData(IOdsApi ods, bool checkMetadata)
            {
                var apiServerId = ods.Config.ApiServerId;
                var bootstrapDatas = _dbContext.BootstrapDatas
                    .Where(p => p.BootstrapDataAgents.Any(y => y.Agent.ApiServerId == apiServerId && !y.Agent.Archived && y.Agent.Enabled))
                    .Include(p => p.BootstrapDataApiServers)
                    .Include(p => p.BootstrapDataAgents)
                    .ThenInclude(p => p.Agent)
                    .ThenInclude(a => a.ApiServer)
                    .ThenInclude(s => s.ApiVersion)
                    .AsSingleQuery()
                    .ToList();

                var bootstrapPayloads = bootstrapDatas
                    .Where(b =>
                    {
                        var bootstrapDataApiServer =
                            b.BootstrapDataApiServers.SingleOrDefault(x => x.ApiServerId == apiServerId);
                        return bootstrapDataApiServer == null || (b.UpdateDate.HasValue &&
                                                                  b.UpdateDate.Value >
                                                                  bootstrapDataApiServer.ProcessedDate);
                    })
                    .OrderBy(x => x.BootstrapDataAgents.Min(y => y.ProcessingOrder)).ToList();


                foreach (var singlePayload in bootstrapPayloads)
                {
                    string errorDetails;
                    if (checkMetadata && singlePayload.MetadataIsIncompatible(_dbContext, out errorDetails))
                    {
                        throw new Exception(
                            $"Cannot insert bootstrap data for ID {singlePayload.Id} because its " +
                            $"'{singlePayload.ResourcePath}' resource metadata differs from that " +
                            "of the target ODS API. The bootstrap data may need to be redefined " +
                            $"against this ODS API version. {errorDetails}");
                    }
                }

                foreach (var singlePayload in bootstrapPayloads)
                {
                    _logger.LogInformation("Inserting bootstrap data for ID: {id}", singlePayload.Id);

                    var resourcePath = singlePayload.ResourcePath;
                    var endpointUrl = $"{ods.Config.ApiUrl}{resourcePath}";

                    var convertedPayload = JToken.Parse(singlePayload.Data);

                    if (convertedPayload.Type == JTokenType.Array)
                    {
                        // There are multiple payloads as part of an array. Post each payload.
                        foreach (var singleElement in (JArray) convertedPayload)
                        {
                            var dataToInsert = singleElement.ToString();
                            OdsResponse response = null;
                            try
                            {
                                response = await ods.PostBootstrapData(endpointUrl, dataToInsert);
                                if (!response.StatusCode.IsSuccessStatusCode())
                                {
                                    throw new Exception("Failed to POST bootstrap data. HTTP Status Code: " +
                                                        response.StatusCode);
                                }
                            }
                            catch
                            {
                                LogIngestion(IngestionResult.Error, Common.Constants.IngestionLogLevel.Error, singlePayload, endpointUrl,
                                    response?.StatusCode, response?.Content, dataToInsert);
                                throw;
                            }
                        }
                    }
                    else // Post the single payload
                    {
                        OdsResponse response = null;
                        try
                        {
                            response = await ods.PostBootstrapData(endpointUrl, singlePayload.Data);
                            if (!response.StatusCode.IsSuccessStatusCode())
                            {
                                throw new Exception("Failed to POST bootstrap data. HTTP Status Code: " +
                                                    response.StatusCode);
                            }
                        }
                        catch
                        {
                            LogIngestion(IngestionResult.Error, Common.Constants.IngestionLogLevel.Error, singlePayload, endpointUrl,
                                response?.StatusCode, response?.Content);
                            throw;
                        }

                        LogIngestion(IngestionResult.Success, Common.Constants.IngestionLogLevel.Information, singlePayload, endpointUrl);

                        // After updating the payload, stamp it so it doesn't run again in next cycle (unless it has been updated).
                        var bootstrapDataApiServer =
                            singlePayload.BootstrapDataApiServers.SingleOrDefault(x => x.ApiServerId == apiServerId);
                        if (bootstrapDataApiServer == null)
                        {
                            bootstrapDataApiServer = new BootstrapDataApiServer
                            {
                                ApiServerId = apiServerId
                            };
                            singlePayload.BootstrapDataApiServers.Add(bootstrapDataApiServer);
                        }

                        bootstrapDataApiServer.ProcessedDate = DateTimeOffset.Now;

                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            private void LogIngestion(IngestionResult result, string level, BootstrapData bootstrapData, string endpointUrl, HttpStatusCode? statusCode = null, string odsResponse = null, string dataOverride = null)
            {
                var agents = bootstrapData.BootstrapDataAgents.Select(a => a.Agent).ToList();

                var ingestionLog = new IngestionLog
                {
                    Date = DateTimeOffset.Now,
                    Result = result,
                    RowNumber = "0",
                    EndPointUrl = endpointUrl,
                    HttpStatusCode = statusCode?.ToString(),
                    Data = dataOverride ?? bootstrapData.Data,
                    OdsResponse = odsResponse,
                    Level = level,
                    Operation = "PostBootstrapData",
                    Process = "DataImport.Server.TransformLoad",
                    FileName = $"Bootstrap: {bootstrapData.Name}",
                    AgentName = string.Join(", ", agents.Select(x => x.Name)),
                    ApiServerName = string.Join(", ", agents.Select(x => x.ApiServer.Name)),
                    ApiVersion = string.Join(", ", agents.Select(x => x.ApiServer.ApiVersion.Version)),
                };

                _dbContext.IngestionLogs.Add(ingestionLog);

                _dbContext.SaveChanges();
            }
        }
    }
}
