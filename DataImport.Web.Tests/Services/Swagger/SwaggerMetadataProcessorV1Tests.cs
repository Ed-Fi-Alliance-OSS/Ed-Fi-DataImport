// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Services.Swagger;
using Shouldly;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DataImport.Web.Tests.Services.Swagger
{
    [TestFixture]
    public class SwaggerMetadataProcessorV1Tests
    {
        private const string ResourcesDocument = @"{
  ""apiVersion"": ""2.0"",
        ""swaggerVersion"": ""1.2"",
        ""basePath"": ""https://api.ed-fi.org:443/v2/api/metadata/resources/api-docs"",
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

        private const string DescriptorsDocument = @"{
  ""apiVersion"": ""2.0"",
        ""swaggerVersion"": ""1.2"",
        ""basePath"": ""https://api.ed-fi.org:443/v2/api/metadata/descriptors/api-docs"",
        ""apis"": [
        {
            ""path"": ""/termDescriptors"",
            ""description"": ""...""
        }]
}";

        [Test]
        public void ShouldHandleSwaggerVersion1Point2()
        {
            var sut = new SwaggerMetadataProcessorV1(null);

            var document = JObject.Parse(ResourcesDocument);
            sut.CanHandle(document).ShouldBeTrue();

            document = JObject.Parse(DescriptorsDocument);
            sut.CanHandle(document).ShouldBeTrue();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithDifferentMinorVersion()
        {
            var sut = new SwaggerMetadataProcessorV1(null);

            var document = JObject.Parse(ResourcesDocument);
            document["swaggerVersion"] = "1.3";
            sut.CanHandle(document).ShouldBeFalse();

            document = JObject.Parse(DescriptorsDocument);
            document["swaggerVersion"] = "1.3";
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithDifferentMajorVersion()
        {
            var sut = new SwaggerMetadataProcessorV1(null);

            var document = JObject.Parse(ResourcesDocument);
            document["swaggerVersion"] = "2.2";
            sut.CanHandle(document).ShouldBeFalse();

            document = JObject.Parse(DescriptorsDocument);
            document["swaggerVersion"] = "2.2";
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithMissingVersion()
        {
            var sut = new SwaggerMetadataProcessorV1(null);

            var document = JObject.Parse(ResourcesDocument);
            document.Remove("swaggerVersion");
            sut.CanHandle(document).ShouldBeFalse();

            document = JObject.Parse(DescriptorsDocument);
            document.Remove("swaggerVersion");
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public async Task ShouldFindResources()
        {
            var document = JObject.Parse(ResourcesDocument);
            var swaggerClientMock = new MockSwaggerWebClient();
            swaggerClientMock
                .Setup("https://api.ed-fi.org:443/v2/api/metadata/resources/api-docs/student", "studentDocument");

            swaggerClientMock
                .Setup("https://api.ed-fi.org:443/v2/api/metadata/resources/api-docs/studentAssessments", "studentAssessmentsDocument");

            var sut = new SwaggerMetadataProcessorV1(swaggerClientMock);

            var swaggerResources = await sut.GetMetadata(document, ApiSection.Resources);
            swaggerResources.ShouldMatch(
                    new SwaggerResource
                    {
                        SwaggerVersion = "1.2",
                        Metadata = "studentDocument",
                        Path = "/student",
                        ApiSection = ApiSection.Resources
                    },
                    new SwaggerResource
                    {
                        SwaggerVersion = "1.2",
                        Metadata = "studentAssessmentsDocument",
                        Path = "/studentAssessments",
                        ApiSection = ApiSection.Resources
                    });
        }

        [Test]
        public async Task ShouldFindDescriptors()
        {
            var document = JObject.Parse(DescriptorsDocument);
            var swaggerClientMock = new MockSwaggerWebClient();
            swaggerClientMock
                .Setup("https://api.ed-fi.org:443/v2/api/metadata/descriptors/api-docs/termDescriptors", "termDescriptorsDocument");

            var sut = new SwaggerMetadataProcessorV1(swaggerClientMock);

            var swaggerResources = await sut.GetMetadata(document, ApiSection.Descriptors);
            swaggerResources.ShouldMatch(
                    new SwaggerResource
                    {
                        SwaggerVersion = "1.2",
                        Metadata = "termDescriptorsDocument",
                        Path = "/termDescriptors",
                        ApiSection = ApiSection.Descriptors
                    });
        }
    }
}
