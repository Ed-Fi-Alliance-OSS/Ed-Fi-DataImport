// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.DataMaps;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    public abstract class DataMapperFieldsTestBase
    {
        protected ApiVersion ApiVersion;

        protected readonly List<SelectListItem> DataSources = new List<SelectListItem>
        {
            new SelectListItem { Text = "Select Data Source", Value = "" },
            new SelectListItem { Text = "column", Value = "column" },
            new SelectListItem { Text = "lookup-table", Value = "lookup-table" },
            new SelectListItem { Text = "static", Value = "static" },
        };

        protected readonly List<SelectListItem> EmptySourceColumns = new List<SelectListItem>
        {
            new SelectListItem { Text = "Select Source Column", Value = "" }
        };

        [OneTimeSetUp]
        public async Task Init()
        {
            await SetUpResources();

            ApiVersion = GetDefaultApiVersion();
        }

        public abstract Task SetUpResources();

        [Test]
        public async Task ShouldGetMinimalViewModelWhenNoResourceSelected()
        {
            var response = await Send(new DataMapperFields.Query
            {
                ResourcePath = null,
                ColumnHeaders = null
            });

            var populatedLookups = Query(DataMapperFields.MapLookupTablesToViewModel);

            response.ShouldMatch(new DataMapperFieldsViewModel
            {
                ResourceMetadata = new List<ResourceMetadata>(),
                Mappings = new List<DataMapper>(),
                DataSources = DataSources,
                SourceTables = populatedLookups,
                SourceColumns = EmptySourceColumns
            });
        }

        public abstract Task ShouldGetUserFacingSourceColumnSelectionListGivenRepresentativeCsvHeaders();

        public abstract Task ShouldGetUserFacingMappableFieldsForStudentsResource();

        public abstract Task ShouldGetUserFacingMappableFieldsForStudentAssessmentsResource();

        [Test]
        public async Task ShouldGetResourceMetadataIfResourcesForMultipleApiVersionsAreAvailable()
        {
            await AddApiServer(StubSwaggerWebClient.ApiServerUrlV311, OdsApiV311);
            await AddApiServer(StubSwaggerWebClient.ApiServerUrlV311, "3.1.2"); // This will create resources with the same path but different api version
            await AddApiServer(StubSwaggerWebClient.ApiServerUrlV25, OdsApiV25);

            var resourceApiVersions = Query(d => d.Resources.Select(x => x.ApiVersionId).Distinct().ToList());
            resourceApiVersions.Count.ShouldBeGreaterThan(1);

            foreach (var apiVersionId in resourceApiVersions)
            {
                var resource = RandomResource(apiVersionId);

                var viewModel = await Send(new DataMapperFields.Query
                {
                    ResourcePath = resource.Path,
                    ColumnHeaders = null,
                    ApiVersionId = apiVersionId
                });

                viewModel.ResourceMetadata.ShouldNotBeEmpty();
                viewModel.Mappings.ShouldNotBeEmpty();
            }
        }

        [Test]
        public async Task SmokeTestFieldsForKnownResources()
        {
            //NOTE: Unlike most tests, we are not making specific assertions here. This test is useful
            //      for uncovering unanticipated edge cases by exercising Swagger metadata processing
            //      for many representative resources. For instance, inspect debugging log entries output
            //      while running this test.

            var resourcePaths = Query(db => db.Resources.Where(x => x.ApiVersionId == ApiVersion.Id).Select(x => x.Path).ToArray());

            foreach (var resourcePath in resourcePaths)
            {
                var response = await GetDataMapperFields(resourcePath);
                response.ResourceMetadata.ShouldNotBeEmpty();
                response.Mappings.ShouldNotBeEmpty();
            }
        }

        protected async Task<DataMapperFieldsViewModel> GetDataMapperFields(string resourcePath, string[] columnHeaders = null)
        {
            var resource = Query(database => database.Resources.SingleOrDefault(x => x.Path == resourcePath && x.ApiVersionId == ApiVersion.Id));

            resource.ShouldNotBeNull($"Resources table does not contain a definition for resource '{resourcePath}'.");

            return await Send(new DataMapperFields.Query
            {
                ResourcePath = resource.Path,
                ColumnHeaders = columnHeaders,
                ApiVersionId = resource.ApiVersionId
            });
        }

        protected static ResourceMetadata FieldMetadata(string name, string dataType, params ResourceMetadata[] children)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = dataType,
                Children = children.ToList()
            };
        }

        protected static ResourceMetadata RequiredFieldMetadata(string name, string dataType, params ResourceMetadata[] children)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = dataType,
                Required = true,
                Children = children.ToList()
            };
        }

        protected static DataMapper Field(string name, params DataMapper[] children)
        {
            return new DataMapper
            {
                Name = name,
                Children = children.ToList()
            };
        }
    }
}
