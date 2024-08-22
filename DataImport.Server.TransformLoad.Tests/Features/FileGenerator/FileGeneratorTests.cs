// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DataImport.Server.TransformLoad.Tests.Testing;

namespace DataImport.Server.TransformLoad.Tests.Features.FileGenerator
{
    [TestFixture]
    internal class FileGeneratorTests
    {
        [Test]
        public async Task ShouldCorrectlyGenerateFile()
        {
            var tempFile = Path.GetTempFileName();

            var logger = Services.GetService<ILogger<TransformLoad.Features.FileGeneration.FileGenerator>>();
            var fileServices = GetRegisteredFileServices();
            var dbContext = Services.GetService<DataImportDbContext>();
            var options = Services.GetService<IOptions<AppSettings>>();
            var service = new PowerShellPreprocessorService(options.Value, new PowerShellPreprocessorOptions(), A.Fake<IAuthRequestWrapper>());
            var extService = Services.GetService<ExternalPreprocessorService>();
            var commandHandler = new CommandHandlerTestWrapper(logger, options, dbContext, fileServices, service, extService);

            var apiServer = GetDefaultApiServer();

            var scriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}" // File generator should return a path to the file
            });

            var agentId = AddScheduledAgentForScript("AgentName", apiServer.Id, scriptId);

            await commandHandler.Execute(new TransformLoad.Features.FileGeneration.FileGenerator.Command
            {
                ApiServerId = apiServer.Id,
            }, CancellationToken.None);

            var file = Query(d => d.Files.Single(f => f.AgentId == agentId));
            file.Status.ShouldBe(FileStatus.Uploaded);
            file.FileName.ShouldBe(new FileInfo(tempFile).Name);

            // Disable agent
            Query(d =>
            {
                var agent = d.Agents.Single(x => x.Id == agentId);
                agent.Enabled = false;
                d.SaveChanges();
                return agent;
            });

            System.IO.File.Delete(tempFile);
        }

        [Test]
        public async Task ShouldRunAgentsInOrder()
        {
            var tempFile = Path.GetTempFileName();

            var logger = Services.GetService<ILogger<TransformLoad.Features.FileGeneration.FileGenerator>>();
            var fileServices = GetRegisteredFileServices();
            var dbContext = Services.GetService<DataImportDbContext>();
            var options = Services.GetService<IOptions<AppSettings>>();
            var service = new PowerShellPreprocessorService(options.Value, new PowerShellPreprocessorOptions(), A.Fake<IAuthRequestWrapper>());
            var extService = Services.GetService<ExternalPreprocessorService>();
            var commandHandler = new CommandHandlerTestWrapper(logger, options, dbContext, fileServices, service, extService);

            var apiServer = GetDefaultApiServer();

            var firstScriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}" // File generator should return a path to the file
            });

            var secondScriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}"
            });

            var unorderedScriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}"
            });

            var secondAgentId = AddScheduledAgentForScript("SecondAgentName", apiServer.Id, firstScriptId, 2);
            var unorderedAgentId = AddScheduledAgentForScript("UnorderedAgentName", apiServer.Id, unorderedScriptId, null);
            var firstAgentId = AddScheduledAgentForScript("FirstAgentName", apiServer.Id, secondScriptId, 1);

            await commandHandler.Execute(new TransformLoad.Features.FileGeneration.FileGenerator.Command
            {
                ApiServerId = apiServer.Id,
            }, CancellationToken.None);

            var firstFile = Query(d => d.Files.Single(f => f.AgentId == firstAgentId));
            var secondFile = Query(d => d.Files.Single(f => f.AgentId == secondAgentId));
            var unorderedFile = Query(d => d.Files.Single(f => f.AgentId == unorderedAgentId));

            firstFile.CreateDate.Value.ShouldBeLessThan(secondFile.CreateDate.Value);
            firstFile.CreateDate.Value.ShouldBeLessThan(unorderedFile.CreateDate.Value);
            secondFile.CreateDate.Value.ShouldBeLessThan(unorderedFile.CreateDate.Value);

            // Disable agents
            Query(d =>
            {
                var agents = d.Agents.Where(x => x.Id == firstAgentId || x.Id == secondAgentId || x.Id == unorderedAgentId).ToList();
                foreach (var agent in agents)
                {
                    agent.Enabled = false;
                }
                d.SaveChanges();
                return agents;
            });

            System.IO.File.Delete(tempFile);
        }

        [Test]
        public async Task ShouldRunOnlyEnabledAgents()
        {
            var tempFile = Path.GetTempFileName();

            var logger = Services.GetService<ILogger<TransformLoad.Features.FileGeneration.FileGenerator>>();
            var fileServices = GetRegisteredFileServices();
            var dbContext = Services.GetService<DataImportDbContext>();
            var options = Services.GetService<IOptions<AppSettings>>();
            var service = new PowerShellPreprocessorService(options.Value, new PowerShellPreprocessorOptions(), A.Fake<IAuthRequestWrapper>());
            var extService = Services.GetService<ExternalPreprocessorService>();
            var commandHandler = new CommandHandlerTestWrapper(logger, options, dbContext, fileServices, service, extService);

            var apiServer = GetDefaultApiServer();

            var enabledAgentScriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}" // File generator should return a path to the file
            });

            var disabledAgentScriptId = Add(new Script
            {
                ScriptType = ScriptType.CustomFileGenerator,
                Name = SampleString("TestScript"),
                ScriptContent = $"Write-Output {tempFile}"
            });

            var enabledAgentName = "EnabledAgentName";
            var enabledAgentId = AddScheduledAgentForScript(enabledAgentName, apiServer.Id, enabledAgentScriptId, 1);

            var disabledAgentName = "DisabledAgentName";
            var disabledAgentId = AddScheduledAgentForScript(disabledAgentName, apiServer.Id, disabledAgentScriptId, 2, false);

            await commandHandler.Execute(new TransformLoad.Features.FileGeneration.FileGenerator.Command
            {
                ApiServerId = apiServer.Id,
            }, CancellationToken.None);

            // Disable agents
            Query(d =>
            {
                var agents = d.Agents.Where(x => x.Id == enabledAgentId || x.Id == disabledAgentId).ToList();
                foreach (var agent in agents)
                {
                    agent.Enabled = false;
                }
                d.SaveChanges();
                return agents;
            });

            System.IO.File.Delete(tempFile);
        }

        private int AddScheduledAgentForScript(string agentName, int apiServerId, int scriptId, int? agentOrder = null, bool enabled = true)
        {
            var agentId = AddAgent(new Agent
            {
                AgentTypeCode = AgentTypeCodeEnum.PowerShell,
                Name = SampleString(agentName),
                ApiServerId = apiServerId,
                Enabled = enabled,
                FileGeneratorScriptId = scriptId,
                RunOrder = agentOrder,
            });

            var schedule = DateTime.Now.AddMinutes(-15);
            Add(new AgentSchedule
            {
                AgentId = agentId,
                Day = (int) schedule.DayOfWeek,
                Hour = schedule.Hour,
                Minute = schedule.Minute
            });

            return agentId;
        }

        public class CommandHandlerTestWrapper : TransformLoad.Features.FileGeneration.FileGenerator.CommandHandler
        {
            public CommandHandlerTestWrapper(ILogger<TransformLoad.Features.FileGeneration.FileGenerator> logger, IOptions<AppSettings> options, DataImportDbContext dbContext, ResolveFileService fileServices, IPowerShellPreprocessorService powerShellPreprocessorService, IExternalPreprocessorService externalPreprocessorService)
                : base(logger, options, dbContext, fileServices, powerShellPreprocessorService, externalPreprocessorService)
            {
            }

            public Task Execute(TransformLoad.Features.FileGeneration.FileGenerator.Command command, CancellationToken cancellationToken)
            {
                return base.Handle(command, cancellationToken);
            }
        }
    }
}
