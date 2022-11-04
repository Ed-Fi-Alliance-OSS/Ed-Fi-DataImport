// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataImport.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataImport.Web.Features.ApiServers
{
    public class ApiServerIndex
    {
        public class ViewModel
        {
            public IList<ApiServerModel> ApiServers { get; set; }
        }

        public class ApiServerModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ApiVersion { get; set; }
            public string Url { get; set; }
        }

        public class Query : IRequest<ViewModel>
        {
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var apiServers = await _database.ApiServers.Include(x => x.ApiVersion).AsNoTracking().ToListAsync(cancellationToken);

                return new ViewModel
                {
                    ApiServers = apiServers.Select(x => _mapper.Map<ApiServerModel>(x)).OrderBy(x => x.Name).ToList()
                };
            }
        }
    }
}