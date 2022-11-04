// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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

        public class QueryHandler : RequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext dataImportDbContext, IMapper mapper)
            {
                _dataImportDbContext = dataImportDbContext;
                _mapper = mapper;
            }

            protected override ViewModel Handle(Query request)
            {
                var lookupsFromDb = _dataImportDbContext.Lookups
                    .OrderBy(x => x.SourceTable)
                    .ThenBy(x => x.Key)
                    .ToList();

                var lookupsBySourceTable = lookupsFromDb
                    .Select(x => _mapper.Map<LookupItem>(x))
                    .GroupBy(x => x.SourceTable);

                return new ViewModel
                {
                    Lookups = lookupsBySourceTable
                };
            }
        }
    }
}