// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Helpers;
using Newtonsoft.Json.Linq;

namespace DataImport.Web.Services.Swagger
{
    public class SwaggerMetadataProcessorV1 : ISwaggerMetadataProcessor
    {
        private readonly ISwaggerWebClient _swaggerWebClient;

        public SwaggerMetadataProcessorV1(ISwaggerWebClient swaggerWebClient)
        {
            _swaggerWebClient = swaggerWebClient;
        }

        public bool CanHandle(JObject swaggerDocument)
        {
            var version = GetSwaggerDocumentVersion(swaggerDocument);
            return version == "1.2";
        }

        private static string GetSwaggerDocumentVersion(JObject swaggerDocument)
        {
            return swaggerDocument.Value<string>("swaggerVersion");
        }

        public async Task<List<SwaggerResource>> GetMetadata(JObject swaggerDocument, ApiSection apiSection)
        {
            var resources = FetchMetadataAsync(swaggerDocument, apiSection);
            return await resources.ToListAsync();
        }

        private async IAsyncEnumerable<SwaggerResource> FetchMetadataAsync(JObject swaggerDocument, ApiSection apiSection)
        {
            var basePath = swaggerDocument.Value<string>("basePath");
            var resources = swaggerDocument["apis"];
            var swaggerVersion = GetSwaggerDocumentVersion(swaggerDocument);

            foreach (var resource in resources)
            {
                var resourcePath = resource.Value<string>("path");
                var resourceMetadataUrl = $"{basePath}{resourcePath}";

                var resourceMetadata = await _swaggerWebClient.DownloadString(resourceMetadataUrl);

                yield return new SwaggerResource
                {
                    Metadata = resourceMetadata,
                    SwaggerVersion = swaggerVersion,
                    Path = resourcePath,
                    ApiSection = apiSection
                };
            }
        }

        public string GetTokenUrl(JObject swaggerDocument)
            => swaggerDocument["authorizations"]["edfi"].Value<string>("tokenUrl");

        public string GetAuthUrl(JObject swaggerDocument)
            => swaggerDocument["authorizations"]["edfi"].Value<string>("authUrl");
    }
}