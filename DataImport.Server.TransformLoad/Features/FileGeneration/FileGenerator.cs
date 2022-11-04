// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Enums;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features.FileGeneration
{
    public class FileGenerator
    {
        public class Command : IRequest
        {
            public int ApiServerId { get; set; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly ILogger<FileGenerator> _logger;
            private readonly DataImportDbContext _dbContext;
            private readonly IPowerShellPreprocessorService _powerShellPreprocessorService;
            private readonly IExternalPreprocessorService _externalPreprocessorService;
            private readonly IFileService _fileService;

            public CommandHandler(ILogger<FileGenerator> logger, IOptions<AppSettings> options, DataImportDbContext dbContext, ResolveFileService fileServices, IPowerShellPreprocessorService powerShellPreprocessorService, IExternalPreprocessorService externalPreprocessorService)
            {
                _logger = logger;
                _dbContext = dbContext;
                _powerShellPreprocessorService = powerShellPreprocessorService;
                _externalPreprocessorService = externalPreprocessorService;
                _fileService = fileServices(options.Value.FileMode);
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {               
                var agents = await _dbContext.Agents
                    .Include(agent => agent.AgentSchedules)
                    .Include(x => x.FileGenerator)
                    .Include(x => x.ApiServer).ThenInclude(x => x.ApiVersion)
                    .Where(agent =>
                        agent.ApiServerId == request.ApiServerId &&
                        agent.Enabled && agent.Archived == false &&
                        agent.AgentTypeCode == AgentTypeCodeEnum.PowerShell)
                    .OrderBy(agent => agent.RunOrder == null)
                    .ThenBy(agent => agent.RunOrder)
                    .ThenBy(agent => agent.Id)
                    .ToListAsync(cancellationToken);

                foreach (var agent in agents)
                {
                    if (!Helper.ShouldExecuteOnSchedule(agent, DateTimeOffset.Now))
                        continue;
                    _logger.LogInformation("Processing agent name: {agent}", agent.Name);
                    _logger.LogInformation("File generator: {generator}", agent.FileGenerator.Name);

                    var generatedFilePath = agent.FileGenerator.ScriptType switch
                    {
                        ScriptType.CustomFileGenerator => _powerShellPreprocessorService.GenerateFile(agent.FileGenerator.ScriptContent, CreateOptionsForPreprocessor(agent.FileGenerator, agent.ApiServer, agent.Id)),
                        ScriptType.ExternalFileGenerator => _externalPreprocessorService.GenerateFile(agent.FileGenerator.ExecutablePath, agent.FileGenerator.ExecutableArguments),
                        _ => throw new NotImplementedException($"Handling for script type {agent.FileGenerator.ScriptType} is not implemented.")
                    };

                    var fileName = Path.GetFileName(generatedFilePath);

                    using (var stream = new FileStream(generatedFilePath, FileMode.Open))
                        await _fileService.Transfer(stream, fileName, agent);

                    agent.LastExecuted = DateTimeOffset.Now;
                }

                _dbContext.SaveChanges();
            }

            private ProcessOptions CreateOptionsForPreprocessor(Script preprocessor, ApiServer apiServer, int agentId)
            {
                var options = new ProcessOptions
                {
                    RequiresOdsConnection = preprocessor.RequireOdsApiAccess,
                    OdsConnectionSettings = apiServer,
                    IsDataMapPreview = false,
                    CacheIdentifier = agentId.ToString(CultureInfo.InvariantCulture),
                    UsePowerShellWithNoRestrictions = preprocessor.ShouldRunPowerShellWithNoRestrictions()
                };

                options.ProcessMessageLogged += Options_ProcessMessageLogged;

                return options;
            }

            private void Options_ProcessMessageLogged(object sender, ProcessMessageEventArgs e)
            {
                _logger.Log(e.Level, e.Message);
            }
        }
    }
}