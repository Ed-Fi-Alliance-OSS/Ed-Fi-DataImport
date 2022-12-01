// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class ApiConfig
    {
        public int ApiServerId { get; set; }
        public string ApiUrl { get; set; }
        public string AuthorizeUrl { get; set; }
        public string AccessTokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiVersion { get; set; }
        public string Name { get; set; }
    }
}
