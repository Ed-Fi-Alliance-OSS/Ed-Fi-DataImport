// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.EdFi.Models.Resources;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Assessment
{
    public class ObjectiveAssessmentDetails
    {
        public class ViewModel : IApiServerListViewModel
        {
            public string Id { get; set; }
            public PagedList<ObjectiveAssessment> ObjectiveAssessments { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
        }

        public class Query : IRequest<ViewModel>, IApiServerSpecificRequest
        {
            public string Id { get; set; }
            public int PageNumber { get; set; }
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
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

                var id = request.Id;

                var assessment = await _edFiServiceManager.GetAssessmentById(request.ApiServerId.Value, id);

                var pagedObjectiveAssessments = await Page<ObjectiveAssessment>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetObjectiveAssessmentsByAssessment(request.ApiServerId.Value, assessment, offset, limit),
                        request.PageNumber, 10);

                return new ViewModel { Id = request.Id, ObjectiveAssessments = pagedObjectiveAssessments };
            }
        }
    }
}
