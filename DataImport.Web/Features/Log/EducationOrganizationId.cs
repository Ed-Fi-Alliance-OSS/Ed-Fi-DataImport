// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Log;

public class EducationOrganizationId
{
    public class Query : IRequest<LogViewModel>
    {

    }

    public class QueryHandler : IRequestHandler<Query, LogViewModel>
    {
        private readonly DataImportDbContext _dataImportDbContext;

        public QueryHandler(DataImportDbContext dataImportDbContext)
        {
            _dataImportDbContext = dataImportDbContext;
        }

        public Task<LogViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var allEducationOrganizationIds = _dataImportDbContext.IngestionLogs
                .Where(il => il.EducationOrganizationId.HasValue)
                .Select(il => new SelectListItem { Value = il.EducationOrganizationId.Value.ToString(), Text = il.EducationOrganizationId.Value.ToString() })
                .Distinct()
                .ToList();

            return Task.FromResult(new LogViewModel
            {
                EducationOrganizationIds = allEducationOrganizationIds,
            });
        }
    }
}
