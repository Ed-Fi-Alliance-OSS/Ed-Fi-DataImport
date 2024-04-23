// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.DataMaps
{
    public class DataMapperFields
    {
        public class Query : IRequest<DataMapperFieldsViewModel>
        {
            public string ResourcePath { get; set; }
            public string[] ColumnHeaders { get; set; }
            public int ApiVersionId { get; set; }
            public bool IsDeleteOperation { get; set; }
            public bool IsDeleteByNaturalKey { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, DataMapperFieldsViewModel>
        {
            private readonly DataImportDbContext _database;

            public QueryHandler(DataImportDbContext database)
            {
                _database = database;
            }

            public Task<DataMapperFieldsViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var columnHeaders = request.ColumnHeaders;

                var resourceMetadata = GetResourceMetadata(request);

                return Task.FromResult(new DataMapperFieldsViewModel
                {
                    DataSources = MapDataSourcesTypesToViewModel(),
                    SourceTables = MapLookupTablesToViewModel(_database),
                    SourceColumns = MapCsvHeadersToSourceColumns(columnHeaders),
                    ResourceMetadata = resourceMetadata,
                    Mappings = request.IsDeleteOperation
                        ? request.IsDeleteByNaturalKey
                            ? InitialDeleteByNaturalKeyMappings(resourceMetadata)
                            : InitialDeleteByIdMappings()
                        : InitialMappings(resourceMetadata)
                });
            }

            private static List<DataMapper> InitialMappings(IEnumerable<ResourceMetadata> resourceMetadata)
            {
                return resourceMetadata.Select(x => x.BuildInitialMappings()).ToList();
            }

            private static List<DataMapper> InitialDeleteByIdMappings()
            {
                return new List<DataMapper>() { new DataMapper() { Name = "Id" } };
            }

            private static List<DataMapper> InitialDeleteByNaturalKeyMappings(IEnumerable<ResourceMetadata> resourceMetadata)
            {
                return resourceMetadata.Where(r => r.Required).Select(x => x.BuildInitialMappings()).ToList();
            }

            private ResourceMetadata[] GetResourceMetadata(Query request)
            {
                var resourceSelected = _database.Resources.SingleOrDefault(x => x.Path == request.ResourcePath && x.ApiVersionId == request.ApiVersionId);

                if (resourceSelected == null)
                    return new ResourceMetadata[] { };

                return ResourceMetadata.DeserializeFrom(resourceSelected);
            }
        }

        public static List<SelectListItem> MapCsvHeadersToSourceColumns(string[] columnHeaders)
        {
            return (columnHeaders ?? new string[] { }).ToSelectListItems("Select Source Column");
        }

        public static List<SelectListItem> MapDataSourcesTypesToViewModel()
        {
            return Sources.GetAll().ToSelectListItems("Select Data Source");
        }

        public static List<SelectListItem> MapLookupTablesToViewModel(DataImportDbContext dataImportDbContext)
        {
            return dataImportDbContext.Lookups
                .Select(x => x.SourceTable)
                .Distinct()
                .OrderBy(sourceTable => sourceTable)
                .ToArray()
                .ToSelectListItems("Select Source Table");
        }
    }
}
