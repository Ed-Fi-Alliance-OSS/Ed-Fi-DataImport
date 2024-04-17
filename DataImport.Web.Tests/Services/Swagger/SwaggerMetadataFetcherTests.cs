// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Models;
using DataImport.Web.Services.Swagger;
using Shouldly;
using NUnit.Framework;
using static DataImport.Web.Tests.Testing;
using System.Threading.Tasks;

namespace DataImport.Web.Tests.Services.Swagger
{
    [TestFixture]
    public class SwaggerMetadataFetcherTests
    {
        // Test-specific Subclass (TSS) to facilitate testing of protected members
        public class SwaggerMetadataFetcherTss : SwaggerMetadataFetcher
        {
            public SwaggerMetadataFetcherTss(IEnumerable<ISwaggerMetadataProcessor> swaggerMetadataProcessors, ISwaggerWebClient swaggerWebClient) : base(swaggerMetadataProcessors, swaggerWebClient)
            {
            }

            public new async Task<string> GetSwaggerBaseDocumentUrl(string apiUrl, string apiVersion, string tenant, string context, ApiSection apiSection)
            {
                return await base.GetSwaggerBaseDocumentUrl(apiUrl, apiVersion, tenant, context, apiSection);
            }
        }

        [Test]
        public void ShouldThrowIfNoHandlersAvailable()
        {
            var testDocument = @"{
      ""apiVersion"": ""2.0"",
            ""swaggerVersion"": ""1.2"",
            ""basePath"": ""https://someapiurl/metadata/resources/api-docs"",
            ""apis"": [
            {
                ""path"": ""/student"",
                ""description"": ""This entity represents an individual for whom instruction, services, and/or care are provided in an early childhood, elementary, or secondary educational program under the jurisdiction of a school, education agency or other institution or program. A student is a person who has been enrolled in a school or other educational institution.""
            },
            {
                ""path"": ""/studentAssessments"",
                ""description"": ""This entity represents the analysis or scoring of a student's response on an assessment. The analysis results in a value that represents a student's performance on a set of items on a test.""
            }]
    }";

            var swaggerClientMock = new MockSwaggerWebClient();
            swaggerClientMock
                .Setup("https://someapiurl/metadata/resources/api-docs", testDocument);

            var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
            sut.GetMetadata("https://someapiurl/api/v2.0/2018", OdsApiV25, "", "")
                .ShouldThrow<NotSupportedException>()
                .Message.ShouldBe("No handler available to process Swagger document");
        }

        [TestFixture]
        public class GetSwaggerBaseDocumentUrl
        {
            [Test]
            public async Task Should_support_Suite2()
            {
                // Arrange
                var version260Url = "https://api-stage.ed-fi.org/v2.6.0/api/api/v2.0";
                var apiVersion = "2.6.0";
                var apiSection = ApiSection.Resources;

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, null);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(version260Url, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/v2.6.0/api/metadata/resources/api-docs");

            }

            [Test]
            public async Task Should_support_Suite3_Version311_in_Sandbox_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/v3.1.1/api/data/";
                var apiVersion = "3.1.1";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""3.1.1"",
  ""informationalVersion"": ""3.1.1"",
  ""build"": ""3.1.1.3888"",
  ""apiMode"": ""Sandbox"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.1.0"" },
    { ""name"": ""GrandBend"", ""version"": ""1.0.0"" }
  ]
}
";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/v3.1.1/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/v3.1.1/api/metadata/data/v3/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_Version510_in_Sandbox_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/v5.1.0/api/data/";
                var apiVersion = "5.1.0";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""5.1.0"",
  ""informationalVersion"": ""5.1.0"",
  ""suite"": ""3"",
  ""build"": ""5.1.0.12144"",
  ""apiMode"": ""Sandbox"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.2.0-c"" },
    { ""name"": ""Sample"", ""version"": ""1.0.0"" },
    { ""name"": ""TPDM"", ""version"": ""0.8.0"" }
  ],
  ""urls"": {
    ""openApiMetadata"": ""https://api-stage.ed-fi.org/v5.1.0/api/metadata/"",
    ""dependencies"": ""https://api-stage.ed-fi.org/v5.1.0/api/metadata/data/v3/dependencies"",
    ""oauth"": ""https://api-stage.ed-fi.org/v5.1.0/api/oauth/token"",
    ""dataManagementApi"": ""https://api-stage.ed-fi.org/v5.1.0/api/data/v3/""
  }
}";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/v5.1.0/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/v5.1.0/api/metadata/data/v3/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_in_Version311_SharedInstance_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/v3.1.1/api/data/";
                var apiVersion = "3.1.1";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""3.1.1"",
  ""informationalVersion"": ""3.1.1"",
  ""build"": ""3.1.1.3888"",
  ""apiMode"": ""SharedInstance"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.1.0"" },
    { ""name"": ""GrandBend"", ""version"": ""1.0.0"" }
  ]
}
";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/v3.1.1/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/v3.1.1/api/metadata/data/v3/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_Version510_in_SharedInstance_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/v5.1.0/api/data/";
                var apiVersion = "5.1.0";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""5.1.0"",
  ""informationalVersion"": ""5.1.0"",
  ""suite"": ""3"",
  ""build"": ""5.1.0.12144"",
  ""apiMode"": ""SharedInstance"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.2.0-c"" },
    { ""name"": ""Sample"", ""version"": ""1.0.0"" },
    { ""name"": ""TPDM"", ""version"": ""0.8.0"" }
  ],
  ""urls"": {
    ""openApiMetadata"": ""https://api-stage.ed-fi.org/v5.1.0/api/metadata/"",
    ""dependencies"": ""https://api-stage.ed-fi.org/v5.1.0/api/metadata/data/v3/dependencies"",
    ""oauth"": ""https://api-stage.ed-fi.org/v5.1.0/api/oauth/token"",
    ""dataManagementApi"": ""https://api-stage.ed-fi.org/v5.1.0/api/data/v3/""
  }
}";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/v5.1.0/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/v5.1.0/api/metadata/data/v3/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_Version311_in_YearSpecific_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/YearSpecific_v3.1.1/api/data/v3/2020";
                var apiVersion = "3.1.1";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""3.1.1"",
  ""informationalVersion"": ""3.1.1"",
  ""build"": ""3.1.1.3888"",
  ""apiMode"": ""Year Specific"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.1.0"" },
    { ""name"": ""GrandBend"", ""version"": ""1.0.0"" }
  ]
}
";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/YearSpecific_v3.1.1/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/YearSpecific_v3.1.1/api/metadata/data/v3/2020/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_Version510_in_YearSpecific_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/data/v3/2020";
                var apiVersion = "5.1.0";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""5.1.0"",
  ""informationalVersion"": ""5.1.0"",
  ""suite"": ""3"",
  ""build"": ""5.1.0.12129"",
  ""apiMode"": ""Year Specific"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.2.0-c"" },
    { ""name"": ""Homograph"", ""version"": ""1.0.0"" },
    { ""name"": ""Sample"", ""version"": ""1.0.0"" },
    { ""name"": ""TPDM"", ""version"": ""0.8.0"" }
  ],
  ""urls"": {
    ""openApiMetadata"": ""https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/metadata/2020"",
    ""dependencies"": ""https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/metadata/data/v3/2020/dependencies"",
    ""oauth"": ""https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/oauth/token"",
    ""dataManagementApi"": ""https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/data/v3/2020""
  }
}";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/YearSpecific_v5.1.0/api/metadata/data/v3/2020/resources/swagger.json");
            }

            [Test]
            public async Task Should_support_Suite3_Version52_in_InstanceYearSpecific_mode()
            {
                // Arrange
                var versionUrl = "https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/data/v3/INSTANCEABC123/2021";
                var apiVersion = "5.2";
                var apiSection = ApiSection.Resources;

                var testDocument = @"{
  ""version"": ""5.2"",
  ""informationalVersion"": ""5.2"",
  ""suite"": ""3"",
  ""build"": ""5.2.0.12345"",
  ""apiMode"": ""Instance Year Specific"",
  ""dataModels"": [
    { ""name"": ""Ed-Fi"", ""version"": ""3.3.0-a"" }
  ],
  ""urls"": {
    ""dependencies"": ""https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/metadata/data/v3/{instance}/2021/dependencies"",
    ""openApiMetadata"": ""https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/metadata/{instance}/2021"",
    ""oauth"": ""https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/{instance}/oauth/token"",
    ""dataManagementApi"": ""https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/data/v3/{instance}/2021"",
    ""xsdMetadata"": ""https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/metadata/{instance}/2021/xsd""
  }
}";

                var swaggerClientMock = new MockSwaggerWebClient();
                swaggerClientMock
                    .Setup("https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api", testDocument);

                // Act
                var sut = new SwaggerMetadataFetcherTss(null, swaggerClientMock);
                var baseDocumentUrl = await sut.GetSwaggerBaseDocumentUrl(versionUrl, apiVersion, string.Empty, string.Empty, apiSection);

                // Assert
                baseDocumentUrl.ShouldBe(
                    "https://api-stage.ed-fi.org/InstanceYearSpecific_v5.2/api/metadata/data/v3/INSTANCEABC123/2021/resources/swagger.json");
            }
        }

        [Test]
        public async Task ShouldHandleV1Point2Document()
        {
            var testResourcesDocument = @"{
      ""apiVersion"": ""2.0"",
            ""swaggerVersion"": ""1.2"",
            ""basePath"": ""https://someapiurl/metadata/resources/api-docs"",
            ""apis"": [
            {
                ""path"": ""/student"",
                ""description"": ""This entity represents an individual for whom instruction, services, and/or care are provided in an early childhood, elementary, or secondary educational program under the jurisdiction of a school, education agency or other institution or program. A student is a person who has been enrolled in a school or other educational institution.""
            },
            {
                ""path"": ""/studentAssessments"",
                ""description"": ""This entity represents the analysis or scoring of a student's response on an assessment. The analysis results in a value that represents a student's performance on a set of items on a test.""
            }]
    }";

            var testDescriptorsDocument = @"{
      ""apiVersion"": ""2.0"",
            ""swaggerVersion"": ""1.2"",
            ""basePath"": ""https://someapiurl/metadata/descriptors/api-docs"",
            ""apis"": [
            {
                ""path"": ""/termDescriptors"",
                ""description"": ""...""
            }]
    }";

            var swaggerClientMock = new MockSwaggerWebClient();
            swaggerClientMock
                .Setup("https://someapiurl/metadata/resources/api-docs", testResourcesDocument);
            swaggerClientMock
                .Setup("https://someapiurl/metadata/descriptors/api-docs", testDescriptorsDocument);

            swaggerClientMock
                .Setup("https://someapiurl/metadata/resources/api-docs/student", "studentDocument");
            swaggerClientMock
                .Setup("https://someapiurl/metadata/resources/api-docs/studentAssessments", "studentAssessmentsDocument");
            swaggerClientMock
                .Setup("https://someapiurl/metadata/descriptors/api-docs/termDescriptors", "termDescriptorsDocument");

            var processors = GetMetadataProcessors(swaggerClientMock);
            var sut = new SwaggerMetadataFetcherTss(processors, swaggerClientMock);

            var resources = (await sut.GetMetadata("https://someapiurl/api/v2.0/2018", OdsApiV25, string.Empty, string.Empty)).ToList();
            resources.Count.ShouldBe(3);
            resources.Any(e => e.Path == "/student").ShouldBeTrue();
            resources.Any(e => e.Path == "/studentAssessments").ShouldBeTrue();
            resources.Any(e => e.Path == "/termDescriptors").ShouldBeTrue();
        }

        [Test]
        public async Task ShouldHandleUrlWithExtraApiInPath()
        {
            var testResourcesDocument = @"{
      ""apiVersion"": ""2.0"",
            ""swaggerVersion"": ""1.2"",
            ""basePath"": ""https://someapiurl/v2/api/metadata/resources/api-docs"",
            ""apis"": [
            {
                ""path"": ""/student"",
                ""description"": ""This entity represents an individual for whom instruction, services, and/or care are provided in an early childhood, elementary, or secondary educational program under the jurisdiction of a school, education agency or other institution or program. A student is a person who has been enrolled in a school or other educational institution.""
            },
            {
                ""path"": ""/studentAssessments"",
                ""description"": ""This entity represents the analysis or scoring of a student's response on an assessment. The analysis results in a value that represents a student's performance on a set of items on a test.""
            }]
    }";

            var testDescriptorsDocument = @"{
      ""apiVersion"": ""2.0"",
            ""swaggerVersion"": ""1.2"",
            ""basePath"": ""https://someapiurl/v2/api/metadata/descriptors/api-docs"",
            ""apis"": [
            {
                ""path"": ""/termDescriptors"",
                ""description"": ""...""
            }]
    }";

            var swaggerClientMock = new MockSwaggerWebClient();
            swaggerClientMock
                .Setup("https://someapiurl/v2/api/metadata/resources/api-docs", testResourcesDocument);
            swaggerClientMock
                .Setup("https://someapiurl/v2/api/metadata/descriptors/api-docs", testDescriptorsDocument);

            swaggerClientMock
                .Setup("https://someapiurl/v2/api/metadata/resources/api-docs/student", "studentDocument");
            swaggerClientMock
                .Setup("https://someapiurl/v2/api/metadata/resources/api-docs/studentAssessments", "studentAssessmentsDocument");
            swaggerClientMock
                .Setup("https://someapiurl/v2/api/metadata/descriptors/api-docs/termDescriptors", "termDescriptorsDocument");


            var processors = GetMetadataProcessors(swaggerClientMock);
            var sut = new SwaggerMetadataFetcherTss(processors, swaggerClientMock);

            var resources = (await sut.GetMetadata("https://someapiurl/v2/api/api/v2.0/2018", OdsApiV25, string.Empty, string.Empty)).ToList();
            resources.Count.ShouldBe(3);
        }

        private static IEnumerable<ISwaggerMetadataProcessor> GetMetadataProcessors(ISwaggerWebClient swaggerWebClient)
        {
            yield return new SwaggerMetadataProcessorV1(swaggerWebClient);
        }
    }
}
