// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DataImport.Server.TransformLoad.Features.LoadResources;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    public class TestOdsApi : IOdsApi
    {
        public TestOdsApi()
        {
            PostedBootstrapData = new List<SimulatedPost>();
            PostedContent = new List<SimulatedPost>();
            DeletedContent = new List<SimulatedDelete>();
        }

        public List<SimulatedPost> PostedContent { get; }

        public List<SimulatedDelete> DeletedContent { get; }

        public List<SimulatedPost> PostedBootstrapData { get; }

        public Task<OdsResponse> Delete(string id, string endpointUrl)
        {
            DeletedContent.Add(new SimulatedDelete(endpointUrl, id));

            return Task.FromResult(new OdsResponse(HttpStatusCode.NoContent, string.Empty));
        }

        public ApiConfig Config { get; set; } = new ApiConfig
        { ApiUrl = "http://test-ods-v2.5.0.1.example.com/api/v2.0/2019" };

        public Task<OdsResponse> PostBootstrapData(string endpointUrl, string dataToInsert)
        {
            PostedBootstrapData.Add(new SimulatedPost(endpointUrl, dataToInsert));

            return Task.FromResult(new OdsResponse(HttpStatusCode.OK, string.Empty));
        }

        public Task<OdsResponse> Post(string content, string endpointUrl, string postInfo = null)
        {
            PostedContent.Add(new SimulatedPost(endpointUrl, content));

            return Task.FromResult(new OdsResponse(HttpStatusCode.OK, string.Empty));
        }

        public class SimulatedPost
        {
            public SimulatedPost(string endpointUrl, string body)
            {
                EndpointUrl = endpointUrl;
                Body = body;
            }

            public string EndpointUrl { get; set; }
            public string Body { get; set; }
        }

        public class SimulatedDelete
        {
            public SimulatedDelete(string endpointUrl, string id)
            {
                EndpointUrl = endpointUrl;
                Id = id;
            }

            public string EndpointUrl { get; set; }
            public string Id { get; set; }
        }
    }
}
