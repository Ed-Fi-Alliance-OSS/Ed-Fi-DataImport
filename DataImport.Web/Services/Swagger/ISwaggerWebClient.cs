// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net.Http;
using System.Threading.Tasks;

namespace DataImport.Web.Services.Swagger
{
    public interface ISwaggerWebClient
    {
        Task<string> DownloadString(string url);
    }

    public class SwaggerWebClient : ISwaggerWebClient
    {
        private readonly IHttpClientFactory _clientFactory;

        public SwaggerWebClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<string> DownloadString(string url)
        {
            var httpClient = _clientFactory.CreateClient();

            using (var response = await httpClient.GetAsync(url))
            {
                using (var content = response.Content)
                {
                    return await content.ReadAsStringAsync();
                }
            }
        }
    }
}
