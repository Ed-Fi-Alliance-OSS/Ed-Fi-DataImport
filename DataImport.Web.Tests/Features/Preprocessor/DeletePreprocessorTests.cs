// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.Preprocessor;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Preprocessor
{
    [TestFixture]
    public class DeletePreprocessorTests
    {
        [Test]
        public async Task ShouldBeAbleToDeletePreprocessor()
        {
            var preprocessor = await AddPreprocessor(ScriptType.CustomFileProcessor);
            var allPreprocessors = await Send(new PreprocessorIndex.Query());

            allPreprocessors.Preprocessors.ShouldNotBeEmpty();
            allPreprocessors.Preprocessors.Any(x => x.Id == preprocessor.Id).ShouldBeTrue();

            var deleteResponse = await Send(new DeletePreprocessor.Command
            {
                Id = preprocessor.Id
            });
            deleteResponse.AssertToast($"Preprocessor '{preprocessor.Name}' was deleted.");

            allPreprocessors = await Send(new PreprocessorIndex.Query());
            allPreprocessors.Preprocessors.Any(x => x.Id == preprocessor.Id).ShouldBeFalse();
        }

        [Test]
        public async Task ShouldShowErrorMessageIfAgentIsAssociatedWithPreprocessor()
        {
            // Test File Generator
            var viewModel = new AddEditPreprocessorViewModel
            {
                Name = SampleString("Some Custom File Generator"),
                ScriptType = ScriptType.CustomFileGenerator,
                ScriptContent = SampleString("Some PS Content"),
                RequireOdsApiAccess = true
            };

            var addPreprocessorResponse = await Send(new AddPreprocessor.Command { ViewModel = viewModel });

            var apiServer = GetDefaultApiServer();
            var addAgent = new AddEditAgentViewModel
            {
                AgentTypeCode = AgentTypeCodeEnum.PowerShell,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                FileGeneratorId = addPreprocessorResponse.PreprocessorId
            };

            await Send(new AddAgent.Command { ViewModel = addAgent });

            var deleteResponse = await Send(new DeletePreprocessor.Command { Id = addPreprocessorResponse.PreprocessorId });
            deleteResponse.AssertToast("Preprocessor cannot be deleted because there is at least one Agent using it.", false);

            // Test Row Generator
            viewModel = new AddEditPreprocessorViewModel
            {
                Name = SampleString("Some Custom Row Processor"),
                ScriptType = ScriptType.CustomRowProcessor,
                ScriptContent = SampleString("Some PS Content"),
                RequireOdsApiAccess = true
            };

            addPreprocessorResponse = await Send(new AddPreprocessor.Command { ViewModel = viewModel });

            addAgent = new AddEditAgentViewModel
            {
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                RowProcessorId = addPreprocessorResponse.PreprocessorId
            };

            await Send(new AddAgent.Command { ViewModel = addAgent });

            deleteResponse = await Send(new DeletePreprocessor.Command { Id = addPreprocessorResponse.PreprocessorId });
            deleteResponse.AssertToast("Preprocessor cannot be deleted because there is at least one Agent using it.", false);
        }
    }
}
