// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;

namespace DataImport.Web.Features.Log
{
    public class FilesLog
    {
        public class Query : IRequest<LogViewModel>, IApiServerSpecificRequest
        {
            public int PageNumber { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
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
                var pagedFileLogs =
                    Page<LogViewModel.File>.Fetch((offset, limit) => GetFileLogs(request.ApiServerId, offset, limit), request.PageNumber);
                return new LogViewModel { Files = pagedFileLogs };
            }

            public IEnumerable<LogViewModel.File> GetFileLogs(int? apiServerId, int offset, int limit)
            {
                var pagedList =
                    _dataImportDbContext.Files
                        .Include(x => x.Agent)
                        .ThenInclude(x => x.ApiServer)
                        .Where(x => !apiServerId.HasValue || apiServerId.HasValue && x.Agent.ApiServerId == apiServerId.Value)
                        .OrderByDescending(x => x.CreateDate)
                        .Skip(offset)
                        .Take(limit).ToList();
                return pagedList.Select(_mapper.Map<LogViewModel.File>);
            }
        }
    }
}