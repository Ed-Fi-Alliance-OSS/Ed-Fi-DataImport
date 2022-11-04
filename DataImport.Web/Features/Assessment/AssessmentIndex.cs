// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Web.Features.Shared;
using DataImport.Web.Helpers;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Assessment
{
    public class AssessmentIndex
    {
        public class ViewModel: IApiServerListViewModel
        {
            public PagedList<Assessment> Assessments { get; set; }

            public class Assessment
            {
                public string Id { get; set; }

                public string Title { get; set; }

                public string CategoryDescriptor { get; set; }

                public string AcademicSubjectDescriptor { get; set; }

                public string AssessedGradeLevelDescriptor { get; set; }

                public string AssessmentIdentificationSystemDescriptor { get; set; }
            }

            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
        }

        public class Query : IRequest<ViewModel>, IApiServerSpecificRequest, IApiServerListViewModel
        {
            public int PageNumber { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly EdFiServiceManager _edFiServiceManager;
            private readonly IMapper _mapper;

            public QueryHandler(EdFiServiceManager edFiServiceManager, IMapper mapper)
            {
                _edFiServiceManager = edFiServiceManager;
                _mapper = mapper;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return new ViewModel
                {
                    Assessments = request.ApiServerId.HasValue ? await Page<ViewModel.Assessment>.FetchAsync(async (offset, limit) => await GetAssessments(request.ApiServerId.Value, offset, limit), request.PageNumber) : new PagedList<ViewModel.Assessment>()
                };
            }

            private async Task<List<ViewModel.Assessment>> GetAssessments(int apiServerId, int offset, int limit)
            {
                var pagedAssessments = (await _edFiServiceManager.GetResourceAssessments(apiServerId, offset, limit)).OrderBy(x => x.AssessmentTitle);

                return pagedAssessments.Select(x =>
                {
                    var mappedAssessment = _mapper.Map<ViewModel.Assessment>(x);

                    mappedAssessment.AssessmentIdentificationSystemDescriptor = x.IdentificationCodes.FirstOrDefault()
                        ?.AssessmentIdentificationSystemDescriptor.ToDescriptorName();

                    return mappedAssessment;
                }).ToList();
            }
        }
    }
}