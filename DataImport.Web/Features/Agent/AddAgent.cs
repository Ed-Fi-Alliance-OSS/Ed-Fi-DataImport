// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using DataImport.Web.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Agent
{
    public class AddAgent
    {
        public class Query : IRequest<AddEditAgentViewModel>
        {

        }

        public class QueryHandler : IRequestHandler<Query, AddEditAgentViewModel>
        {
            private readonly AgentSelectListProvider _agentSelectListProvider;
            private readonly FilesActionSelectListProvider _filesActionSelectListProvider;

            public QueryHandler(AgentSelectListProvider agentSelectListProvider, FilesActionSelectListProvider filesActionSelectListProvider)
            {
                _agentSelectListProvider = agentSelectListProvider;
                _filesActionSelectListProvider = filesActionSelectListProvider;
            }

            public Task<AddEditAgentViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AddEditAgentViewModel
                {
                    DataMaps = _agentSelectListProvider.GetDataMapList(),
                    AgentTypes = _agentSelectListProvider.GetAgentTypes(),
                    RowProcessors = _agentSelectListProvider.GetRowProcessors(),
                    FileGenerators = _agentSelectListProvider.GetFileGenerators(),
                    BootstrapDatas = _agentSelectListProvider.GetBootstrapDataList(),
                    ActionFiles = _filesActionSelectListProvider.GetSelectListItems(),
                    Enabled = true,
                });
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

        public class CommandHandler : IRequestHandler<Command, Response>
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

            public Task<Response> Handle(Command request, CancellationToken cancellationToken)
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
                    ApiServerId = viewmodel.ApiServerId,
                    ActionFileCode = viewmodel.AgentTypeCode is AgentTypeCodeEnum.Sftp or AgentTypeCodeEnum.Ftps ? viewmodel.ActionFileCode : null,
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

                return Task.FromResult(new Response
                {
                    AgentId = agent.Id,
                    Message = $"Agent '{agent.Name}' was created."
                });
            }
        }
    }
}
