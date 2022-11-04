// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using AutoMapper;
using DataImport.Models;
using MediatR;

namespace DataImport.Web.Features.BootstrapData
{
    public class BootstrapDataIndex
    {
        public class ViewModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ResourceName { get; set; }
        }

        public class Query : IRequest<ViewModel[]>
        {
        }

        public class QueryHandler : RequestHandler<Query, ViewModel[]>
        {
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override ViewModel[] Handle(Query request)
            {
                return _database.BootstrapDatas
                        .OrderBy(x => x.Name)
                        .ToList()
                        .Select(x => _mapper.Map<ViewModel>(x))
                        .ToArray();
            }
        }
    }
}