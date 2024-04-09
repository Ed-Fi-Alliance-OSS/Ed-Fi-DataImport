// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Shared
{
    public class ApiServerSpecificRequestHandler<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IApiServerListViewModel
    {
        private readonly ApiServerSelectListProvider _serverSelectListProvider;
        private readonly DataImportDbContext _database;

        public ApiServerSpecificRequestHandler(ApiServerSelectListProvider serverSelectListProvider, DataImportDbContext database)
        {
            _serverSelectListProvider = serverSelectListProvider;
            _database = database;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var apiServerSelectItems = await _serverSelectListProvider.GetApiServers(cancellationToken);
            var hasConfiguredApiServer = apiServerSelectItems.Any(x => !string.IsNullOrWhiteSpace(x.Value));
            if (!hasConfiguredApiServer)
            {
                throw new OdsApiServerException(new Exception("No API Connections found."));
            }

            int? selectedApiServerId = null;
            if (request is IApiServerSpecificRequest apiServerSpecificRequest)
            {
                if (apiServerSpecificRequest.ApiVersionId.HasValue)
                {
                    var filteredApiVersions = await _database.ApiServers.Where(x => x.ApiVersionId == apiServerSpecificRequest.ApiVersionId).Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToListAsync(cancellationToken);
                    apiServerSelectItems = apiServerSelectItems.Where(x => string.IsNullOrEmpty(x.Value) || filteredApiVersions.Contains(x.Value)).ToList();
                }

                var apiServers = apiServerSelectItems.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();

                if (apiServers.Count == 1 && !apiServerSpecificRequest.ApiServerId.HasValue)
                {
                    apiServerSpecificRequest.ApiServerId = int.Parse(apiServers[0].Value);
                }

                selectedApiServerId = apiServerSpecificRequest.ApiServerId;
            }

            var response = await next();

            response.ApiServers = apiServerSelectItems;
            if (selectedApiServerId.HasValue)
            {
                response.ApiServerId = selectedApiServerId.Value;
            }

            return response;
        }
    }
}
