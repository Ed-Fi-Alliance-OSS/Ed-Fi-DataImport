// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using DataImport.Web.Services;
using FluentValidation;
using MediatR;

namespace DataImport.Web.Features.ApiServers
{
    public class InferOdsApiVersion
    {
        public class Query : IRequest<string>
        {
            public string ApiServerUrl { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.ApiServerUrl).NotEmpty();
            }
        }

        public class QueryHandler : IRequestHandler<Query, string>
        {
            private readonly IConfigurationService _configurationService;

            public QueryHandler(IConfigurationService configurationService)
            {
                _configurationService = configurationService;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                var apiVersion = await _configurationService.InferOdsApiVersion(request.ApiServerUrl);
                return apiVersion;
            }
        }
    }
}
