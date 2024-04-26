// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using DataImport.Web.Services.Swagger;
using NUnit.Framework;
using File = System.IO.File;

namespace DataImport.Web.Tests
{
    public class StubSwaggerWebClient : ISwaggerWebClient
    {
        private const string OdsDomainV25 = "http://test-ods-v2.5.0.1.example.com";
        private const string OdsDomainV311 = "http://test-ods-v3.1.1.example.com";
        private const string OdsDomainV711 = "http://test-ods-v7.1.1.example.com";

        public const string V711Context = "2022";

        public const string ApiServerUrlV25 = OdsDomainV25 + "/api/v2.0/2019";
        public const string ApiServerUrlV311 = OdsDomainV311 + "/data/v3";
        public const string ApiServerUrlV711 = OdsDomainV711 + "/data/v3";
        public const string ApiServerUrlV711WithOdsContext = $"{OdsDomainV711}/{V711Context}/data/v3";

        public async Task<string> DownloadString(string url)
        {
            const string SwaggerResourcesV25 = OdsDomainV25 + "/metadata/resources/api-docs";
            const string SwaggerDescriptorsV25 = OdsDomainV25 + "/metadata/descriptors/api-docs";

            const string SwaggerResourcesV311 = OdsDomainV311 + "/metadata/data/v3/resources/swagger.json";
            const string SwaggerDescriptorsV311 = OdsDomainV311 + "/metadata/data/v3/descriptors/swagger.json";

            const string SwaggerResourcesV711 = OdsDomainV711 + "/metadata/data/v3/resources/swagger.json";
            const string SwaggerDescriptorsV711 = OdsDomainV711 + "/metadata/data/v3/descriptors/swagger.json";

            const string SwaggerResourcesV711WithOdsContext = $"{OdsDomainV711}/metadata/{V711Context}/data/v3/resources/swagger.json";
            const string SwaggerDescriptorsV711WithOdsContext = $"{OdsDomainV711}/metadata/{V711Context}/data/v3/descriptors/swagger.json";

            if (url == SwaggerResourcesV25)
                return await SampleSwaggerResponseV25("Swagger-Resources-API-Docs.json");

            if (url == SwaggerDescriptorsV25)
                return await SampleSwaggerResponseV25("Swagger-Descriptors-API-Docs.json");

            if (url.StartsWith(SwaggerResourcesV25 + "/"))
            {
                var resource = url.Substring(SwaggerResourcesV25.Length + 1);

                return await SampleSwaggerResponseV25(resource + ".json");
            }

            if (url.StartsWith(SwaggerDescriptorsV25 + "/"))
            {
                var descriptor = url.Substring(SwaggerDescriptorsV25.Length + 1);

                return await SampleSwaggerResponseV25(descriptor + ".json");
            }

            if (url == SwaggerResourcesV311)
                return await SampleSwaggerResponseV311("Swagger-Resources-API-Docs.json");

            if (url == SwaggerDescriptorsV311)
                return await SampleSwaggerResponseV311("Swagger-Descriptors-API-Docs.json");

            if (url == OdsDomainV311)
                return await SampleSwaggerResponseV311("Swagger-Base-API-Response.json");

            if (url == SwaggerResourcesV711 || url == SwaggerResourcesV711WithOdsContext)
                return await SampleSwaggerResponseV711("Swagger-Resources-API-Docs.json");

            if (url == SwaggerDescriptorsV711 || url == SwaggerDescriptorsV711WithOdsContext)
                return await SampleSwaggerResponseV711("Swagger-Descriptors-API-Docs.json");

            if (url == OdsDomainV711)
                return await SampleSwaggerResponseV711("Swagger-Base-API-Response.json");

            throw new Exception(GetType().Name + " cannot simulate a request to url " + url);
        }

        private static Task<string> SampleSwaggerResponseV25(string filename)
            => Task.FromResult(File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "SampleMetadata-v2.5.0.1",
                    filename)));

        private static Task<string> SampleSwaggerResponseV311(string filename)
            => Task.FromResult(File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "SampleMetadata-v3.1.1",
                    filename)));
        private static Task<string> SampleSwaggerResponseV711(string filename)
            => Task.FromResult(File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "SampleMetadata-v7.1.1",
                    filename)));
    }
}
