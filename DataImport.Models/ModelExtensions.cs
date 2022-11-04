// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using Newtonsoft.Json.Linq;

namespace DataImport.Models
{
    public static class ModelExtensions
    {
        public static bool MetadataIsIncompatible(this DataMap entity, DataImportDbContext database)
        {
            string discardErrorDetails;
            return MetadataIsIncompatible(entity.Map, entity.ResourcePath, database, MetadataCompatibilityLevel.DataMap, entity.ApiVersionId, out discardErrorDetails);
        }

        public static bool MetadataIsIncompatible(this DataMap entity, DataImportDbContext database, out string errorDetails)
        {
            return MetadataIsIncompatible(entity.Map, entity.ResourcePath, database, MetadataCompatibilityLevel.DataMap, entity.ApiVersionId, out errorDetails);
        }

        public static bool MetadataIsIncompatible(this BootstrapData entity, DataImportDbContext database)
        {
            string discardErrorDetails;
            return MetadataIsIncompatible(entity.Data, entity.ResourcePath, database, MetadataCompatibilityLevel.Bootstrap, entity.ApiVersionId, out discardErrorDetails);
        }

        public static bool MetadataIsIncompatible(this BootstrapData entity, DataImportDbContext database, out string errorDetails)
        {
            return MetadataIsIncompatible(entity.Data, entity.ResourcePath, database, MetadataCompatibilityLevel.Bootstrap, entity.ApiVersionId, out errorDetails);
        }

        private static bool MetadataIsIncompatible(string json, string resourcePath, DataImportDbContext database, MetadataCompatibilityLevel level,  int apiVersionId, out string errorDetails)
        {
            var resource = database.Resources.SingleOrDefault(x => x.Path == resourcePath && x.ApiVersionId == apiVersionId);

            errorDetails = null;
            return resource == null || !JsonCompatibleWithResource(json, resource, level, out errorDetails);
        }

        private static bool JsonCompatibleWithResource(string json, Resource resource, MetadataCompatibilityLevel level, out string errorDetails)
        {
            return JToken.Parse(json).IsCompatibleWithResource(resource, level, out errorDetails);
        }
    }
}
