// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using DataImport.Web.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Agent
{
    public class EditAgent
    {
        public class Query : IRequest<AddEditAgentViewModel>
        {
            public int Id { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, AddEditAgentViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IMapper _mapper;
            private readonly AgentSelectListProvider _selectListProvider;
            private readonly IEncryptionService _encryptionService;
            private readonly string _encryptionKey;
            private readonly FilesActionSelectListProvider _filesActionProvider;
            public QueryHandler(DataImportDbContext dataImportDbContext, IEncryptionKeyResolver encryptionKeyResolver, IMapper mapper, AgentSelectListProvider selectListProvider, IEncryptionService encryptionService, FilesActionSelectListProvider filesActionProvider)
            {
                _dataImportDbContext = dataImportDbContext;
                _encryptionService = encryptionService;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _mapper = mapper;
                _selectListProvider = selectListProvider;
                _filesActionProvider = filesActionProvider;
            }

            public Task<AddEditAgentViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var agent = _dataImportDbContext.Agents
                    .Include(x => x.AgentSchedules)
                    .Include(x => x.DataMapAgents).ThenInclude(y => y.DataMap)
                    .Include(x => x.BootstrapDataAgents).ThenInclude(x => x.BootstrapData)
                    .FirstOrDefault(x => x.Id == request.Id);

                if (agent == null)
                    return Task.FromResult(new AddEditAgentViewModel());

                var vm = _mapper.Map<AddEditAgentViewModel>(agent);
                vm.EncryptionFailureMsg = null;
                if (!string.IsNullOrWhiteSpace(agent.Password))
                {
                    if (_encryptionService.TryDecrypt(agent.Password, _encryptionKey, out var decryptedValue))
                    {
                        vm.Password = decryptedValue;
                    }
                    else
                    {
                        vm.Password = string.Empty;
                        vm.EncryptionFailureMsg = Constants.AgentDecryptionError;
                    }
                }
                vm.ActionFileCode = vm.ActionFileCode ?? AgentActionsFile.DeleteOnSuccessful.ToString();
                vm.DataMaps = _selectListProvider.GetDataMapList();
                vm.AgentTypes = _selectListProvider.GetAgentTypes();
                vm.RowProcessors = _selectListProvider.GetRowProcessors();
                vm.FileGenerators = _selectListProvider.GetFileGenerators();
                vm.BootstrapDatas = _selectListProvider.GetBootstrapDataList();
                vm.ActionFiles = _filesActionProvider.GetSelectListItems();
                return Task.FromResult(vm);
            }
        }

        public class Command : IRequest<ToastResponse>
        {
            public AddEditAgentViewModel ViewModel { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger<EditAgent> _logger;
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IEncryptionService _encryptionService;
            private readonly string _encryptionKey;

            public CommandHandler(ILogger<EditAgent> logger, DataImportDbContext dataImportDbContext, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService)
            {
                _logger = logger;
                _dataImportDbContext = dataImportDbContext;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _encryptionService = encryptionService;
            }

            public Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var vm = request.ViewModel;

                var agent = _dataImportDbContext.Agents.Include(x => x.BootstrapDataAgents).Single(x => x.Id == vm.Id);

                agent.Name = vm.Name;
                agent.AgentTypeCode = vm.AgentTypeCode;
                agent.Url = vm.Url;
                agent.Port = vm.Port;
                agent.RunOrder = vm.RunOrder;
                agent.Username = vm.Username;
                agent.FilePattern = vm.FilePattern;
                agent.Enabled = vm.Enabled;
                agent.Password = !string.IsNullOrEmpty(vm.Password)
                    ? _encryptionService.TryEncrypt(vm.Password, _encryptionKey, out var encryptedKey) ? encryptedKey : string.Empty
                    : string.Empty;

                agent.Directory = vm.Directory;
                agent.RowProcessorScriptId = vm.AgentTypeCode != AgentTypeCodeEnum.PowerShell ? vm.RowProcessorId : null;
                agent.FileGeneratorScriptId = vm.AgentTypeCode == AgentTypeCodeEnum.PowerShell ? vm.FileGeneratorId : null;
                agent.ActionFileCode = vm.AgentTypeCode is AgentTypeCodeEnum.Sftp or AgentTypeCodeEnum.Ftps ? vm.ActionFileCode : null;
                agent.ApiServerId = vm.ApiServerId;

                foreach (var dataMapAgent in _dataImportDbContext.DataMapAgents.Where(x => x.AgentId == agent.Id))
                {
                    _dataImportDbContext.DataMapAgents.Remove(dataMapAgent);
                }

                foreach (var dataMap in vm.DdlDataMaps.Select(JsonConvert.DeserializeObject<MappedAgent>))
                {
                    agent.DataMapAgents.Add(new DataMapAgent
                    {
                        DataMapId = dataMap.DataMapId,
                        ProcessingOrder = dataMap.ProcessingOrder
                    });
                }

                foreach (var agentSchedule in _dataImportDbContext.AgentSchedules.Where(x => x.AgentId == agent.Id))
                {
                    _dataImportDbContext.AgentSchedules.Remove(agentSchedule);
                }

                foreach (var schedule in vm.DdlSchedules.Select(JsonConvert.DeserializeObject<AgentSchedule>))
                {
                    agent.AgentSchedules.Add(new AgentSchedule
                    {
                        Day = schedule.Day,
                        Hour = schedule.Hour,
                        Minute = schedule.Minute
                    });
                }

                _dataImportDbContext.BootstrapDataAgents.RemoveRange(agent.BootstrapDataAgents);

                foreach (var agentBootstrapData in vm.DdlBootstrapDatas
                    .Select(JsonConvert.DeserializeObject<AgentBootstrapData>)
                    .GroupBy(a => a.BootstrapDataId)
                    .Select(x => x.First()))
                {
                    agent.BootstrapDataAgents.Add(new BootstrapDataAgent
                    {
                        BootstrapDataId = agentBootstrapData.BootstrapDataId,
                        ProcessingOrder = agentBootstrapData.ProcessingOrder,
                        AgentId = request.ViewModel.Id
                    });
                }

                _logger.Modified(agent, a => a.Name);

                return Task.FromResult(new ToastResponse
                {
                    Message = $"Agent '{agent.Name}' was modified."
                });
            }
        }
    }
}
