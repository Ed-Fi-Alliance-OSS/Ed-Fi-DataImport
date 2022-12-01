// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AutoMapper;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Services;
using MediatR;

namespace DataImport.Web.Features.Log
{
    public class IngestionLog
    {
        public class Query : IRequest<LogViewModel>
        {
            public LogViewModel.Filters LogFilters { get; set; }

            public int PageNumber { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, LogViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext dataImportDbContext, IMapper mapper)
            {
                _dataImportDbContext = dataImportDbContext;
                _mapper = mapper;
            }

            protected override LogViewModel Handle(Query request)
            {
                var pagedIngestionLogs =
                    Page<LogViewModel.Ingestion>.Fetch(
                        (offset, limit) => GetIngestionLogs(request.LogFilters, offset, limit),
                        request.PageNumber);

                return new LogViewModel
                { LogFilters = request.LogFilters, IngestionLogs = pagedIngestionLogs };
            }

            public IEnumerable<LogViewModel.Ingestion> GetIngestionLogs(LogViewModel.Filters filters, int offset, int limit)
            {
                var logsByDateDesc = _dataImportDbContext.IngestionLogs.OrderByDescending(x => x.Date);
                if (filters != null)
                {
                    if (filters.SelectedResult > 0)
                    {
                        logsByDateDesc =
                            (IOrderedQueryable<DataImport.Models.IngestionLog>) logsByDateDesc.Where(x =>
                                (int) x.Result == filters.SelectedResult);
                    }
                    if (filters.SelectedResponse > 0)
                    {
                        if (filters.SelectedResponse == 1)
                        {
                            var usefulResponses = Enum.GetValues(typeof(EdFiHttpStatus)).Cast<EdFiHttpStatus>()
                                .ToList().Select(x => x.ToString().ToLower());
                            var otherHttpResponses = Enum.GetValues(typeof(HttpStatusCode)).Cast<HttpStatusCode>()
                                .ToList().Select(x => x.ToString().ToLower()).Except(usefulResponses);
                            logsByDateDesc =
                                (IOrderedQueryable<DataImport.Models.IngestionLog>) logsByDateDesc.Where(x =>
                                    otherHttpResponses.Contains(x.HttpStatusCode));
                        }
                        else
                        {
                            var statusCode = Enum.GetName(typeof(EdFiHttpStatus), filters.SelectedResponse);
                            logsByDateDesc = (IOrderedQueryable<DataImport.Models.IngestionLog>) logsByDateDesc.Where(x =>
                                x.HttpStatusCode == statusCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(filters.Filename))
                    {
                        logsByDateDesc = (IOrderedQueryable<DataImport.Models.IngestionLog>) logsByDateDesc.Where(x =>
                            x.FileName.ToLower().Contains(filters.Filename));
                    }
                }
                var pagedList = logsByDateDesc.Skip(offset).Take(limit).ToList();
                return pagedList.Select(_mapper.Map<LogViewModel.Ingestion>);
            }
        }
    }
}
