// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using DataImport.Web.Features.Shared.SelectListProviders;
using MediatR;

namespace DataImport.Web.Features.Shared
{
    public class ApiVersionSpecificRequestHandler<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IApiVersionListViewModel
    {
        private readonly ApiVersionSelectListProvider _apiVersionSelectListProvider;

        public ApiVersionSpecificRequestHandler(ApiVersionSelectListProvider apiVersionSelectListProvider)
        {
            _apiVersionSelectListProvider = apiVersionSelectListProvider;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var apiVersionSelectList = await _apiVersionSelectListProvider.GetApiVersions(cancellationToken);

            var response = await next();

            response.ApiVersions = apiVersionSelectList;

            return response;
        }
    }
}
