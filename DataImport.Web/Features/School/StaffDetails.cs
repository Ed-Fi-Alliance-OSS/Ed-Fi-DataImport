// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using DataImport.EdFi.Models.EnrollmentComposite;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;

namespace DataImport.Web.Features.School
{
    public class StaffDetails
    {
        public class Query : IRequest<SchoolDetails.Detail>, IApiServerSpecificRequest
        {
            public string Id { get; set; }
            public int PageNumber { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, SchoolDetails.Detail>
        {
            private readonly EdFiServiceManager _edFiServiceManager;

            public QueryHandler(EdFiServiceManager edFiServiceManager)
            {
                _edFiServiceManager = edFiServiceManager;
            }

            public async Task<SchoolDetails.Detail> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.ApiServerId.HasValue)
                {
                    return new SchoolDetails.Detail();
                }

                var id = request.Id;

                var pagedStaves = await Page<Staff>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetStaffBySchoolId(request.ApiServerId.Value, id, offset, limit),
                        request.PageNumber);

                return new SchoolDetails.Detail { Id = request.Id, Staves = pagedStaves };
            }
        }
    }
}