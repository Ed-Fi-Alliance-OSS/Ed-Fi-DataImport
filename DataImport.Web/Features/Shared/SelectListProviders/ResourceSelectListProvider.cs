// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Web.Features.Shared.SelectListProviders
{
    public class ResourceSelectListProvider
    {
        private readonly DataImportDbContext _dbContext;

        public ResourceSelectListProvider(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<SelectListItem> GetResources(int apiVersionId)
        {
            var resources = new SelectListGroup { Name = ApiSection.Resources.ToDisplayName() };
            var descriptors = new SelectListGroup { Name = ApiSection.Descriptors.ToDisplayName() };

            Func<ResourceSelection, SelectListGroup> getGroup = resourceSelection =>
            {
                switch (resourceSelection.ApiSection)
                {
                    case ApiSection.Resources:
                        return resources;
                    case ApiSection.Descriptors:
                        return descriptors;
                    default:
                        return null;
                }
            };

            return _dbContext.Resources
                .Where(x => x.ApiVersionId == apiVersionId)
                .Select(x => new ResourceSelection
                {
                    ApiSection = x.ApiSection,
                    Value = x.Path,
                    Text = x.ToResourceName()
                })
                .ToList()
                .OrderBy(x => x.ApiSection)
                .ThenBy(x => x.Text)
                .ToList()
                .ToSelectListItems("Select Resource", x => x.Value, x => x.Text, getGroup)
                .ToList();
        }

        public List<SelectListItem> GetDescriptorsForLookup(int apiVersionId)
        {
            return _dbContext.Resources
                .Where(x => x.ApiVersionId == apiVersionId)
                .Select(x => new ResourceSelection
                {
                    ApiSection = x.ApiSection,
                    Value = x.Path,
                    Text = x.ToResourceName()
                })
                .ToList()
                .Where(x => x.ApiSection == ApiSection.Descriptors)
                .OrderBy(x => x.Text)
                .ToList()
                .ToSelectListItems("Select Descriptor", x => x.Value, x => x.Text)
                .ToList();
        }

        class ResourceSelection
        {
            public ApiSection ApiSection { set; get; }
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }
}