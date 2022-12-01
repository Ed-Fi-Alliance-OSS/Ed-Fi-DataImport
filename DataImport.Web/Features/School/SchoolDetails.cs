// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.EdFi.Models.EnrollmentComposite;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.School
{
    public class SchoolDetails
    {
        public class Query : IRequest<Detail>, IApiServerSpecificRequest
        {
            public string Id { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class Detail : IApiServerListViewModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public PagedList<Student> Students { get; set; }
            public PagedList<Staff> Staves { get; set; }
            public PagedList<Section> Sections { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Detail>
        {
            private readonly EdFiServiceManager _edFiServiceManager;

            public QueryHandler(EdFiServiceManager edFiServiceManager)
            {
                _edFiServiceManager = edFiServiceManager;
            }

            public async Task<Detail> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.ApiServerId.HasValue)
                {
                    return new Detail();
                }

                const int PageNumber = 1;

                var id = request.Id;

                var school = await _edFiServiceManager.GetSchool(request.ApiServerId.Value, id);

                var pagedStudents =
                    await Page<Student>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetStudentsBySchoolId(request.ApiServerId.Value, id, offset, limit),
                        PageNumber);

                var pagedStaves =
                    await Page<Staff>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetStaffBySchoolId(request.ApiServerId.Value, id, offset, limit),
                        PageNumber);

                var pagedSections =
                    await Page<Section>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetSectionsBySchoolId(request.ApiServerId.Value, id, offset, limit),
                        PageNumber);

                return new Detail
                {
                    Id = school.Id,
                    Name = school.NameOfInstitution,
                    Students = pagedStudents,
                    Staves = pagedStaves,
                    Sections = pagedSections
                };
            }
        }
    }
}
