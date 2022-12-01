// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.TestHelpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Models.Tests
{
    public class MetadataValidationExtensionsTests
    {
        private readonly Resource _resource;

        private const string Empty = "{}";

        private const string Partial = @"{
                    ""propertyA"": ""A"",
                    ""complexProperty"": {
                        ""nestedPropertyF"": ""F""
                    }
                }";

        private const string Statics = @"{
                    ""propertyA"": ""A"",
                    ""propertyB"": 2,
                    ""propertyC"": ""C"",
                    ""complexProperty"": {
                        ""nestedPropertyD"": ""D"",
                        ""nestedPropertyE"": ""E"",
                        ""nestedPropertyF"": ""F""
                    },
                    ""arrayProperty"": [
                        { ""key"": ""G"" },
                        { ""key"": ""H"" }
                    ]
                }";

        private const string Columns = @"{
                    ""propertyA"": { ""Column"": ""ColumnA"" },
                    ""propertyB"": { ""Column"": ""ColumnB"" },
                    ""propertyC"": { ""Column"": ""ColumnC"", ""Default"": ""Default C"" },
                    ""complexProperty"": {
                        ""nestedPropertyD"": { ""Column"": ""ColumnD"" },
                        ""nestedPropertyE"": { ""Column"": ""ColumnE"" },
                        ""nestedPropertyF"": { ""Column"": ""ColumnF"" }
                    },
                    ""arrayProperty"": [
                        { ""key"": { ""Column"": ""ColumnG"" } },
                        { ""key"": { ""Column"": ""ColumnH"" } }
                    ]
                }";

        private const string ArbitraryKeyOrder = @"{
                    ""arrayProperty"": [
                        { ""key"": ""G"" },
                        { ""key"": ""H"" }
                    ],
                    ""propertyB"": 2,
                    ""complexProperty"": {
                        ""nestedPropertyE"": ""E"",
                        ""nestedPropertyD"": ""D"",
                        ""nestedPropertyF"": ""F""
                    },
                    ""propertyA"": ""A"",
                    ""propertyC"": ""C""
                }";

        private const string UnexpectedKey = @"{
                    ""propertyA"": ""A"",
                    ""complexProperty"": {
                        ""nestedPropertyF"": ""F""
                    },
                    ""unexpectedProperty"": ""!?"",
                }";

        private const string UnexpectedStringLiteral = @"{
                    ""propertyB"": ""2"",
                }";

        private const string ExpectedObject = @"{
                    ""propertyA"": ""A"",
                    ""complexProperty"": ""!?""
                }";

        private const string ExpectedArray = @"{
                    ""propertyA"": ""A"",
                    ""complexProperty"": {
                        ""nestedPropertyF"": ""F""
                    },
                    ""arrayProperty"": ""!?""
                }";

        public MetadataValidationExtensionsTests()
        {
            var resourceMetadata = new[]
            {
                ResourceMetadataBuilder.Property("propertyA", "string"),
                ResourceMetadataBuilder.Property("propertyB", "integer"),
                ResourceMetadataBuilder.Property("propertyC", "string"),

                ResourceMetadataBuilder.Object("complexProperty", "complexPropertyType",
                    ResourceMetadataBuilder.Property("nestedPropertyD", "string"),
                    ResourceMetadataBuilder.Property("nestedPropertyE", "string"),
                    ResourceMetadataBuilder.Property("nestedPropertyF", "string")),

                ResourceMetadataBuilder.Array(
                    "arrayProperty",
                    ResourceMetadataBuilder.Object("arrayItem","arrayItemType",
                        ResourceMetadataBuilder.Property("key", "string")))
            };

            _resource = new Resource
            {
                Path = "/testResource",
                Metadata = ResourceMetadata.Serialize(resourceMetadata),
                ApiSection = ApiSection.Resources
            };
        }

        [Test]
        public void ShouldRecognizeValidJsonObjectsAsCompatibleWithTheirIntendedResource()
        {
            IsCompatible(Empty, MetadataCompatibilityLevel.DataMap);
            IsCompatible(Partial, MetadataCompatibilityLevel.DataMap);
            IsCompatible(Statics, MetadataCompatibilityLevel.DataMap);
            IsCompatible(Columns, MetadataCompatibilityLevel.DataMap);
            IsCompatible(ArbitraryKeyOrder, MetadataCompatibilityLevel.DataMap);

            IsCompatible(Empty, MetadataCompatibilityLevel.Bootstrap);
            IsCompatible(Partial, MetadataCompatibilityLevel.Bootstrap);
            IsCompatible(Statics, MetadataCompatibilityLevel.Bootstrap);
            IsNotCompatible(Columns, MetadataCompatibilityLevel.Bootstrap, "Bootstrap JSON cannot include column references.");
            IsCompatible(ArbitraryKeyOrder, MetadataCompatibilityLevel.Bootstrap);

            //Single value type mismatches are permitted by DataMapSerializer, so unexpectedStringLiteral
            //is still treated as compatible as far as Data Import is concerned. The type of individual values
            //is only a concern at POST time. This way, users can edit a Data Map's static and default values
            //at will until they are satisfied with their work. Besides, no amount of up front type checking
            //for single values would be sufficient, because an arbitrary CSV cell could still be the wrong
            //type, and we can't check for *that* until TransformLoad time.
            IsCompatible(UnexpectedStringLiteral, MetadataCompatibilityLevel.DataMap);
            IsCompatible(UnexpectedStringLiteral, MetadataCompatibilityLevel.Bootstrap);
        }

        [Test]
        public void ShouldRecognizeInvalidJsonObjectsAsIncompatibleWithTheirIntendedResource()
        {
            foreach (MetadataCompatibilityLevel level in Enum.GetValues(typeof(MetadataCompatibilityLevel)))
            {
                IsNotCompatible(UnexpectedKey, level,
                    "Cannot deserialize mappings from JSON, because the key 'unexpectedProperty' " +
                    "should not exist according to the metadata for resource '/testResource'.");

                IsNotCompatible(ExpectedObject, level,
                    "Cannot deserialize mappings from JSON, because an object literal was expected. Instead, found: \"!?\"");

                IsNotCompatible(ExpectedArray, level,
                    "Cannot deserialize mappings from JSON, because an array literal was expected. Instead, found: \"!?\"");
            }
        }

        [Test]
        public void ShouldRecognizeTopLevelArraysAsIncompatibleForAnyDataMap()
        {
            const string UnexpectedTopLevelArray =
                "A single data map object for a single '/testResource' resource" +
                " was expected, but an array of objects was provided.";

            IsNotCompatible(JsonArray(Empty), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(Partial), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(Statics), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(Columns), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(ArbitraryKeyOrder), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(UnexpectedKey), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(UnexpectedStringLiteral), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(ExpectedObject), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
            IsNotCompatible(JsonArray(ExpectedArray), MetadataCompatibilityLevel.DataMap, UnexpectedTopLevelArray);
        }

        [Test]
        public void ShouldRecognizeTopLevelArraysAsCompatibleForBootstrapsIfAndOnlyIfTheItemsAreAllIndividuallyCompatible()
        {
            IsCompatible(JsonArray(Empty, Partial, Statics, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap);

            IsNotCompatible(JsonArray(Empty, Partial, Statics, Columns, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap,
                "Bootstrap JSON cannot include column references.");

            IsNotCompatible(JsonArray(Empty, Partial, Statics, UnexpectedKey, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap,
                "Cannot deserialize mappings from JSON, because the key 'unexpectedProperty' " +
                "should not exist according to the metadata for resource '/testResource'.");

            //Single value type mismatches are permitted by DataMapSerializer, so the presence of
            //unexpectedStringLiteral in this array is still treated as compatible as far as Data Import
            //is concerned. As with Data Maps, the type of individual values is only a concern at POST time.
            IsCompatible(JsonArray(Empty, Partial, Statics, UnexpectedStringLiteral, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap);

            IsNotCompatible(JsonArray(Empty, Partial, Statics, ExpectedObject, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap,
                "Cannot deserialize mappings from JSON, because an object literal was expected. Instead, found: \"!?\"");

            IsNotCompatible(JsonArray(Empty, Partial, Statics, ExpectedArray, ArbitraryKeyOrder), MetadataCompatibilityLevel.Bootstrap,
                "Cannot deserialize mappings from JSON, because an array literal was expected. Instead, found: \"!?\"");
        }

        private string JsonArray(params string[] elementLiterals)
        {
            return $"[{string.Join(", ", elementLiterals)}]";
        }

        private void IsCompatible(string json, MetadataCompatibilityLevel level)
        {
            string errorMessage;
            JToken.Parse(json)
                .IsCompatibleWithResource(_resource, level, out errorMessage)
                .ShouldBe(true);
            errorMessage.ShouldBe(null);
        }

        private void IsNotCompatible(string json, MetadataCompatibilityLevel level, string expectedErrorMessage)
        {
            string errorMessage;
            JToken.Parse(json)
                .IsCompatibleWithResource(_resource, level, out errorMessage)
                .ShouldBe(false);
            errorMessage.ShouldBe(expectedErrorMessage);
        }
    }
}
