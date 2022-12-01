// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using DataImport.Web.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace DataImport.Web.Features.Agent
{
    public class AddAgent
    {
        public class Query : IRequest<AddEditAgentViewModel>
        {

        }

        public class QueryHandler : RequestHandler<Query, AddEditAgentViewModel>
        {
            private readonly AgentSelectListProvider _agentSelectListProvider;

            public QueryHandler(AgentSelectListProvider agentSelectListProvider)
            {
                _agentSelectListProvider = agentSelectListProvider;
            }

            protected override AddEditAgentViewModel Handle(Query request)
            {
                return new AddEditAgentViewModel
                {
                    DataMaps = _agentSelectListProvider.GetDataMapList(),
                    AgentTypes = _agentSelectListProvider.GetAgentTypes(),
                    RowProcessors = _agentSelectListProvider.GetRowProcessors(),
                    FileGenerators = _agentSelectListProvider.GetFileGenerators(),
                    BootstrapDatas = _agentSelectListProvider.GetBootstrapDataList(),
                    Enabled = true,
                };
            }
        }

        public class Response : ToastResponse
        {
            public int AgentId { get; set; }
        }

        public class Command : IRequest<Response>
        {
            public AddEditAgentViewModel ViewModel { get; set; }
        }

        public class CommandHandler : RequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IEncryptionService _encryptionService;
            private readonly string _encryptionKey;

            public CommandHandler(ILogger<AddAgent> logger, DataImportDbContext dataImportDbContext, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService)
            {
                _logger = logger;
                _dataImportDbContext = dataImportDbContext;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _encryptionService = encryptionService;
            }

            protected override Response Handle(Command request)
            {
                var viewmodel = request.ViewModel;

                var agent = new DataImport.Models.Agent
                {
                    Name = viewmodel.Name,
                    AgentTypeCode = viewmodel.AgentTypeCode,
                    Url = viewmodel.Url,
                    Port = viewmodel.Port,
                    RunOrder = viewmodel.RunOrder,
                    Username = viewmodel.Username,
                    FilePattern = viewmodel.FilePattern,
                    Enabled = viewmodel.Enabled,
                    Created = DateTimeOffset.Now,
                    Directory = viewmodel.Directory,
                    RowProcessorScriptId = viewmodel.AgentTypeCode != AgentTypeCodeEnum.PowerShell ? viewmodel.RowProcessorId : null,
                    FileGeneratorScriptId = viewmodel.AgentTypeCode == AgentTypeCodeEnum.PowerShell ? viewmodel.FileGeneratorId : null,
                    ApiServerId = viewmodel.ApiServerId
                };

                foreach (var dataMap in viewmodel.DdlDataMaps.Select(JsonConvert.DeserializeObject<MappedAgent>))
                {
                    agent.DataMapAgents.Add(new DataMapAgent
                    {
                        DataMapId = dataMap.DataMapId,
                        ProcessingOrder = dataMap.ProcessingOrder
                    });
                }

                foreach (var schedule in viewmodel.DdlSchedules.Select(JsonConvert.DeserializeObject<AgentSchedule>))
                {
                    agent.AgentSchedules.Add(schedule);
                }

                foreach (var agentBootstrapData in viewmodel.DdlBootstrapDatas
                    .Select(JsonConvert.DeserializeObject<AgentBootstrapData>)
                    .GroupBy(a => a.BootstrapDataId)
                    .Select(x => x.First()))
                {
                    agent.BootstrapDataAgents.Add(new BootstrapDataAgent
                    {
                        BootstrapDataId = agentBootstrapData.BootstrapDataId,
                        ProcessingOrder = agentBootstrapData.ProcessingOrder,
                        Agent = agent
                    });
                }

                if (viewmodel.Password != null)
                {
                    agent.Password = _encryptionService.TryEncrypt(viewmodel.Password, _encryptionKey, out var encryptedKey) ? encryptedKey : string.Empty;
                }

                _dataImportDbContext.Agents.Add(agent);
                _dataImportDbContext.SaveChanges();

                _logger.Added(agent, a => a.Name);

                return new Response
                {
                    AgentId = agent.Id,
                    Message = $"Agent '{agent.Name}' was created."
                };
            }
        }
    }
}
