// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Preprocessor
{
    public class PreprocessorIndex
    {
        public class ViewModel
        {
            public IList<PreprocessorIndexModel> Preprocessors { get; set; }
        }

        public class Query : IRequest<ViewModel>
        {
        }

        public class PreprocessorIndexModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ScriptType ScriptType { get; set; }
            public List<SelectListItem> UsedBy { get; set; }
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
                var preprocessors = await _database.Scripts.
                    Select(x => new { Preprocessor = x, DataMaps = x.DataMaps.Select(m => new { m.Id, m.Name }).ToList() })
                    .ToListAsync(cancellationToken);

                var agents = await _database.Agents.Where(x => x.FileGeneratorScriptId.HasValue || x.RowProcessorScriptId.HasValue)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var preprocessorModels = preprocessors
                    .Select(x =>
                    {
                        var preprocessor = _mapper.Map<PreprocessorIndexModel>(x.Preprocessor);

                        if (preprocessor.ScriptType == ScriptType.CustomFileProcessor)
                        {
                            preprocessor.UsedBy = x.DataMaps.Select(m => new SelectListItem
                            {
                                Value = m.Id.ToString(CultureInfo.InvariantCulture),
                                Text = m.Name,
                                Group = new SelectListGroup
                                {
                                    Name = "Data Maps"
                                }
                            }).ToList();
                        }
                        else
                        {
                            preprocessor.UsedBy = agents.Where(a => a.FileGeneratorScriptId == x.Preprocessor.Id || a.RowProcessorScriptId == x.Preprocessor.Id).Select(m => new SelectListItem
                            {
                                Value = m.Id.ToString(CultureInfo.InvariantCulture),
                                Text = m.Name,
                                Group = new SelectListGroup
                                {
                                    Name = "Agents"
                                }
                            }).ToList();
                        }

                        return preprocessor;
                    })
                    .OrderBy(x => x.Name)
                    .ToList();

                return new ViewModel
                {
                    Preprocessors = preprocessorModels
                };
            }
        }
    }
}
