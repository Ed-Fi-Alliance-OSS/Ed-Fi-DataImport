// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataImport.EdFi.Models.Resources;

namespace DataImport.Web.Features.School
{
    public class Index
    {
        public class Query : IRequest<ViewModel>, IApiServerSpecificRequest
        {
            public int PageNumber { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class ViewModel : IApiServerListViewModel
        {
            public PagedList<School> Schools { get; set; }

            public class School
            {
                public string Id { get; set; }
                public string LocalEducationAgencyId { get; set; }
                public string Name { get; set; }
                public string Abbreviation { get; set; }
                public string District { get; set; }
                public int StaffCount { get; set; }
                public int StudentCount { get; set; }
                public string LastIngestionDate { get; set; }
            }

            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly EdFiServiceManager _edFiServiceManager;

            public QueryHandler(EdFiServiceManager edFiServiceManager)
            {
                _edFiServiceManager = edFiServiceManager;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.ApiServerId.HasValue)
                {
                    return new ViewModel();
                }

                return new ViewModel
                {
                    Schools = await Page<ViewModel.School>.FetchAsync(async (offset, limit) => (IReadOnlyList<ViewModel.School>) await GetSchools(request.ApiServerId.Value, offset, limit), request.PageNumber)
                };
            }

            private async Task<IEnumerable<ViewModel.School>> GetSchools(int apiServerId, int offset, int limit)
            {
                var pagedSchools = await _edFiServiceManager.GetSchools(apiServerId, offset, limit);

                var districtIds = pagedSchools
                    .Where(x => x.LocalEducationAgency != null)
                    .Select(x => x.LocalEducationAgency.Id)
                    .Distinct();

                var districts = new List<LocalEducationAgency>();
                foreach (var districtId in districtIds)
                {
                    var district = await _edFiServiceManager.GetLocalEducationAgencyById(apiServerId, districtId);
                    districts.Add(district);
                }

                var schools = pagedSchools.Select(school => new ViewModel.School
                {
                    Id = school.Id,
                    Name = school.NameOfInstitution,
                    Abbreviation = school.ShortNameOfInstitution,
                    District = districts.FirstOrDefault(d => d.Id == school.LocalEducationAgency?.Id)
                                   ?.NameOfInstitution ??
                               string.Empty
                });

                return schools.OrderBy(x => x.District).ThenBy(x => x.Name).ToList();
            }
        }
    }
}
