// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.Log;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DataImport.Web.Features.Agent
{
    public class ArchiveAgent
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class CommandHandler : RequestHandler<Command, ToastResponse>
        {
            private readonly ILogger<ArchiveAgent> _logger;
            private readonly DataImportDbContext _database;
            private readonly IMediator _mediator;

            public CommandHandler(ILogger<ArchiveAgent> logger, DataImportDbContext database, IMediator mediator)
            {
                _logger = logger;
                _database = database;
                _mediator = mediator;
            }

            protected override ToastResponse Handle(Command request)
            {
                var agent = _database.Agents.Include(x => x.BootstrapDataAgents).Single(x => x.Id == request.Id);

                agent.Enabled = false;
                agent.Archived = true;
                agent.ApiServerId = null;
                agent.FileGeneratorScriptId = null;
                agent.RowProcessorScriptId = null;

                _database.BootstrapDataAgents.RemoveRange(agent.BootstrapDataAgents);

                UpdateAgentFiles(agent);

                _logger.Archived(agent, a => a.Name);

                return new ToastResponse
                {
                    Message = $"Agent '{agent.Name}' was archived."
                };
            }

            private void UpdateAgentFiles(DataImport.Models.Agent agent)
            {
                var files = _database.Files.Where(x =>
                    x.AgentId == agent.Id && FileStatusExtensions.CancelableStatuses.Contains(x.Status)).ToList();
                foreach (var file in files)
                    _mediator.Send(new CancelFile.Command { Id = file.Id });
            }
        }
    }
}