// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Models;
using MediatR;

namespace DataImport.Web.Features.Lookup
{
    public class LookupIndex
    {
        public class ViewModel
        {
            public IEnumerable<IGrouping<string, LookupItem>> Lookups { get; set; }
        }

        public class LookupItem
        {
            public int Id { get; set; }

            public string SourceTable { get; set; }

            public string Key { get; set; }

            public string Value { get; set; }
        }

        public class Query : IRequest<ViewModel>
        {

        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;

            public QueryHandler(DataImportDbContext dataImportDbContext)
            {
                _dataImportDbContext = dataImportDbContext;
            }

            public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var lookupsFromDb = _dataImportDbContext.Lookups
                    .OrderBy(x => x.SourceTable)
                    .ThenBy(x => x.Key)
                    .ToList();

                var lookupsBySourceTable = lookupsFromDb
                    .Select(x => x.ToLookupItem())
                    .GroupBy(x => x.SourceTable);

                return Task.FromResult(new ViewModel
                {
                    Lookups = lookupsBySourceTable
                });
            }
        }
    }
}
