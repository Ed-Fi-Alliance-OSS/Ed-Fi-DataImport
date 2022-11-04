// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using DataImport.Web.Services.Swagger;

namespace DataImport.Web.Tests.Services.Swagger
{
    public class MockSwaggerWebClient : ISwaggerWebClient
    {
        private readonly IDictionary<string, string> _setups;

        public MockSwaggerWebClient()
        {
            _setups = new Dictionary<string, string>();
        }

        public void Setup(string url, string result)
        {
            _setups.Add(url, result);
        }

        public async Task<string> DownloadString(string url)
        {
            return await Task.FromResult(_setups[url]);
        }
    }
}