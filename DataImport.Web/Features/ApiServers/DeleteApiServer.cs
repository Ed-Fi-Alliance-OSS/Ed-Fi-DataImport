// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class DeleteApiServer
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<DeleteApiServer> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public async Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                if (await _database.Agents.AnyAsync(x => x.ApiServerId == request.Id, cancellationToken))
                {
                    return new ToastResponse
                    {
                        IsSuccess = false,
                        Message = "API connection cannot be deleted because there is at least one agent using it."
                    };
                }

                var apiServer = await _database.ApiServers.Include(x => x.BootstrapDataApiServers).SingleAsync(x => x.Id == request.Id, cancellationToken);

                if (apiServer.BootstrapDataApiServers.Count > 0)
                {
                    _database.BootstrapDataApiServers.RemoveRange(apiServer.BootstrapDataApiServers);
                }

                _database.ApiServers.Remove(apiServer);
                _logger.Deleted(apiServer, a => a.Name);

                return new ToastResponse
                {
                    Message = $"Connection '{apiServer.Name}' was deleted."
                };
            }
        }
    }
}
