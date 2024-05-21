// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataImport.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataImport.Web.Features.Log
{
    public class OdsApi7xFilters
    {
        public class Query : IRequest<LogViewModel>
        {

        }

        public class QueryHandler : IRequestHandler<Query, LogViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext dataImportDbContext, IMapper mapper)
            {
                _dataImportDbContext = dataImportDbContext;
                _mapper = mapper;
            }

            public Task<LogViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var allTenants = _dataImportDbContext.IngestionLogs
                    .Where(c => !string.IsNullOrEmpty(c.Tenant))
                    .GroupBy(c => c.Tenant).Select(g => g.First()).ToList();

                var allContexts = _dataImportDbContext.IngestionLogs
                    .Where(c => !string.IsNullOrEmpty(c.Context))
                    .GroupBy(c => c.Context).Select(g => g.First()).ToList();

                var tenants = allTenants.Select(i => new SelectListItem { Value = i.Tenant, Text = i.Tenant }).ToList();
                var contexts = allContexts.Select(i => new SelectListItem { Value = i.Context, Text = i.Context }).ToList();

                var noTenant = new SelectListItem { Value = Helpers.Constants.IngestionLogsFiltersNoTenant, Text = Helpers.Constants.IngestionLogsFiltersNoTenant };
                var noContext = new SelectListItem { Value = Helpers.Constants.IngestionLogsFiltersNoContext, Text = Helpers.Constants.IngestionLogsFiltersNoContext };

                tenants.Insert(0, noTenant);
                contexts.Insert(0, noContext);

                return Task.FromResult(new LogViewModel
                {
                    Tenants = tenants,
                    Contexts = contexts
                });
            }
        }
    }
}
