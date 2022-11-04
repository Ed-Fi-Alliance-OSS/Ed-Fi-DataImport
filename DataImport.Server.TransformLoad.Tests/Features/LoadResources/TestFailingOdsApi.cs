// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Threading.Tasks;
using DataImport.Server.TransformLoad.Features.LoadResources;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    public class TestFailingOdsApi : IOdsApi
    {
        public static string ConfigUrlDefault = "http://test-ods-v2.5.0.1.example.com/api/v2.0/2019";

        public ApiConfig Config { get; set; } = new ApiConfig
            { ApiUrl = ConfigUrlDefault };

        public Task<OdsResponse> PostBootstrapData(string endpointUrl, string dataToInsert)
        {
            throw new Exception("Failed to POST bootstrap data. HTTP Status Code: 500");
        }

        public Task<OdsResponse> Post(string content, string endpointUrl, string postInfo = null)
        {
            return Task.FromResult(new OdsResponse(HttpStatusCode.InternalServerError, "An expected error has occurred."));
        }
    }
}
