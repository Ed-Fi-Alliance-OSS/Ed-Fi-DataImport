// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.Preprocessor;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;
using AddPreprocessor = DataImport.Web.Features.Preprocessor.AddPreprocessor;

namespace DataImport.Web.Tests.Features.Agent
{
    class ArchiveAgentTests
    {
        [Test]
        public async Task ShouldSuccessfullyArchiveAgent()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = SampleString(),
                ApiServerId = apiServer.Id
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var response = await Send(new ArchiveAgent.Command { Id = agentId });
            response.AssertToast($"Agent '{viewModel.Name}' was archived.");

            var archivedAgent = GetAgentById(agentId);

            archivedAgent.ShouldNotBeNull();
            archivedAgent.Archived.ShouldBeTrue();
            archivedAgent.Enabled.ShouldBeFalse();
            archivedAgent.ApiServerId.ShouldBeNull();
        }

        [Test]
        public async Task ShouldAgentListNotContainsArchivedAgents()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = SampleString(),
                ApiServerId = apiServer.Id
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            await Send(new ArchiveAgent.Command { Id = agentId });
            var agentViewModel = await Send(new AgentIndex.Query());

            agentViewModel.Agents.ShouldNotBeNull();
            agentId.ShouldNotBeOneOf(agentViewModel.Agents.Select(x => x.Id).ToArray());
        }

        [Test]
        public async Task ShouldValidateDuplicateAgentNameOnceAgentIsArchived()
        {
            var apiServer = GetDefaultApiServer();
            var duplicateAgentName = SampleString();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = duplicateAgentName,
                ApiServerId = apiServer.Id
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            await Send(new ArchiveAgent.Command { Id = agentId });

            new AddEditAgentViewModel { AgentTypeCode = "Manual", Name = duplicateAgentName, ApiServerId = apiServer.Id }.ShouldValidate();
        }

        [Test]
        public async Task ShouldNotValidateDuplicateRunOrderEvenIfAgentIsArchived()
        {
            var apiServer = GetDefaultApiServer();
            var agentName = SampleString();
            const int runOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = agentName,
                ApiServerId = apiServer.Id,
                RunOrder = runOrder
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            await Send(new ArchiveAgent.Command { Id = agentId });

            new AddEditAgentViewModel { AgentTypeCode = "Manual", Name = agentName, ApiServerId = apiServer.Id, RunOrder = runOrder }
                .ShouldNotValidate($"An Agent with the run order {runOrder} already exists. Please provide a distinct run order.");
        }

        [Test]
        public async Task ShouldCancelAndDeleteAssociatedFilesFromStorageWhenArchivingAgent()
        {
            var uploadedFile = await UploadFile();

            var agentId = uploadedFile.AgentId;
            var localPath = new Uri(uploadedFile.Url).LocalPath;

            try
            {
                System.IO.File.Exists(localPath).ShouldBe(true);
                uploadedFile.Status.ShouldBe(FileStatus.Uploaded);

                await Send(new ArchiveAgent.Command
                {
                    Id = agentId
                });

                var canceledFile = Query<File>(uploadedFile.Id);
                System.IO.File.Exists(localPath).ShouldBe(false);
                canceledFile.Status.ShouldBe(FileStatus.Canceled);
            }
            catch
            {
                System.IO.File.Delete(localPath);
                throw;
            }
        }

        [TestCase(ScriptType.CustomRowProcessor, AgentTypeCodeEnum.Manual)]
        [TestCase(ScriptType.CustomFileGenerator, AgentTypeCodeEnum.PowerShell)]
        public async Task ShouldResetPreprocessorsWhenArchivingAgent(ScriptType scriptType, string agentTypeCode)
        {
            var apiServer = GetDefaultApiServer();

            var preprocessor = await Send(new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    Name = SampleString("Name"),
                    ScriptType = scriptType,
                    ScriptContent = "{}"
                }
            });

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = agentTypeCode,
                Name = SampleString(agentTypeCode),
                ApiServerId = apiServer.Id,
                RowProcessorId = scriptType == ScriptType.CustomRowProcessor ? preprocessor.PreprocessorId : (int?)null,
                FileGeneratorId = scriptType == ScriptType.CustomFileGenerator ? preprocessor.PreprocessorId : (int?)null
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;

            var agent = Query(dbContext => dbContext.Agents.Single(x => x.Id == agentId));
            var processorId = scriptType == ScriptType.CustomFileGenerator ? agent.FileGeneratorScriptId : agent.RowProcessorScriptId;
            processorId.ShouldBe(preprocessor.PreprocessorId);

            await Send(new ArchiveAgent.Command { Id = agentId });

            agent = Query(dbContext => dbContext.Agents.Single(x => x.Id == agentId));
            agent.RowProcessorScriptId.ShouldBeNull();
            agent.FileGeneratorScriptId.ShouldBeNull();
        }

        private DataImport.Models.Agent GetAgentById(int agentId) => Query(database => database.Agents.SingleOrDefault(x => x.Id == agentId));
    }
}
