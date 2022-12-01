// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.Preprocessor;
using DataImport.Web.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.TestHelpers.TestHelpers;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Agent
{
    internal class AddEditAgentTests
    {
        private readonly string _originalEncryptionKeyValue = Testing.Services.GetRequiredService<IOptions<AppSettings>>().Value.EncryptionKey;

        [Test]
        public void ShouldRequireAgentTypeCodeAndApiConnection()
            => new AddEditAgentViewModel { Name = SampleString() }
                .ShouldNotValidate("'Agent Type' must not be empty.", "'API Connection' must not be empty.");

        [Test]
        public void ShouldRequireMinimumFieldsForSftpAgent()
            => new AddEditAgentViewModel { AgentTypeCode = "SFTP" }
                .ShouldNotValidate(
                    "'Name' must not be empty.",
                    "'Host Name' must not be empty.",
                    "'Username' must not be empty.",
                    "'Password' must not be empty.",
                    "'Directory' must not be empty.",
                    "'File Pattern' must not be empty.",
                    "'API Connection' must not be empty."
                );

        [Test]
        public void ShouldRequireMinimumFieldsForFtpsAgent()
            => new AddEditAgentViewModel { AgentTypeCode = "FTPS" }
                .ShouldNotValidate(
                    "'Name' must not be empty.",
                    "'Host Name' must not be empty.",
                    "'Username' must not be empty.",
                    "'Password' must not be empty.",
                    "'Directory' must not be empty.",
                    "'File Pattern' must not be empty.",
                    "'API Connection' must not be empty."
                );

        [Test]
        public void ShouldRequireMinimumFieldsForManualAgent()
            => new AddEditAgentViewModel { AgentTypeCode = "Manual" }
                .ShouldNotValidate("'Name' must not be empty.", "'API Connection' must not be empty.");

        [Test]
        public void ShouldRequireMinimumFieldsForPowerShellAgent()
            => new AddEditAgentViewModel { AgentTypeCode = "PowerShell" }
                .ShouldNotValidate(
                    "'Name' must not be empty.",
                    "You must select a File Generator.",
                    "'API Connection' must not be empty."
                );

        [Test]
        public async Task ShouldRequireUniqueNameWhenAddingNewAgent()
        {
            var existingName = SampleString();

            var apiServer = GetDefaultApiServer();

            await Send(new AddAgent.Command { ViewModel = new AddEditAgentViewModel { Name = existingName, ApiServerId = apiServer.Id } });

            new AddEditAgentViewModel { Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id }.ShouldNotValidate(
                $"An Agent named \"{existingName}\" already exists. Please provide a unique Agent name.");
        }

        [Test]
        public async Task ShouldAllowEditingAgentWithoutChangingName()
        {
            var sampleName = SampleString();
            var apiServer = GetDefaultApiServer();

            var agentId = (await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = sampleName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id } })).AgentId;

            new AddEditAgentViewModel { Id = agentId, Name = sampleName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id }.ShouldValidate();
        }

        [Test]
        public async Task ShouldPreventEditingAgentWithDuplicateName()
        {
            var existingName = SampleString();
            var apiServer = GetDefaultApiServer();

            await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id } });

            var agentId = (await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = "Name to change", AgentTypeCode = "Manual", ApiServerId = apiServer.Id } })).AgentId;

            new AddEditAgentViewModel { Id = agentId, Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id }
                .ShouldNotValidate(
                    $"An Agent named \"{existingName}\" already exists. Please provide a unique Agent name.");
        }

        [Test]
        public async Task ShouldNotValidateAgentWithDuplicateRunOrder()
        {
            var existingName = SampleString();
            var apiServer = GetDefaultApiServer();
            const int runOrder = 1;

            await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = runOrder } });

            new AddEditAgentViewModel { Name = "New Agent", AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = runOrder }
                .ShouldNotValidate($"An Agent with the run order {runOrder} already exists. Please provide a distinct run order.");
        }

        [Test]
        public async Task ShouldNotValidateAgentWithNegativeRunOrder()
        {
            var existingName = SampleString();
            var apiServer = GetDefaultApiServer();
            const int runOrder = 1;

            await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = runOrder } });

            new AddEditAgentViewModel { Name = "New Agent", AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = 0 }
                .ShouldValidate();

            new AddEditAgentViewModel { Name = "New Agent", AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = -1 }
                .ShouldNotValidate("'Run Order' must be greater than or equal to '0'.");
        }

        [Test]
        public async Task ShouldValidateAgentWithNullRunOrder()
        {
            var existingName = SampleString();
            var apiServer = GetDefaultApiServer();
            const int runOrder = 1;

            await Send(new AddAgent.Command
            { ViewModel = new AddEditAgentViewModel { Name = existingName, AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = runOrder } });

            new AddEditAgentViewModel { Name = "New Agent", AgentTypeCode = "Manual", ApiServerId = apiServer.Id, RunOrder = null }
                .ShouldValidate();
        }

        [Test]
        public async Task ShouldSuccessfullySaveManualAgent()
        {
            var name = SampleString();
            var apiServer = GetDefaultApiServer();
            var rowProcessor = await AddPreprocessor(ScriptType.CustomRowProcessor);
            const int initialRunOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = name,
                Enabled = true,
                RunOrder = initialRunOrder,
                RowProcessorId = rowProcessor.Id,
                ApiServerId = apiServer.Id
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "Manual",
                Name = name,
                Enabled = true,
                RunOrder = initialRunOrder,
                RowProcessorId = rowProcessor.Id,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });

            editForm.Name = name + " Edited";
            editForm.Enabled = false;
            editForm.RowProcessorId = null;
            editForm.RunOrder = 2;

            await Send(new EditAgent.Command { ViewModel = editForm });

            editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "Manual",
                Name = name + " Edited",
                Enabled = false,
                RunOrder = 2,
                RowProcessorId = null,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                Password = "",
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });
        }

        [Test]
        public async Task ShouldSuccessfullySavePowerShellAgent()
        {
            var name = SampleString();
            var apiServer = GetDefaultApiServer();
            var fileGenerator = await AddPreprocessor(ScriptType.CustomFileGenerator);
            const int initialRunOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "PowerShell",
                Name = name,
                Enabled = true,
                FileGeneratorId = fileGenerator.Id,
                ApiServerId = apiServer.Id,
                RunOrder = initialRunOrder
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "PowerShell",
                Name = name,
                Enabled = true,
                RunOrder = initialRunOrder,
                FileGeneratorId = fileGenerator.Id,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });

            editForm.Name = name + " Edited";
            editForm.Enabled = false;
            editForm.FileGeneratorId = null;
            editForm.RunOrder = 2;

            await Send(new EditAgent.Command { ViewModel = editForm });

            editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "PowerShell",
                Name = name + " Edited",
                Enabled = false,
                RunOrder = 2,
                FileGeneratorId = null,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                Password = "",
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });
        }

        [Test]
        public async Task ShouldSuccessfullySaveExternalProcessAgent()
        {
            var name = SampleString();
            var apiServer = GetDefaultApiServer();
            var fileGenerator = await AddPreprocessor(ScriptType.ExternalFileGenerator);
            const int initialRunOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "PowerShell",
                Name = name,
                Enabled = true,
                FileGeneratorId = fileGenerator.Id,
                ApiServerId = apiServer.Id,
                RunOrder = initialRunOrder
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "PowerShell",
                Name = name,
                Enabled = true,
                RunOrder = initialRunOrder,
                FileGeneratorId = fileGenerator.Id,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });

            editForm.Name = name + " Edited";
            editForm.Enabled = false;
            editForm.FileGeneratorId = null;
            editForm.RunOrder = 2;

            await Send(new EditAgent.Command { ViewModel = editForm });

            editForm = await Send(new EditAgent.Query { Id = agentId });

            editForm.ShouldMatch(new AddEditAgentViewModel
            {
                Id = agentId,
                AgentTypeCode = "PowerShell",
                Name = name + " Edited",
                Enabled = false,
                RunOrder = 2,
                FileGeneratorId = null,
                RowProcessors = editForm.RowProcessors,
                FileGenerators = editForm.FileGenerators,
                DataMaps = editForm.DataMaps,
                AgentTypes = editForm.AgentTypes,
                Password = "",
                ApiServerId = apiServer.Id,
                ApiServers = editForm.ApiServers,
                BootstrapDatas = editForm.BootstrapDatas
            });
        }

        [Test]
        [TestCase(AgentTypeCodeEnum.SFTP, null)]
        [TestCase(AgentTypeCodeEnum.SFTP, 123)]
        [TestCase(AgentTypeCodeEnum.FTPS, null)]
        [TestCase(AgentTypeCodeEnum.FTPS, 234)]
        public async Task ShouldSuccessfullyAddFtpAgent(string agentTypeCode, int? port)
        {
            var apiServer = GetDefaultApiServer();
            const int initialRunOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = agentTypeCode,
                Name = SampleString(),
                Url = SampleString(),
                Port = port,
                Username = SampleString(),
                Password = SampleString(),
                Directory = SampleString(),
                FilePattern = SampleString(),
                ApiServerId = apiServer.Id,
                RunOrder = initialRunOrder
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });
            var actual = await Send(new EditAgent.Query { Id = response.AgentId });

            viewModel.Id = response.AgentId;
            viewModel.DataMaps = actual.DataMaps;
            viewModel.AgentTypes = actual.AgentTypes;
            viewModel.RowProcessors = actual.RowProcessors;
            viewModel.FileGenerators = actual.FileGenerators;
            viewModel.ApiServers = actual.ApiServers;
            viewModel.BootstrapDatas = actual.BootstrapDatas;

            response.AssertToast($"Agent '{viewModel.Name}' was created.");
            actual.ShouldMatch(viewModel);
        }

        [Test]
        [TestCase(AgentTypeCodeEnum.SFTP, null)]
        [TestCase(AgentTypeCodeEnum.SFTP, 456)]
        [TestCase(AgentTypeCodeEnum.FTPS, null)]
        [TestCase(AgentTypeCodeEnum.FTPS, 567)]
        public async Task ShouldSuccessfullyEditFtpAgent(string agentTypeCode, int? port)
        {
            var apiServer = GetDefaultApiServer();
            const int initialRunOrder = 1;

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = agentTypeCode,
                Name = SampleString(),
                Url = SampleString(),
                Port = port,
                Username = SampleString(),
                Password = SampleString(),
                Directory = SampleString(),
                FilePattern = SampleString(),
                ApiServerId = apiServer.Id,
                RunOrder = initialRunOrder
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var agent = await Send(new EditAgent.Query { Id = agentId });

            agent.Username = SampleString("New Username");
            agent.Password = SampleString("New Password");
            agent.Port = port == null ? 999 : (int?) null;
            agent.RunOrder = 2;

            var response = await Send(new EditAgent.Command { ViewModel = agent });
            var updatedAgent = await Send(new EditAgent.Query { Id = agentId });

            response.AssertToast($"Agent '{agent.Name}' was modified.");
            updatedAgent.ShouldMatch(agent);
        }

        [Test]
        public void ShouldValidateAgentWithNoPasswordAndEmptyEncryptionKey()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = SampleString(),
                ApiServerId = apiServer.Id
            };

            try
            {
                // Clearing the encryption key value for testing
                UpdateEncryptionKeyValueOnAppConfig("");
                viewModel.ShouldValidate();
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public void ShouldNotValidateAgentWithPasswordAndEmptyEncryptionKey()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "SFTP",
                Name = SampleString(),
                Url = SampleString(),
                Username = SampleString(),
                Password = SampleString(),
                Directory = SampleString(),
                FilePattern = SampleString(),
                ApiServerId = apiServer.Id
            };

            try
            {
                // Clearing the encryption key value for testing
                UpdateEncryptionKeyValueOnAppConfig("");
                viewModel.ShouldNotValidate(Constants.AgentEncryptionError);
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public async Task ShouldDisplayGuidanceForMissingEncryptionKeyWhileEditingAgent()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "SFTP",
                Name = SampleString(),
                Url = SampleString(),
                Username = SampleString(),
                Password = SampleString(),
                Directory = SampleString(),
                FilePattern = SampleString(),
                ApiServerId = apiServer.Id
            };

            try
            {
                var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
                UpdateEncryptionKeyValueOnAppConfig("");
                var vm = await Send(new EditAgent.Query { Id = agentId });
                vm.EncryptionFailureMsg.ShouldBe(Constants.AgentDecryptionError);
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public async Task ShouldDisplayGuidanceForDifferentEncryptionKeyWhileEditingAgent()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "SFTP",
                Name = SampleString(),
                Url = SampleString(),
                Username = SampleString(),
                Password = SampleString(),
                Directory = SampleString(),
                FilePattern = SampleString(),
                ApiServerId = apiServer.Id
            };

            try
            {
                var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
                UpdateEncryptionKeyValueOnAppConfig("DifferentEncryptionKey");
                var vm = await Send(new EditAgent.Query { Id = agentId });
                vm.EncryptionFailureMsg.ShouldBe(Constants.AgentDecryptionError);
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public async Task ShouldSuccessfullyDisableAndThenReenableAgent()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = true,
                Name = SampleString(),
                ApiServerId = apiServer.Id
            };

            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;

            var disableResponse = await Send(new ToggleAgentStatus.Command() { Id = agentId });
            disableResponse.AssertToast($"Agent '{viewModel.Name}' was disabled.");
            var disabledAgent = await Send(new EditAgent.Query { Id = agentId });
            disabledAgent.Enabled.ShouldBe(false);

            var enabledResponse = await Send(new ToggleAgentStatus.Command() { Id = agentId });
            enabledResponse.AssertToast($"Agent '{viewModel.Name}' was enabled.");
            var enabledAgent = await Send(new EditAgent.Query { Id = agentId });
            enabledAgent.Enabled.ShouldBe(true);
        }

        [Test]
        public async Task ShouldSuccessfullyAssociateBootstrapDataWithAgent()
        {
            var apiServer = GetDefaultApiServer();
            var bootstrapData1 = await AddBootstrapData(RandomResource());
            var bootstrapData2 = await AddBootstrapData(RandomResource());

            var addEditViewModel = await Send(new AddAgent.Query());
            addEditViewModel.BootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData1.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData2.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => string.IsNullOrEmpty(x.Value));
            addEditViewModel.AgentBootstrapDatas.ShouldBeEmpty();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = true,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                DdlBootstrapDatas = new List<string>
                {
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData1.Id, ProcessingOrder = 1 }),
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData2.Id, ProcessingOrder = 2 })
                }
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });
            addEditViewModel = await Send(new EditAgent.Query { Id = response.AgentId });
            addEditViewModel.DdlBootstrapDatas.ShouldBeEmpty();
            addEditViewModel.AgentBootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.AgentBootstrapDatas.Count.ShouldBe(2);
            addEditViewModel.AgentBootstrapDatas.ShouldContain(x => x.BootstrapDataId == bootstrapData1.Id && x.ProcessingOrder == 1);
            addEditViewModel.AgentBootstrapDatas.ShouldContain(x => x.BootstrapDataId == bootstrapData2.Id && x.ProcessingOrder == 2);

            addEditViewModel.DdlBootstrapDatas = new List<string>
            {
                Json(new AgentBootstrapData { BootstrapDataId = bootstrapData1.Id, ProcessingOrder = 5})
            };

            await Send(new EditAgent.Command { ViewModel = addEditViewModel });
            addEditViewModel = await Send(new EditAgent.Query { Id = response.AgentId });
            addEditViewModel.AgentBootstrapDatas.Count.ShouldBe(1);
            addEditViewModel.AgentBootstrapDatas.ShouldContain(x => x.BootstrapDataId == bootstrapData1.Id && x.ProcessingOrder == 5);
            addEditViewModel.BootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData1.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData2.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => string.IsNullOrEmpty(x.Value));


            addEditViewModel.DdlBootstrapDatas = new List<string>();
            await Send(new EditAgent.Command { ViewModel = addEditViewModel });
            addEditViewModel = await Send(new EditAgent.Query { Id = response.AgentId });
            addEditViewModel.AgentBootstrapDatas.ShouldBeEmpty();
            addEditViewModel.BootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData1.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => x.Value == bootstrapData2.Id.ToString(CultureInfo.InvariantCulture));
            addEditViewModel.BootstrapDatas.ShouldContain(x => string.IsNullOrEmpty(x.Value));
        }


        [Test]
        public async Task ShouldRemoveDuplicatesFromAssociatedBootstrapDatas()
        {
            var apiServer = GetDefaultApiServer();
            var bootstrapData = await AddBootstrapData(RandomResource());

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = true,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                DdlBootstrapDatas = new List<string>
                {
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id, ProcessingOrder = 1 }),
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id, ProcessingOrder = 2 })
                }
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });
            var addEditViewModel = await Send(new EditAgent.Query { Id = response.AgentId });
            addEditViewModel.DdlBootstrapDatas.ShouldBeEmpty();
            addEditViewModel.AgentBootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.AgentBootstrapDatas.Count.ShouldBe(1);
            addEditViewModel.AgentBootstrapDatas.ShouldContain(x => x.BootstrapDataId == bootstrapData.Id);

            addEditViewModel.DdlBootstrapDatas = new List<string>
            {
                Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id, ProcessingOrder = 2 }),
                Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id, ProcessingOrder = 1 }),
            };
            await Send(new EditAgent.Command { ViewModel = addEditViewModel });
            addEditViewModel = await Send(new EditAgent.Query { Id = response.AgentId });
            addEditViewModel.DdlBootstrapDatas.ShouldBeEmpty();
            addEditViewModel.AgentBootstrapDatas.ShouldNotBeEmpty();
            addEditViewModel.AgentBootstrapDatas.Count.ShouldBe(1);
            addEditViewModel.AgentBootstrapDatas.ShouldContain(x => x.BootstrapDataId == bootstrapData.Id);
        }

        [Test]
        public async Task ShouldRemoveBootstrapDataAgentsWhenAgentIsArchived()
        {
            var apiServer = GetDefaultApiServer();
            var bootstrapData = await AddBootstrapData(RandomResource());

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = true,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                DdlBootstrapDatas = new List<string>
                {
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id }),
                }
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });

            Query(d => d.BootstrapDataAgents.Count(x => x.AgentId == response.AgentId)).ShouldBe(1);

            await Send(new ArchiveAgent.Command { Id = response.AgentId });

            Query(d => d.BootstrapDataAgents.Count(x => x.AgentId == response.AgentId)).ShouldBe(0);
        }

        [TestCase(AgentTypeCodeEnum.Manual)]
        [TestCase(AgentTypeCodeEnum.PowerShell)]
        public async Task ShouldAutomaticallyDisassociateIncorrectPreprocessorWhenSavingAgent(string agentType)
        {
            var fileGenerator = await Send(new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    Name = SampleString("Name"),
                    ScriptType = ScriptType.CustomFileGenerator,
                    ScriptContent = "{}"
                }
            });

            var rowProcessor = await Send(new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    Name = SampleString("Name"),
                    ScriptType = ScriptType.CustomRowProcessor,
                    ScriptContent = "{}"
                }
            });

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = agentType,
                Enabled = true,
                Name = SampleString(),
                ApiServerId = GetDefaultApiServer().Id,
                RowProcessorId = rowProcessor.PreprocessorId,
                FileGeneratorId = fileGenerator.PreprocessorId
            };

            // Add
            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;
            var addEditViewModel = await Send(new EditAgent.Query { Id = agentId });
            var expectedPreprocessorId = agentType == AgentTypeCodeEnum.Manual ? addEditViewModel.RowProcessorId : addEditViewModel.FileGeneratorId;
            var disassociatedPreprocessorId = agentType == AgentTypeCodeEnum.Manual ? addEditViewModel.FileGeneratorId : addEditViewModel.RowProcessorId;
            expectedPreprocessorId.ShouldNotBeNull();
            disassociatedPreprocessorId.ShouldBeNull();

            // Edit
            addEditViewModel.RowProcessorId = rowProcessor.PreprocessorId;
            addEditViewModel.FileGeneratorId = fileGenerator.PreprocessorId;
            await Send(new EditAgent.Command { ViewModel = addEditViewModel });
            addEditViewModel = await Send(new EditAgent.Query { Id = agentId });
            expectedPreprocessorId = agentType == AgentTypeCodeEnum.Manual ? addEditViewModel.RowProcessorId : addEditViewModel.FileGeneratorId;
            disassociatedPreprocessorId = agentType == AgentTypeCodeEnum.Manual ? addEditViewModel.FileGeneratorId : addEditViewModel.RowProcessorId;
            expectedPreprocessorId.ShouldNotBeNull();
            disassociatedPreprocessorId.ShouldBeNull();
        }

        [Test]
        public async Task ShouldBeEnabledByDefaultOnAdd()
        {
            var addModel = await Send(new AddAgent.Query());
            addModel.Enabled.ShouldBeTrue();
        }

        [Test]
        public async Task ShouldNotOverrideEnabledOnEdit()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = false,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });
            var editModel = await Send(new EditAgent.Query { Id = response.AgentId });

            editModel.Enabled.ShouldBeFalse();
        }
    }
}
