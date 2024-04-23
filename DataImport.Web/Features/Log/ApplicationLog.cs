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
using DataImport.Web.Services;
using MediatR;

namespace DataImport.Web.Features.Log
{
    public class ApplicationLog
    {
        public class Query : IRequest<LogViewModel>
        {
            public int PageNumber { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, LogViewModel>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IMapper _mapper;


            public QueryHandler(DataImportDbContext dataImportDbContext, IMapper mapper)
            {
                _dataImportDbContext = dataImportDbContext;
                _mapper = mapper;
            }

            public Task<LogViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var pagedIngestionLogs =
                    Page<LogViewModel.ApplicationLog>.Fetch(GetApplicationLogs, request.PageNumber);

                return Task.FromResult(new LogViewModel { ApplicationLogs = pagedIngestionLogs });
            }

            public IEnumerable<LogViewModel.ApplicationLog> GetApplicationLogs(int offset, int limit)
            {
                var pagedList = _dataImportDbContext.ApplicationLogs
                    .OrderByDescending(x => x.Logged).Skip(offset).Take(limit).ToList();

                return pagedList.Select(_mapper.Map<LogViewModel.ApplicationLog>);
            }

        }
    }
}
