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
        Task FillSwaggerMetadata(ApiServer apiServer);
        bool AllowUserRegistrations();
        Task<string> GetTokenUrl(string apiUrl, string apiVersion);
        Task<string> GetAuthUrl(string apiUrl, string apiVersion);
        Task<string> InferOdsApiVersion(string apiUrl);
    }
}
