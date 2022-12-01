// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using DataImport.Models;

namespace DataImport.TestHelpers
{
    public static class ResourceMetadataBuilder
    {
        public static ResourceMetadata Array(string name, ResourceMetadata itemMetadata)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = "array",
                Children = new List<ResourceMetadata> { itemMetadata }
            };
        }

        public static ResourceMetadata Object(string name, string dataType, params ResourceMetadata[] properties)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = dataType,
                Children = properties.ToList()
            };
        }

        public static ResourceMetadata RequiredProperty(string name, string dataType)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = dataType,
                Required = true
            };
        }

        public static ResourceMetadata Property(string name, string dataType)
        {
            return new ResourceMetadata
            {
                Name = name,
                DataType = dataType
            };
        }
    }
}
