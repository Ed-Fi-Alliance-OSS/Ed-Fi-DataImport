// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Helpers;
using DataImport.Web.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataImport.Web.Features.Agent
{
    public class AddEditAgentViewModel : IRequest, IApiServerListViewModel
    {
        public AddEditAgentViewModel()
        {
            DataMaps = new List<DropdownItem>();
            AgentTypes = new List<SelectListItem>();
            AgentSchedules = new List<Schedule>();
            MappedAgents = new List<MappedAgent>();
            BootstrapDatas = new List<AgentBootstrapDataDropdownItem>();
            AgentBootstrapDatas = new List<AgentBootstrapData>();
        }

        public int Id { get; set; }
        public bool Enabled { get; set; }
        [Display(Name = "File Pattern")]
        public string FilePattern { get; set; }
        public string Directory { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        [Display(Name = "Host Name")]
        public string Url { get; set; }
        public int? Port { get; set; }
        public string Name { get; set; }

        [Display(Name = "Agent Type")]
        public string AgentTypeCode { get; set; }

        [Display(Name = "Processor")]
        public int? RowProcessorId { get; set; }

        [Display(Name = "Generator")]
        public int? FileGeneratorId { get; set; }

        public List<string> DdlDataMaps { get; set; } = new List<string>();
        public List<string> DdlSchedules { get; set; } = new List<string>();
        public List<string> DdlBootstrapDatas { get; set; } = new List<string>();
        public List<SelectListItem> RowProcessors { get; set; }
        public List<SelectListItem> FileGenerators { get; set; }
        public IEnumerable<DropdownItem> DataMaps { get; set; }
        public List<AgentBootstrapDataDropdownItem> BootstrapDatas { get; set; }
        public IEnumerable<SelectListItem> AgentTypes { get; set; }
        public IEnumerable<Schedule> AgentSchedules { get; set; }
        public IEnumerable<MappedAgent> MappedAgents { get; set; }
        public List<AgentBootstrapData> AgentBootstrapDatas { get; set; }
        public string EncryptionFailureMsg { get; set; }

        public List<SelectListItem> ApiServers { get; set; }

        [Display(Name = "API Connection")]
        public int? ApiServerId { get; set; }

        [Display(Name = "Run Order")]
        public int? RunOrder { get; set; }
    }

    public class Validator : AbstractValidator<AddEditAgentViewModel>
    {
        private readonly DataImportDbContext _dbContext;
        private readonly string _encryptionKey;
        private readonly IEncryptionService _encryptionService;

        public Validator(DataImportDbContext dbContext, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService)
        {
            _dbContext = dbContext;
            _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
            _encryptionService = encryptionService;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Must(BeAUniqueName).WithMessage(model =>
                    $"An Agent named \"{model.Name}\" already exists. Please provide a unique Agent name.");
            RuleFor(x => x.AgentTypeCode).NotEmpty().WithName("Agent Type");
            RuleFor(x => x.Url).NotEmpty().When(SftpOrFtpsAgentType).WithName("Host Name");
            RuleFor(x => x.Username).NotEmpty().When(SftpOrFtpsAgentType);
            RuleFor(x => x.Password).NotEmpty().When(SftpOrFtpsAgentType);
            RuleFor(x => x.Password).Must(HaveValidEncryptionKey).When(x => !string.IsNullOrEmpty(x.Password))
                .WithMessage(Constants.AgentEncryptionError);
            RuleFor(x => x.RunOrder).GreaterThanOrEqualTo(0).Must(BeUnchangedOrUnusedRunOrder).When(x => x.RunOrder != null)
                .WithMessage(model => $"An Agent with the run order {model.RunOrder} already exists. Please provide a distinct run order.");
            RuleFor(x => x.Directory).NotEmpty().When(SftpOrFtpsAgentType);
            RuleFor(x => x.FilePattern).NotEmpty().When(SftpOrFtpsAgentType).WithName("File Pattern");
            RuleFor(x => x.FileGeneratorId).NotEmpty().When(PowerShellAgentType).WithMessage("You must select a File Generator.").WithName("Generator");
            RuleFor(x => x.ApiServerId).NotEmpty().WithName("API Connection");
        }

        private bool BeAUniqueName(AddEditAgentViewModel model, string candidateName) =>
            EditingWithoutChangingAgentName(model, candidateName) || NewNameDoesNotAlreadyExist(model, candidateName);

        private bool EditingWithoutChangingAgentName(AddEditAgentViewModel model, string candidateName) =>
            _dbContext.Agents.FirstOrDefault(agent => agent.Id == model.Id)?.Name == candidateName;

        private bool NewNameDoesNotAlreadyExist(AddEditAgentViewModel model, string candidateName) =>
            _dbContext.Agents.FirstOrDefault(agent => agent.Name == candidateName && agent.Id != model.Id && !agent.Archived) == null;

        private bool BeUnchangedOrUnusedRunOrder(AddEditAgentViewModel model, int? candidateRunOrder) {
            return _dbContext.Agents.FirstOrDefault(agent => agent.RunOrder == candidateRunOrder && agent.Id != model.Id) == null;
        }

        private bool SftpOrFtpsAgentType(AddEditAgentViewModel vm) =>
            !string.IsNullOrWhiteSpace(vm.AgentTypeCode) && (vm.AgentTypeCode.Equals(AgentTypeCodeEnum.SFTP) ||
            vm.AgentTypeCode.Equals(AgentTypeCodeEnum.FTPS));

        private bool PowerShellAgentType(AddEditAgentViewModel vm) =>
            !string.IsNullOrWhiteSpace(vm.AgentTypeCode) && (vm.AgentTypeCode.Equals(AgentTypeCodeEnum.PowerShell));

        private bool HaveValidEncryptionKey(string password) =>
            _encryptionService.TryEncrypt(password, _encryptionKey, out var encryptedKey);
    }

    public class MappedAgent
    {
        public string DataMapName { get; set; }
        public int ProcessingOrder { get; set; }
        public int DataMapId { get; set; }
    }

    public class Schedule
    {
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Id { get; set; }
    }

    public class DropdownItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class AgentBootstrapDataDropdownItem : DropdownItem
    {
        public string Resource { get; set; }
    }

    public class AgentBootstrapData
    {
        public int BootstrapDataId { get; set; }
        public int ProcessingOrder { get; set; }
        public string Resource { get; set; }
        public string BootstrapName { get; set; }
    }
}