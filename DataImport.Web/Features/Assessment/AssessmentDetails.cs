// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.EdFi.Models.Resources;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Assessment
{
    public class AssessmentDetails
    {
        public class Query : IRequest<AssessmentDetail>, IApiServerSpecificRequest
        {
            public string Id { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class AssessmentDetail : IApiServerListViewModel
        {
            public string Id { get; set; }

            [Display(Name = "Assessment Category Descriptor")]
            public string AssessmentCategoryDescriptor { get; set; }

            [Display(Name = "Assessment Identifier")]
            public string AssessmentIdentifier { get; set; }

            [Display(Name = "Assessment Title")]
            public string AssessmentTitle { get; set; }

            public string Namespace { get; set; }

            [Display(Name = "Assessment Version")]
            public int? AssessmentVersion { get; set; }

            [Display(Name = "Academic Subjects")]
            public string AcademicSubjects { get; set; }

            [Display(Name = "Assessed Grade Levels")]
            public string AssessedGradeLevels { get; set; }

            [Display(Name = "Identification Systems")]
            public string IdentificationCodes { get; set; }

            public List<AssessmentPerformanceLevel> PerformanceLevels { get; set; }

            public PagedList<ObjectiveAssessment> ObjectiveAssessments { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, AssessmentDetail>
        {
            private readonly EdFiServiceManager _edFiServiceManager;
            private readonly IMapper _mapper;

            public QueryHandler(EdFiServiceManager edFiServiceManager, IMapper mapper)
            {
                _edFiServiceManager = edFiServiceManager;
                _mapper = mapper;
            }

            public async Task<AssessmentDetail> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.ApiServerId.HasValue)
                {
                    return new AssessmentDetail();
                }

                const int PageNumber = 1;

                var id = request.Id;

                var assessment = await _edFiServiceManager.GetAssessmentById(request.ApiServerId.Value, id);

                var assessmentDetail = _mapper.Map<AssessmentDetail>(assessment);
                assessmentDetail.ObjectiveAssessments = await Page<ObjectiveAssessment>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetObjectiveAssessmentsByAssessment(request.ApiServerId.Value, assessment, offset, limit),
                    PageNumber, 10);

                return assessmentDetail;
            }
        }
    }
}
