// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataImport.Web.Features.Preprocessor
{
    public class AddEditPreprocessorViewModel : IRequest
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "PowerShell Script")]
        [DataType(DataType.MultilineText)]
        public string ScriptContent { get; set; }

        [Display(Name = "Script Type")]
        public ScriptType? ScriptType { get; set; }

        [Display(Name = "Require API Connection?")]
        public bool RequireOdsApiAccess { get; set; }

        [Display(Name = "Has Attribute")]
        public bool HasAttribute { get; set; }

        [Display(Name = "Processor Path")]
        public string ExecutablePath { get; set; }

        [Display(Name = "Processor Arguments")]
        public string ExecutableArguments { get; set; }

        public IList<SelectListItem> ScriptTypes { get; set; }

        public bool ExternalPreprocessorsEnabled { get; set; }
    }

    public class Validator : AbstractValidator<AddEditPreprocessorViewModel>
    {
        private readonly DataImportDbContext _dbContext;
        private readonly IPowerShellPreprocessorService _powerShellPreprocessorService;

        public Validator(DataImportDbContext dbContext, IPowerShellPreprocessorService powerShellPreprocessorService)
        {
            _dbContext = dbContext;
            _powerShellPreprocessorService = powerShellPreprocessorService;

            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ScriptType).NotEmpty().WithName("Script Type");

            When(x => x.ScriptType.HasValue && x.ScriptType.Value.IsPowerShell(), () =>
            {
                RuleFor(x => x.ExecutablePath).Empty().WithMessage("'Processor Path' is not used for PowerShell scripts.");
                RuleFor(x => x.ExecutableArguments).Empty().WithMessage("'Processor Arguments' are not used for PowerShell scripts.");
                RuleFor(x => x.ScriptContent)
                    .NotEmpty()
                    .Must(ValidateScriptType).WithMessage($"There are one or more errors in the script: {Environment.NewLine}{{ScriptErrors}}")
                    .WithName("PowerShell Script");
            });

            When(x => x.ScriptType.HasValue && !string.IsNullOrEmpty(x.Name), () =>
            {
                // Disable ReSharper warning since x.ScriptType will always have value. 
                // ReSharper disable once PossibleInvalidOperationException
                RuleFor(x => x.Name).Must(ValidateScriptName).WithMessage(x => $"Script with name '{x.Name}' and script type '{EnumHelper.GetEnumDescription(x.ScriptType.Value)}' already exists");
            });
            When(x => x.Id.HasValue, () =>
            {
                RuleFor(m => m.ScriptType).Must(NotChange).WithMessage("Changing script type for the existing scripts is not allowed.");
                RuleFor(m => m.HasAttribute).Must(NotBreakExistingDataMaps).WithMessage("Cannot save the preprocessor since the following data map(s) have an empty Attribute field: {DataMaps}. Please fix the data map(s) and try it again.");
            });

            When(x => x.HasAttribute && x.ScriptType.HasValue, () =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                RuleFor(s => s.ScriptType).Must(s => s.Value == ScriptType.CustomFileProcessor).WithMessage("'Has Attribute' is only allowed for 'Custom File Processor'.");
            });

            When(x => x.ScriptType.HasValue && x.ScriptType.Value.IsExternal(), () =>
            {
                RuleFor(x => x.ExecutablePath)
                    .NotEmpty().WithName("Processor Path")
                    .Must(path => path == null || System.IO.File.Exists(path))
                        .WithMessage("Processor not found. Verify the file exists and 'Processor Path' is correct.");
                RuleFor(x => x.ScriptContent).Empty().WithMessage("'Script Content' is not utilized for External Preprocessors and should be cleared.");
                RuleFor(x => x.RequireOdsApiAccess).Equal(false).WithMessage("Script ODS API integration is not supported by External Preprocessors.");
                RuleFor(x => x.HasAttribute).Equal(false).WithMessage("Attributes are not supported by External Preprocessors.");
            });
        }

        private bool NotBreakExistingDataMaps(AddEditPreprocessorViewModel viewModel, bool hasAttribute, ValidationContext<AddEditPreprocessorViewModel> context)
        {
            if (!hasAttribute)
            {
                return true;
            }

            var dataMapsToBreak = _dbContext.DataMaps.Where(x => x.FileProcessorScriptId == viewModel.Id && string.IsNullOrEmpty(x.Attribute)).AsNoTracking().Select(x => x.Name).ToList();
            if (dataMapsToBreak.Count > 0)
            {
                context.MessageFormatter.AppendArgument("DataMaps", string.Join(", ", dataMapsToBreak.Select(x => $"'{x}'")));
                return false;
            }

            return true;
        }

        private bool ValidateScriptType(AddEditPreprocessorViewModel arg1, string scriptContent, ValidationContext<AddEditPreprocessorViewModel> propertyValidatorContext)
        {
            try
            {
                _powerShellPreprocessorService.ValidateScript(scriptContent);
            }
            catch (PowerShellValidateException powerShellValidateException)
            {
                propertyValidatorContext.MessageFormatter.AppendArgument("ScriptErrors",
                    string.Join(Environment.NewLine,
                        powerShellValidateException.ParseErrors.Select(x => $"'{x.Message}' at line {x.Extent.StartLineNumber} column {x.Extent.StartColumnNumber}.")
                        ));
                return false;
            }

            return true;
        }

        private bool NotChange(AddEditPreprocessorViewModel viewModel, ScriptType? scriptType)
        {
            return _dbContext.Scripts.Count(x => x.Id == viewModel.Id && x.ScriptType == scriptType) == 1;
        }

        private bool ValidateScriptName(AddEditPreprocessorViewModel viewModel, string scriptName)
        {
            return _dbContext.Scripts.Count(x => x.Name == scriptName && x.ScriptType == viewModel.ScriptType && (!viewModel.Id.HasValue || viewModel.Id.HasValue && x.Id != viewModel.Id)) == 0;
        }
    }
}
