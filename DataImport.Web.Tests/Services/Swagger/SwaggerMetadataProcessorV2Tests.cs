// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Services.Swagger;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataImport.Models;
using Shouldly;
using Newtonsoft.Json.Linq;
using static DataImport.TestHelpers.TestHelpers;

namespace DataImport.Web.Tests.Services.Swagger
{
    [TestFixture]
    public class SwaggerMetadataProcessorV2Tests
    {
        private string _testResourcesDocument;
        private JObject _testResourcesDocumentObject;

        private string _testDescriptorsDocument;
        private JObject _testDescriptorsDocumentObject;

        [OneTimeSetUp]
        public void SetupFixture()
        {
            var sampleMetadataFolder = $"SampleMetadata-v3.1.1{Path.DirectorySeparatorChar}";
            _testResourcesDocument = ReadTestFile($"{sampleMetadataFolder}Swagger-Resources-API-Docs.json");
            _testResourcesDocumentObject = JObject.Parse(_testResourcesDocument);

            _testDescriptorsDocument = ReadTestFile($"{sampleMetadataFolder}Swagger-Descriptors-API-Docs.json");
            _testDescriptorsDocumentObject = JObject.Parse(_testDescriptorsDocument);
        }

        [Test]
        public void ShouldHandleSwaggerVersion2()
        {
            var sut = new SwaggerMetadataProcessorV2();
            sut.CanHandle(_testResourcesDocumentObject).ShouldBeTrue();
            sut.CanHandle(_testDescriptorsDocumentObject).ShouldBeTrue();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithDifferentMinorVersion()
        {
            var sut = new SwaggerMetadataProcessorV2();

            var document = (JObject)_testResourcesDocumentObject.DeepClone();
            document["swagger"] = "2.1";
            sut.CanHandle(document).ShouldBeFalse();

            document = (JObject)_testDescriptorsDocumentObject.DeepClone();
            document["swagger"] = "2.1";
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithDifferentMajorVersion()
        {
            var sut = new SwaggerMetadataProcessorV2();

            var document = (JObject)_testResourcesDocumentObject.DeepClone();
            document["swagger"] = "1.0";
            sut.CanHandle(document).ShouldBeFalse();

            document = (JObject)_testDescriptorsDocumentObject.DeepClone();
            document["swagger"] = "1.0";
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public void ShouldNotHandleSwaggerDocumentWithMissingVersion()
        {
            var sut = new SwaggerMetadataProcessorV2();

            var document = (JObject)_testResourcesDocumentObject.DeepClone();
            document.Remove("swagger");
            sut.CanHandle(document).ShouldBeFalse();

            document = (JObject)_testDescriptorsDocumentObject.DeepClone();
            document.Remove("swagger");
            sut.CanHandle(document).ShouldBeFalse();
        }

        [Test]
        public async Task ShouldFindResourcesAndDescriptors()
        {
            var sut = new SwaggerMetadataProcessorV2();

            var result = await sut.GetMetadata(_testResourcesDocumentObject, ApiSection.Resources);
            result.Count.ShouldBe(95);
            result.All(x => x.ApiSection == ApiSection.Resources).ShouldBe(true);
            result.All(x => x.SwaggerVersion == "2.0").ShouldBe(true);
            result.All(x => x.Metadata != null).ShouldBe(true);
            result.All(x => x.Path.StartsWith("/")).ShouldBe(true);

            result = await sut.GetMetadata(_testDescriptorsDocumentObject, ApiSection.Descriptors);
            result.Count.ShouldBe(160);
            result.All(x => x.ApiSection == ApiSection.Descriptors).ShouldBe(true);
            result.All(x => x.SwaggerVersion == "2.0").ShouldBe(true);
            result.All(x => x.Metadata != null).ShouldBe(true);
            result.All(x => x.Path.StartsWith("/")).ShouldBe(true);
        }

        [Test]
        public async Task MetadataReferencesShouldBeStoredInModelsObject()
        {
            var sut = new SwaggerMetadataProcessorV2();
            var resources = await sut.GetMetadata(_testResourcesDocumentObject, ApiSection.Resources);

            var refRegex = new Regex("\"[$]ref\"[\\s]*:[\\s]*\"(.+)\"");

            foreach (var resource in resources)
            {
                var resourceDocument = JObject.Parse(resource.Metadata);
                var entityRefs = refRegex.Matches(resource.Metadata);

                foreach (Match entityRef in entityRefs)
                {
                    var entityName = SwaggerHelpers.GetSwagger20EntityNameFromReference(entityRef.Groups[1].Value);
                    resourceDocument["models"][entityName].ShouldNotBeNull();
                }
            }
        }
    }
}
