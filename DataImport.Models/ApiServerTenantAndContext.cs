// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Models
{
    public class ApiServerTenantAndContext : ApiServer
    {
        public string Tenant { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }
}
