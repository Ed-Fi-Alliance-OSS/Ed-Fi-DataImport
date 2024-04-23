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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.BootstrapData
{
    public class DeleteBootstrapData
    {
        public class Command : IRequest<ToastResponse>
        {
            public int BootstrapDataId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<DeleteBootstrapData> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var bootstrapData = _database.BootstrapDatas
                    .Include(x => x.BootstrapDataAgents)
                    .Include(x => x.BootstrapDataApiServers)
                    .Single(x => x.Id == request.BootstrapDataId);

                if (bootstrapData.BootstrapDataAgents.Count > 0)
                {
                    return Task.FromResult(new ToastResponse
                    {
                        IsSuccess = false,
                        Message = "Bootstrap Data cannot be deleted because it is used by one or more Agents."
                    });
                }

                var bootstrapDataName = bootstrapData.Name;

                _logger.Deleted(bootstrapData, b => b.Name);

                if (bootstrapData.BootstrapDataApiServers.Count > 0)
                {
                    _database.BootstrapDataApiServers.RemoveRange(bootstrapData.BootstrapDataApiServers);
                }

                _database.BootstrapDatas.Remove(bootstrapData);

                return Task.FromResult(new ToastResponse
                {
                    Message = $"Bootstrap Data '{bootstrapDataName}' was deleted."
                });
            }

        }
    }
}
