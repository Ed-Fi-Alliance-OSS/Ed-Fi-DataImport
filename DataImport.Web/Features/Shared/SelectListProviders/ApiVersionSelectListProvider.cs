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
    public class ApiVersionSelectListProvider
    {
        private readonly DataImportDbContext _dbContext;

        public ApiVersionSelectListProvider(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SelectListItem>> GetApiVersions(CancellationToken cancellationToken)
        {
            return
                (await _dbContext.ApiVersions
                    .Select(x => new
                    {
                        x.Version,
                        x.Id
                    })
                    .OrderBy(x => x.Version)
                    .ToListAsync(cancellationToken)
                )
                .ToSelectListItems("Select API Version", x => x.Id.ToString(CultureInfo.InvariantCulture), x => x.Version)
                .ToList();
        }
    }
}