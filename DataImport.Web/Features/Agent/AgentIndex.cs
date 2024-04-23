// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Models;
using MediatR;

namespace DataImport.Web.Features.Agent
{
    public class AgentIndex
    {
        public class ViewModel
        {
            public List<AgentModel> Agents { get; set; }
        }

        public class AgentModel
        {
            public string Name { get; set; }
            public string AgentTypeCode { get; set; }
            public int FilesCount { get; set; }
            public DateTimeOffset? LastExecuted { get; set; }
            public bool Enabled { get; set; }
            public int Id { get; set; }
            public int? RunOrder { get; set; }
        }

        public class Query : IRequest<ViewModel>
        {

        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _database;

            public QueryHandler(DataImportDbContext database)
            {
                _database = database;
            }

            public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new ViewModel
                {
                    Agents = _database.Agents
                        .Where(x => !x.Archived)
                        .OrderBy(x => x.RunOrder == null)
                        .ThenBy(x => x.RunOrder)
                        .ThenBy(x => x.Id)
                        .Select(x => new AgentModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AgentTypeCode = x.AgentTypeCode,
                            LastExecuted = x.LastExecuted,
                            Enabled = x.Enabled,
                            FilesCount = x.Files.Count,
                            RunOrder = x.RunOrder,
                        })
                        .ToList()
                });
            }
        }
    }
}
