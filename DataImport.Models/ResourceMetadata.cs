// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataImport.Models
{
    public class ResourceMetadata
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool Required { get; set; }
        public List<ResourceMetadata> Children { get; set; } = new List<ResourceMetadata>();

        public DataMapper BuildInitialMappings()
        {
            // The initial mappings are the same as the resource metadata,
            // except that all array nodes are left with zero initial items.

            return new DataMapper
            {
                Name = Name,
                Children = DataType == "array"
                    ? new List<DataMapper>()
                    : Children.Select(x => x.BuildInitialMappings()).ToList()
            };
        }

        public static string Serialize(ResourceMetadata[] metadata)
        {
            return JsonConvert.SerializeObject(metadata, Formatting.Indented);
        }

        public static ResourceMetadata[] DeserializeFrom(Resource resource)
        {
            return JsonConvert.DeserializeObject<ResourceMetadata[]>(resource.Metadata);
        }

        public static ResourceMetadata[] DeserializeFrom(DataMap dataMap)
        {
            return JsonConvert.DeserializeObject<ResourceMetadata[]>(dataMap.Metadata);
        }
    }
}
