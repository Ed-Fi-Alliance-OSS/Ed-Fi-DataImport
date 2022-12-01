// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Features.Shared.SelectListProviders;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DataImport.Web.Features.Shared
{
    public class RetrieveResources
    {
        public class ViewModel
        {
            public List<SelectListItem> Resources { get; set; }
        }

        public class Query : IRequest<ViewModel>
        {
            public int ApiVersionId { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, ViewModel>
        {
            private readonly ResourceSelectListProvider _resourceProvider;

            public QueryHandler(ResourceSelectListProvider resourceProvider)
            {
                _resourceProvider = resourceProvider;
            }

            protected override ViewModel Handle(Query request)
            {
                return new ViewModel
                {
                    Resources = _resourceProvider.GetResources(request.ApiVersionId)
                };
            }
        }
    }
}
