// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using DataImport.Models;

namespace DataImport.Web.Services
{
    public interface IConfigurationService
    {
        Task FillSwaggerMetadata(ApiServerTenantAndContext apiServer);
        bool AllowUserRegistrations();
        Task<string> GetTokenUrl(string apiUrl, string apiVersion, string tenant, string context);
        Task<string> GetAuthUrl(string apiUrl, string apiVersion, string tenant, string context);
        Task<string> InferOdsApiVersion(string apiUrl);
    }
}
