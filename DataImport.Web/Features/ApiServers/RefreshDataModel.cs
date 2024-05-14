// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUglify;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class RefreshDataModel
    {
        public class Command : IRequest<Response>
        {
            public int ApiServerId { get; set; }
        }

        public class Response : ToastResponse
        {
            public int ApiServerId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly DataImportDbContext _database;
            private readonly IConfigurationService _configurationService;

            public CommandHandler(DataImportDbContext database, IConfigurationService configurationService)
            {
                _database = database;
                _configurationService = configurationService;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var apiServer = await _database.ApiServers
                    .Include(x => x.ApiVersion)
                    .SingleOrDefaultAsync(x => x.Id == request.ApiServerId, cancellationToken);

                if (apiServer is null)
                {
                    throw new System.ArgumentException("Api server not found");
                }

                await _configurationService.FillSwaggerMetadata(apiServer);

                await _database.SaveChangesAsync(cancellationToken);

                var dataMaps = await _database.DataMaps
                    .Join(_database.Resources,
                        dataMap => dataMap.ApiVersionId,
                        resource => resource.ApiVersionId,
                        (dataMap, resource) => new { dataMap = dataMap, resource = resource }
                    )
                    .Where(res => res.dataMap.ApiVersionId == apiServer.ApiVersionId && res.dataMap.ResourcePath.Trim() == res.resource.Path.Trim())
                    .Select(x => x.dataMap)
                    .ToListAsync(cancellationToken);

                var resourcesPath = dataMaps.Select(map => map.ResourcePath).Distinct().ToList();

                var metadataFromResources = await _database
                    .Resources
                    .Where(resource => resource.ApiVersionId == apiServer.ApiVersionId && resourcesPath.Contains(resource.Path))
                    .Select(resource => new { resource.Path, resource.Metadata })
                    .ToListAsync(cancellationToken);

                dataMaps.ForEach(map =>
                {
                    var newMetadata = metadataFromResources.Where(x => x.Path == map.ResourcePath).Select(x => x.Metadata).FirstOrDefault();
                    if (newMetadata is not null)
                    {
                        map.Metadata = newMetadata;
                    }
                });

                _database.DataMaps.UpdateRange(dataMaps);
                await _database.SaveChangesAsync(cancellationToken);

                return new Response
                {
                    ApiServerId = apiServer.Id,
                    Message = "Data model was updated"
                };
            }
        }
    }
}
