// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Preprocessor;
using DataImport.Web.Features.Shared.SelectListProviders;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataImport.Common.Preprocessors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Tests.Features.Preprocessor
{
    using static Testing;

    public class AddEditPreprocessorTests
    {
        [Test]
        public async Task ShouldRequireMinimumFields()
        {
            new AddEditPreprocessorViewModel()
                .ShouldNotValidate("'Name' must not be empty.", "'Script Type' must not be empty.");

            var existingScript = await AddPreprocessor(ScriptType.CustomRowProcessor);

            new AddEditPreprocessorViewModel
            {
                Name = existingScript.Name,
                ScriptType = existingScript.ScriptType
            }
                .ShouldNotValidate($"Script with name '{existingScript.Name}' and script type '{EnumHelper.GetEnumDescription(existingScript.ScriptType)}' already exists", "'PowerShell Script' must not be empty.");


            new AddEditPreprocessorViewModel
            {
                Id = existingScript.Id,
                Name = existingScript.Name,
                ScriptType = ScriptType.CustomFileGenerator
            }
                .ShouldNotValidate("Changing script type for the existing scripts is not allowed.", "'PowerShell Script' must not be empty.");

            new AddEditPreprocessorViewModel
            {
                Name = SampleString(),
                ScriptType = existingScript.ScriptType,
                ScriptContent = "Write-Output Hello"
            }.ShouldValidate();
        }

        [Test]
        public void ShouldRequireScriptContentWhenScriptIsPowerShell()
        {
            new AddEditPreprocessorViewModel { ScriptType = ScriptType.CustomRowProcessor, Name = SampleString() }
                .ShouldNotValidate("'PowerShell Script' must not be empty.");

            new AddEditPreprocessorViewModel { ScriptType = ScriptType.CustomFileProcessor, Name = SampleString() }
                .ShouldNotValidate("'PowerShell Script' must not be empty.");

            new AddEditPreprocessorViewModel { ScriptType = ScriptType.CustomFileGenerator, Name = SampleString() }
                .ShouldNotValidate("'PowerShell Script' must not be empty.");
        }

        [Test]
        public void ShouldRequireValidExeWhenScriptIsExternal()
        {
            new AddEditPreprocessorViewModel { ScriptType = ScriptType.ExternalFileProcessor, Name = SampleString() }
                .ShouldNotValidate("'Processor Path' must not be empty.");

            new AddEditPreprocessorViewModel { ScriptType = ScriptType.ExternalFileGenerator, Name = SampleString(), ExecutablePath = "notreal"}
                .ShouldNotValidate("Processor not found. Verify the file exists and 'Processor Path' is correct.");

            new AddEditPreprocessorViewModel { ScriptType = ScriptType.ExternalFileGenerator, Name = SampleString(), ExecutablePath = Path.GetTempFileName() }
                .ShouldValidate();
        }

        [Test]
        public void ShouldValidateScriptContent()
        {
            new AddEditPreprocessorViewModel
            {
                Name = SampleString("Name"),
                ScriptType = ScriptType.CustomFileGenerator,
                ScriptContent = "{"
            }
                .ShouldNotValidate($"There are one or more errors in the script: {Environment.NewLine}'Missing closing '}}' in statement block or type definition.' at line 1 column 1.");

            new AddEditPreprocessorViewModel
            {
                Name = SampleString("Name"),
                ScriptType = ScriptType.CustomFileGenerator,
                ScriptContent = "{}"
            }
                .ShouldValidate();
        }

        [Test]
        public void ShouldOnlyAllowHasAttributeForCustomFileProcessor()
        {
            new AddEditPreprocessorViewModel
            {
                Name = SampleString("Name"),
                ScriptType = ScriptType.CustomFileProcessor,
                ScriptContent = "Write-Output Hello",
                HasAttribute = true
            }
                .ShouldValidate();

            new AddEditPreprocessorViewModel
            {
                Name = SampleString("Name"),
                ScriptContent = "{}",
                ScriptType = ScriptType.CustomRowProcessor,
                HasAttribute = true
            }
                .ShouldNotValidate("'Has Attribute' is only allowed for 'Custom File Processor'.");

            new AddEditPreprocessorViewModel
            {
                Name = SampleString("Name"),
                ScriptContent = "{}",
                ScriptType = ScriptType.CustomFileGenerator,
                HasAttribute = true
            }
                .ShouldNotValidate("'Has Attribute' is only allowed for 'Custom File Processor'.");
        }

        [Test]
        public void ShouldNotAllowMixingFieldsForTypes()
        {
            AddEditPreprocessorViewModel NewFilledViewModelForType(ScriptType scriptType)
            {
                return new AddEditPreprocessorViewModel
                {
                    Name = SampleString("Name"),
                    ScriptType = scriptType,
                    ExecutablePath = Path.GetTempFileName(),
                    ExecutableArguments = "-abc 123",
                    ScriptContent = "Write-Output Hello",
                    RequireOdsApiAccess = true,
                };
            }

            NewFilledViewModelForType(ScriptType.CustomRowProcessor)
                .ShouldNotValidate("'Processor Path' is not used for PowerShell scripts.", "'Processor Arguments' are not used for PowerShell scripts.");
            NewFilledViewModelForType(ScriptType.CustomFileProcessor)
                .ShouldNotValidate("'Processor Path' is not used for PowerShell scripts.", "'Processor Arguments' are not used for PowerShell scripts.");
            NewFilledViewModelForType(ScriptType.CustomFileGenerator)
                .ShouldNotValidate("'Processor Path' is not used for PowerShell scripts.", "'Processor Arguments' are not used for PowerShell scripts.");

            NewFilledViewModelForType(ScriptType.ExternalFileProcessor)
                .ShouldNotValidate("'Script Content' is not utilized for External Preprocessors and should be cleared.", "Script ODS API integration is not supported by External Preprocessors.");
            NewFilledViewModelForType(ScriptType.ExternalFileGenerator)
                .ShouldNotValidate("'Script Content' is not utilized for External Preprocessors and should be cleared.", "Script ODS API integration is not supported by External Preprocessors.");
        }

        [Test]
        public async Task ShouldSuccessfullyAddPreprocessor()
        {
            var vm = await Send(new AddPreprocessor.Query());
            vm.ScriptTypes.ShouldNotBeNull();
            vm.RequireOdsApiAccess.ShouldBeFalse();
            vm.Name.ShouldBeNull();
            vm.Id.ShouldBeNull();
            vm.ScriptContent.ShouldBeNull();
            vm.HasAttribute.ShouldBeFalse();

            vm.Name = SampleString("Some Custom File Generator");
            vm.ScriptType = ScriptType.CustomFileProcessor;
            vm.ScriptContent = "Some PS Script";
            vm.RequireOdsApiAccess = true;
            vm.HasAttribute = true;

            var addPreprocessorId = await Send(new AddPreprocessor.Command { ViewModel = vm });
            addPreprocessorId.AssertToast($"Script '{vm.Name}' was created.");
            addPreprocessorId.PreprocessorId.ShouldBeGreaterThan(0);

            var addEditViewModel = await Send(new EditPreprocessor.Query { Id = addPreprocessorId.PreprocessorId });
            addEditViewModel.ShouldMatch(new AddEditPreprocessorViewModel
            {
                RequireOdsApiAccess = vm.RequireOdsApiAccess,
                ScriptType = vm.ScriptType,
                ScriptContent = vm.ScriptContent,
                Name = vm.Name,
                Id = addEditViewModel.Id,
                HasAttribute = true
            });
        }

        [Test]
        public async Task ShouldSuccessfullyEditPreprocessor()
        {
            var viewModel = new AddEditPreprocessorViewModel
            {
                Name = SampleString("Some Custom File Generator"),
                ScriptType = ScriptType.CustomFileProcessor,
                ScriptContent = SampleString("Some PS Content"),
                RequireOdsApiAccess = true,
                HasAttribute = true
            };

            var addPreprocessorId = await Send(new AddPreprocessor.Command { ViewModel = viewModel });
            addPreprocessorId.AssertToast($"Script '{viewModel.Name}' was created.");
            addPreprocessorId.PreprocessorId.ShouldBeGreaterThan(0);

            var addEditViewModel = await Send(new EditPreprocessor.Query { Id = addPreprocessorId.PreprocessorId });

            addEditViewModel.Name = SampleString("New Name");
            addEditViewModel.RequireOdsApiAccess = false;
            addEditViewModel.ScriptContent = SampleString("New Script Content");
            addEditViewModel.ScriptType = ScriptType.CustomFileGenerator;
            addEditViewModel.HasAttribute = false;

            addEditViewModel.ShouldNotValidate("Changing script type for the existing scripts is not allowed.");

            addEditViewModel.ScriptType = ScriptType.CustomRowProcessor;
            var editResponse = await Send(new EditPreprocessor.Command { ViewModel = addEditViewModel });

            var editedViewModel = await Send(new EditPreprocessor.Query { Id = editResponse.PreprocessorId });
            editedViewModel.ShouldMatch(new AddEditPreprocessorViewModel
            {
                Id = editResponse.PreprocessorId,
                ScriptType = ScriptType.CustomRowProcessor,
                RequireOdsApiAccess = false,
                Name = addEditViewModel.Name,
                ScriptContent = addEditViewModel.ScriptContent,
                HasAttribute = false
            });
        }

        [Test(Description = "Verifies that scripts with all possible types can be successfully stored in the database")]
        public async Task ShouldSuccessfullyPersistAllPossibleScriptTypes()
        {
            await With<ScriptTypeSelectListProvider>(async p =>
            {
                var scriptTypes = p.GetSelectListItems();
                foreach (SelectListItem selectListItem in scriptTypes)
                {
                    if (string.IsNullOrEmpty(selectListItem.Value))
                    {
                        continue;
                    }

                    var preprocessor = await AddPreprocessor((ScriptType)int.Parse(selectListItem.Value));
                    Query<Script>(preprocessor.Id).ShouldNotBeNull();
                }
            });
        }

        [Test]
        public async Task ShouldNotAllowSavingPreprocessorIfItBreaksExistingDataMaps()
        {
            var viewModel = new AddEditPreprocessorViewModel
            {
                Name = SampleString("Some Custom File Generator"),
                ScriptType = ScriptType.CustomFileProcessor,
                ScriptContent = SampleString("Write-Output Header1"),
            };

            var addPreprocessorResponse = await Send(new AddPreprocessor.Command { ViewModel = viewModel });

            var addEditViewModel = await Send(new EditPreprocessor.Query { Id = addPreprocessorResponse.PreprocessorId });
            addEditViewModel.HasAttribute = true;
            addEditViewModel.ShouldValidate();

            var apiVersion = GetDefaultApiVersion();
            var existingResource = RandomResource(apiVersion.Id);
            var trivialMappings = await TrivialMappings(existingResource);
            var addDataMapCommand = new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = SampleString(),
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings,
                PreprocessorId = addPreprocessorResponse.PreprocessorId
            };
            await Send(addDataMapCommand);
            addEditViewModel.ShouldNotValidate($"Cannot save the preprocessor since the following data map(s) have an empty Attribute field: '{addDataMapCommand.MapName}'. Please fix the data map(s) and try it again.");
        }

        [Test]
        public Task ShouldNotAllowAddingExternalPreprocessorWhenDisabled()
        {
            var disabledSettings = new ExternalPreprocessorOptions { Enabled = false };
            var handler = new AddPreprocessor.CommandHandler(
                Services.GetRequiredService<ILogger<AddPreprocessor.CommandHandler>>(),
                Services.GetRequiredService<DataImportDbContext>(), Services.GetRequiredService<IMapper>(),
                Options.Create(disabledSettings));

            var command = new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    ScriptType = ScriptType.ExternalFileProcessor,
                    ExecutablePath = Path.GetTempFileName(),
                }
            };

            return Should.ThrowAsync<ValidationException>(async () => await handler.Handle(command, CancellationToken.None));
        }
    }
}
