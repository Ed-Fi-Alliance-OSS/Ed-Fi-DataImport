// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using DataImport.Web.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class AddApiServer
    {
        public class Query : IRequest<AddEditApiServerViewModel>
        {
        }

        public class QueryHandler : RequestHandler<Query, AddEditApiServerViewModel>
        {
            protected override AddEditApiServerViewModel Handle(Query request)
            {
                return new AddEditApiServerViewModel();
            }
        }

        public class Response : ToastResponse
        {
            public int ApiServerId { get; set; }
        }

        public class Command : IRequest<Response>
        {
            public AddEditApiServerViewModel ViewModel { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;
            private readonly IEncryptionService _encryptionService;
            private readonly IConfigurationService _configurationService;
            private readonly string _encryptionKey;

            public CommandHandler(ILogger<AddApiServer> logger, DataImportDbContext database, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService, IConfigurationService configurationService)
            {
                _logger = logger;
                _database = database;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _encryptionService = encryptionService;
                _configurationService = configurationService;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var apiVersion = await _database.ApiVersions.SingleOrDefaultAsync(x => x.Version == request.ViewModel.ApiVersion, cancellationToken);
                if (apiVersion == null)
                {
                    apiVersion = new ApiVersion
                    {
                        Version = request.ViewModel.ApiVersion
                    };
                }

                var apiServer = new ApiServer();
                _database.ApiServers.Add(apiServer);

                await request.ViewModel.MapTo(apiServer, apiVersion, _encryptionService, _encryptionKey, _configurationService);

                await _database.SaveChangesAsync(cancellationToken); // Explicitly call SaveChanges to get Id for the apiServer.

                await _configurationService.FillSwaggerMetadata(apiServer); // DI-1340 error here for new ods api I need to fix.

                _logger.Added(apiServer, x => x.Name);

                return new Response
                {
                    ApiServerId = apiServer.Id,
                    Message = $"Connection '{apiServer.Name}' was created."
                };
            }
        }
    }
}
