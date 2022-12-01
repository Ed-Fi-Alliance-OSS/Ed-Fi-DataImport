// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features.FileTransport
{
    public class FileTransporter
    {
        public class Command : IRequest
        {
            public int ApiServerId { get; set; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly ILogger<FileTransporter> _logger;
            private readonly DataImportDbContext _dbContext;
            private readonly ResolveFileServer _fileServers;

            public CommandHandler(ILogger<FileTransporter> logger, DataImportDbContext dbContext, ResolveFileServer fileServers)
            {
                _fileServers = fileServers;
                _logger = logger;
                _dbContext = dbContext;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var agents = await _dbContext.Agents
                    .Include(agent => agent.AgentSchedules)
                    .Where(agent => agent.ApiServerId == request.ApiServerId &&
                        agent.Enabled && agent.Archived == false &&
                        (agent.AgentTypeCode == AgentTypeCodeEnum.Sftp ||
                         agent.AgentTypeCode == AgentTypeCodeEnum.Ftps))
                    .OrderBy(agent => agent.RunOrder == null)
                    .ThenBy(agent => agent.RunOrder)
                    .ThenBy(agent => agent.Id)
                    .ToListAsync(cancellationToken);

                foreach (var agent in agents)
                {
                    if (!Helper.ShouldExecuteOnSchedule(agent, DateTimeOffset.Now))
                        continue;
                    _logger.LogInformation("Processing agent name: {name}", agent.Name);

                    var fileServer = _fileServers(agent.AgentTypeCode);
                    var fileList = (await fileServer.GetFileList(agent)).ToList();
                    _logger.LogInformation("Items to process: {count}", fileList.Count());

                    foreach (var file in fileList)
                    {
                        _logger.LogInformation("Processing file: {file}", file);

                        // Check the file log to see if the file already exists, if not, upload to file storage
                        if (!await Helper.DoesFileExistInLog(_dbContext, agent.Id, file))
                            await fileServer.TransferFileToStorage(agent, file);
                    }

                    agent.LastExecuted = DateTimeOffset.Now;
                }

                _dbContext.SaveChanges();
            }
        }
    }
}
