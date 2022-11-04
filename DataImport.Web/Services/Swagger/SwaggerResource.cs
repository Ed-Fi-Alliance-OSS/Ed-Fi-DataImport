// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Services.Swagger
{
    public class SwaggerResource
    {
        public string SwaggerVersion { get; set; }
        public string Metadata { get; set; }
        public string Path { get; set; }
        public ApiSection ApiSection { get; set; }
    }
}