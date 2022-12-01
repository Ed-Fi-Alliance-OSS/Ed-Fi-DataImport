// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.EdFi;
using DataImport.EdFi.Models;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.DataMaps
{
    public class RetrieveDescriptors
    {
        public class Query : IRequest<ViewModel>, IApiServerSpecificRequest
        {
            public int? ApiServerId { get; set; }
            public string DescriptorName { get; set; }
            public int PageNumber { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class ViewModel : IApiServerListViewModel
        {
            public string DescriptorName { get; set; }
            public PagedList<Descriptor> Descriptors { get; set; }
            public string ApiVersion { get; set; }
            public List<SelectListItem> AvailableDescriptors { get; set; }
            public bool DescriptorsFound { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            [Display(Name = "")]
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly EdFiServiceManager _edFiServiceManager;
            private readonly DataImportDbContext _dbContext;
            private readonly ResourceSelectListProvider _resourceProvider;


            public QueryHandler(EdFiServiceManager edFiServiceManager, DataImportDbContext dbContext, ResourceSelectListProvider resourceProvider)
            {
                _edFiServiceManager = edFiServiceManager;
                _dbContext = dbContext;
                _resourceProvider = resourceProvider;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.ApiServerId.HasValue)
                {
                    return new ViewModel
                    {
                        DescriptorsFound = false,
                        ApiVersionId = request.ApiVersionId
                    };
                }

                const int pageSize = 10;

                var pageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;

                var apiServer = _dbContext.ApiServers.Include(x => x.ApiVersion).AsNoTracking().Single(x => x.Id == request.ApiServerId);

                var manualDescriptorLookupViewModel = new ViewModel
                {
                    AvailableDescriptors = _resourceProvider.GetDescriptorsForLookup(apiServer.ApiVersionId),
                    ApiVersion = apiServer.ApiVersion.Version,
                    DescriptorsFound = false,
                    ApiVersionId = request.ApiVersionId
                };

                if (string.IsNullOrEmpty(request.DescriptorName))
                    return manualDescriptorLookupViewModel;

                try
                {
                    var descriptorPath = GetDescriptorPath(request.DescriptorName, apiServer.ApiVersion.Version);

                    var pagedDescriptors = await Page<Descriptor>.FetchAsync(async (offset, limit) => await _edFiServiceManager.GetDescriptors(apiServer.Id, descriptorPath, offset, limit), pageNumber, pageSize);

                    return new ViewModel
                    {
                        DescriptorName = request.DescriptorName,
                        Descriptors = pagedDescriptors,
                        ApiVersion = apiServer.ApiVersion.Version,
                        DescriptorsFound = true,
                        ApiVersionId = request.ApiVersionId
                    };
                }
                catch (DescriptorNotFoundException)
                {
                    return manualDescriptorLookupViewModel;
                }
            }

            protected string GetDescriptorPath(string descriptorName, string apiVersion)
            {
                var descriptorPath = descriptorName.Trim('/');

                if (!apiVersion.IsOdsV2() && !descriptorPath.Contains('/'))
                    descriptorPath = $"ed-fi/{descriptorPath}";

                if (!descriptorPath.EndsWith("s"))
                    descriptorPath += "s";

                return descriptorPath;
            }
        }
    }
}
