// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataImport.Web.Services.Swagger
{
    public interface ISwaggerMetadataFetcher
    {
        Task<IEnumerable<SwaggerResource>> GetMetadata(string apiUrl, string apiVersion, string tenant, string context);
        Task<string> GetTokenUrl(string apiUrl, string apiVersion, string tenant, string context);
        Task<string> GetAuthUrl(string apiUrl, string apiVersion, string tenant, string context);
        Task<string> InferOdsApiVersion(string apiUrl);
        Task<string> GetYearSpecificYear(string apiUrl);
        Task<string> GetInstanceYearSpecificInstance(string apiUrl);
        Task<string> GetInstanceYearSpecificYear(string apiUrl);
    }
}
