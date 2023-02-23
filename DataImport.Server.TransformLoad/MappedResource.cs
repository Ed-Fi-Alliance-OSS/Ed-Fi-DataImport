// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Newtonsoft.Json.Linq;

namespace DataImport.Server.TransformLoad
{
    public class MappedResource
    {
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public string ApiServerName { get; set; }
        public string ApiVersion { get; set; }
        public string FileName { get; set; }
        public string ResourcePath { get; set; }
        public string Metadata { get; set; }
        public int RowNumber { get; set; }
        public JToken Value { get; set; }
    }
}
