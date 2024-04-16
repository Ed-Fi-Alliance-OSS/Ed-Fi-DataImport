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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class EditApiServer
    {
        public class Query : IRequest<AddEditApiServerViewModel>
        {
            public int Id { get; set; }

            public bool OdsApiServerException { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, AddEditApiServerViewModel>
        {
            private readonly DataImportDbContext _database;
            private readonly IEncryptionService _encryptionService;
            private readonly string _encryptionKey;

            public QueryHandler(DataImportDbContext database, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService)
            {
                _database = database;
                _encryptionService = encryptionService;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
            }

            public async Task<AddEditApiServerViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var apiServer = await _database.ApiServers.Include(x => x.ApiVersion).SingleAsync(x => x.Id == request.Id, cancellationToken);

                var viewModel = new AddEditApiServerViewModel
                {
                    Id = apiServer.Id,
                    Name = apiServer.Name,
                    Url = apiServer.Url,
                    ApiVersion = apiServer.ApiVersion.Version,
                    Key = _encryptionService.TryDecrypt(apiServer.Key, _encryptionKey, out var decryptedKey) ? SensitiveText.Mask(decryptedKey) : string.Empty,
                    Secret = _encryptionService.TryDecrypt(apiServer.Secret, _encryptionKey, out var decryptedSecret) ? SensitiveText.Mask(decryptedSecret) : string.Empty
                };

                if (!_encryptionService.TryDecrypt(apiServer.Key, _encryptionKey, out _))
                {
                    viewModel.EncryptionFailureMsg = Constants.ConfigDecryptionError;
                }

                if (request.OdsApiServerException)
                {
                    viewModel.ConfigurationFailureMsg = "An error occurred while attempting to contact the configured ODS API Server. Check the connection here and try again.";
                }

                return viewModel;
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

            public CommandHandler(ILogger<EditApiServer> logger, DataImportDbContext database, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService, IConfigurationService configurationService)
            {
                _logger = logger;
                _database = database;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _encryptionService = encryptionService;
                _configurationService = configurationService;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var apiVersion = ResolveApiVersion(request.ViewModel.ApiVersion);

                var apiServer = _database.ApiServers.Single(x => x.Id == request.ViewModel.Id);

                try
                {
                    await request.ViewModel.MapTo(apiServer, apiVersion, _encryptionService, _encryptionKey, _configurationService);
                }
                catch (OdsApiServerException e)
                {
                    e.ApiServerId = apiServer.Id;
                    throw;
                }

                await _configurationService.FillSwaggerMetadata(apiServer);

                _logger.Added(apiServer, x => x.Name);

                return new Response
                {
                    ApiServerId = apiServer.Id,
                    Message = $"Connection '{apiServer.Name}' was modified."
                };
            }

            protected ApiVersion ResolveApiVersion(string apiVersion)
            {
                var apiVersionEntity = _database.ApiVersions.SingleOrDefault(x => x.Version == apiVersion);
                if (apiVersionEntity == null)
                {
                    apiVersionEntity = new ApiVersion
                    {
                        Version = apiVersion
                    };
                }

                return apiVersionEntity;
            }
        }
    }
}
