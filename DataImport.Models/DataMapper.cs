// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataImport.Models
{
    public class DataMapper
    {
        public DataMapper()
        {
            Children = new List<DataMapper>();
        }

        public string Name { get; set; }

        /// <summary>
        /// The column in the CSV that maps to this property
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SourceColumn { get; set; }

        /// <summary>
        /// The source table, or lookup table, that should be used to determine the final value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SourceTable { get; set; }

        /// <summary>
        /// The default value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Default { get; set; }

        /// <summary>
        /// The static value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        public List<DataMapper> Children { get; set; }

        public bool ContainsAnyMappedProperty()
        {
            if (SourceColumn != null || Value != null)
                return true;

            return Children.Any(child => child.ContainsAnyMappedProperty());
        }
    }
}
