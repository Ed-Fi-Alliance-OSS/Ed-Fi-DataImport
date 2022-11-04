// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Shared.SelectListProviders
{
    public class ApiServerSelectListProvider
    {
        private readonly DataImportDbContext _dbContext;

        public ApiServerSelectListProvider(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SelectListItem>> GetApiServers(CancellationToken cancellationToken)
        {
            var apiServers = await _dbContext.ApiServers
                .Select(x => new
                {
                    x.Name,
                    x.Id,
                    x.ApiVersion.Version
                })
                .ToListAsync(cancellationToken);

            var apiVersionGroups = apiServers.Select(x => x.Version).Distinct().Select(x => new SelectListGroup { Name = x }).OrderBy(x => x.Name).ToList();

            return apiServers
                .ToSelectListItems("Select API Connection", x => x.Id.ToString(CultureInfo.InvariantCulture), x => x.Name, apiServer =>
                {
                    return apiVersionGroups.First(x => x.Name == apiServer.Version);
                })
                .OrderBy(x => $"{(x.Group == null ? "0" : x.Group.Name)}-{x.Text}")
                .ToList();
        }
    }
}