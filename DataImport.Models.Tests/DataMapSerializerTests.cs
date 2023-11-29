// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using NUnit.Framework;
using DataImport.TestHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using static DataImport.TestHelpers.ResourceMetadataBuilder;
using static DataImport.TestHelpers.DataMapperBuilder;

namespace DataImport.Models.Tests
{
    public class DataMapSerializerTests
    {
        private readonly ResourceMetadata[] _resourceMetadata;

        public DataMapSerializerTests()
        {
            _resourceMetadata = new[]
            {
                Property("propertyA", "string"),

                Property("propertyB", "string"),
                Property("propertyC", "integer"),

                Property("propertyD", "number"),
                Property("propertyE", "date-time"),

                Property("unmappedProperty", "string"),

                Object("complexProperty", "complexPropertyType",
                    Property("nestedPropertyF", "string"),
                    Property("nestedPropertyG", "string"),
                    Property("nestedUnmappedProperty", "string")),

                Object("unmappedComplexProperty", "unmappedComplexPropertyType",
                    Property("nestedPropertyH", "string"),
                    Property("nestedPropertyI", "string")),

                Array(
                    "arrayProperty",
                    Object(
                        "arrayItem",
                        "arrayItemType",
                        Property("key", "string"))),

                Array(
                    "unmappedArrayProperty",
                    Object(
                        "arrayItem",
                        "arrayItemType",
                        Property("key", "string")))
            };
        }

        [Test]
        public void ShouldSerializeAndDeserializeJsonMapRepresentationOfMappings()
        {
            //As a set of mappings is serialized to a JSON Map and deserialized back,
            //ensure that no data is lost in the round trip. Note how unmapped items
            //in the user-facing set of mappings are omitted from the JSON representation,
            //yet reinstated (where necessary) during deserialization, so that the user
            //has the option of filling them out.

            var mappings = new[]
            {
                MapStatic("propertyA", "Static A"),

                MapColumn("propertyB", "ColumnB"),
                MapColumn("propertyC", "ColumnC", "Default C"),

                MapLookup("propertyD", "ColumnD", "test-lookup"),
                MapLookup("propertyE", "ColumnE", "test-lookup", "Default E"),

                Unmapped("unmappedProperty"),

                MapObject("complexProperty",
                    MapColumn("nestedPropertyF", "ColumnF"),
                    MapColumn("nestedPropertyG", "ColumnG"),
                    Unmapped("nestedUnmappedProperty")),

                MapObject("unmappedComplexProperty",
                    Unmapped("nestedPropertyH"),
                    Unmapped("nestedPropertyI")),

                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("key", "ColumnH")),
                    MapObject("arrayItem", MapColumn("key", "ColumnI"))),

                MapArray("unmappedArrayProperty")
            };

            var jsonMap = @"{
                    ""propertyA"": ""Static A"",
                    ""propertyB"": {
                        ""Column"": ""ColumnB""
                    },
                    ""propertyC"": {
                        ""Column"": ""ColumnC"",
                        ""Default"": ""Default C""
                    },
                    ""propertyD"": {
                        ""Column"": ""ColumnD"",
                        ""Lookup"": ""test-lookup""
                    },
                    ""propertyE"": {
                        ""Column"": ""ColumnE"",
                        ""Lookup"": ""test-lookup"",
                        ""Default"": ""Default E""
                    },
                    ""complexProperty"": {
                        ""nestedPropertyF"": {
                            ""Column"": ""ColumnF""
                        },
                        ""nestedPropertyG"": {
                            ""Column"": ""ColumnG""
                        }
                    },
                    ""arrayProperty"": [
                        {
                            ""key"": {
                                ""Column"": ""ColumnH""
                            }
                        },
                        {
                            ""key"": {
                                ""Column"": ""ColumnI""
                            }
                        }
                    ]
                }";

            Serialize(_resourceMetadata, mappings).ShouldMatch(jsonMap);
            Deserialize(_resourceMetadata, jsonMap).ShouldMatch(mappings);
        }

        [Test]
        public void ShouldSerializeToMinimalJsonMapWhenMappingsAreSubsetOfMetadata()
        {
            //Metadata describes many fields, some of which may not even be required by the ODS.
            //If a set of mappings to serialize is a subset of the full potential described by
            //the metadata, we should be able to serialize the given mappings anyway. The order
            //each key appears in the mappings to serialize is not meaningful, but is preserved
            //as the most natural behavior.

            var emptyMappings = new DataMapper[] { };
            Serialize(_resourceMetadata, emptyMappings).ShouldMatch("{}");

            var partialMappings = new[]
            {
                MapColumn("propertyC", "ColumnC", "Default C"),
                MapColumn("propertyB", "ColumnB")
            };
            var expectedJsonMap = @"{
                      ""propertyC"": {
                                ""Column"": ""ColumnC"",
                                ""Default"": ""Default C""
                            },
                      ""propertyB"": {
                                ""Column"": ""ColumnB""
                            }
                  }";
            Serialize(_resourceMetadata, partialMappings).ShouldMatch(expectedJsonMap);
        }

        [Test]
        public void ShouldSerializeMappingsOmittingUnmappedArrayElements()
        {
            //If an array element has no actual contained property mappings, it should
            //be omitted from serialization. Naturally, the removed item should no
            //longer be present upon deserialization.

            var resourceMetadata = new[]
            {
                Array(
                    "arrayProperty",
                    Object(
                        "arrayItem",
                        "arrayItemType",
                        Property("key", "string")))
            };

            var mappingsIncludingUnmappedItem = new[]
            {
                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("key", "ColumnH")),
                    MapObject("arrayItem", Unmapped("key")),
                    MapObject("arrayItem", MapColumn("key", "ColumnI")))
            };

            var mappingsLackingUnmappedItem = new[]
            {
                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("key", "ColumnH")),
                    MapObject("arrayItem", MapColumn("key", "ColumnI")))
            };

            var jsonLackingUnmappedItem = @"{
                    ""arrayProperty"": [
                        {
                            ""key"": {
                                ""Column"": ""ColumnH""
                            }
                        },
                        {
                            ""key"": {
                                ""Column"": ""ColumnI""
                            }
                        }
                    ]
                }";

            Serialize(resourceMetadata, mappingsIncludingUnmappedItem).ShouldMatch(jsonLackingUnmappedItem);
            Deserialize(resourceMetadata, jsonLackingUnmappedItem).ShouldMatch(mappingsLackingUnmappedItem);
        }

        [Test]
        public void ShouldFailToSerializeMappingsWithKeysNotFoundInMetadata()
        {
            //If a mapping to be serialized has { object } keys that are not defined by the metadata,
            //then we know the mapping is invalid and should refuse to proceed with serialization.
            //In addition to merely being as suspicious request, the lack of metadata means we do
            //not have enough information to honor the request at all: is the unexpected node another
            //{ object }, and [ array ], a { singular value's column mapping }, a system defect, human
            //error? The request is ambiguous.

            var invalidMappings = new[]
            {
                MapColumn("propertyC", "ColumnC", "Default C"),
                MapColumn("propertyB", "ColumnB"),
                MapColumn("unexpectedProperty", "ColumnZ")
            };

            Action attemptToSerializeInvalidMapping = () => Serialize(_resourceMetadata, invalidMappings);

            attemptToSerializeInvalidMapping
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because the key 'unexpectedProperty' " +
                          "should not exist according to the metadata for resource '/testResource'.");
        }

        [Test]
        public void ShouldFailToSerializeMappingsWhichMapSingleValueWhenExpectingAnObjectLiteral()
        {
            Action attemptToSerializeStaticMappingToExpectedObject = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapStatic("complexProperty", "Static Value"),
                });
            };

            attemptToSerializeStaticMappingToExpectedObject
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because an object " +
                          "literal was expected for key 'complexProperty', but " +
                          "instead it is being mapped to a single value.");

            Action attemptToSerializeColumnMappingToExpectedObject = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapColumn("complexProperty", "ColumnA"),
                });
            };

            attemptToSerializeColumnMappingToExpectedObject
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because an object " +
                          "literal was expected for key 'complexProperty', but " +
                          "instead it is being mapped to a single value.");
        }

        [Test]
        public void ShouldFailToSerializeMappingsWhichMapSingleValueWhenExpectingAnArrayLiteral()
        {
            Action attemptToSerializeStaticMappingToExpectedArray = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapStatic("arrayProperty", "Static Value"),
                });
            };

            attemptToSerializeStaticMappingToExpectedArray
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because an array " +
                          "literal was expected for key 'arrayProperty', but " +
                          "instead it is being mapped to a single value.");

            Action attemptToSerializeColumnMappingToExpectedArray = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapColumn("arrayProperty", "ColumnA"),
                });
            };

            attemptToSerializeColumnMappingToExpectedArray
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because an array " +
                          "literal was expected for key 'arrayProperty', but " +
                          "instead it is being mapped to a single value.");
        }

        [Test]
        public void ShouldFailToSerializeMappingsWhichMapComplexValuesWhenExpectingSingleValues()
        {
            Action attemptToSerializeObjectMappingToExpectedSingleValue = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapObject("propertyA",
                        MapColumn("nestedPropertyF", "ColumnF"),
                        MapColumn("nestedPropertyG", "ColumnG"),
                        Unmapped("nestedUnmappedProperty"))
                });
            };

            attemptToSerializeObjectMappingToExpectedSingleValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because a single " +
                          "value was expected for key 'propertyA', but " +
                          "instead it is being mapped to a complex value.");

            Action attemptToSerializeArrayMappingToExpectedSingleValue = () =>
            {
                Serialize(_resourceMetadata, new[]
                {
                    MapArray(
                        "propertyA",
                        MapObject("arrayItem", MapColumn("key", "ColumnH")))
                });
            };

            attemptToSerializeArrayMappingToExpectedSingleValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot serialize mappings to JSON, because a single " +
                          "value was expected for key 'propertyA', but " +
                          "instead it is being mapped to a complex value.");
        }

        [Test]
        public void ShouldDeserializeFromPreviouslyParsedJObject()
        {
            var mappings = new[]
            {
                MapStatic("propertyA", "Static A"),

                MapColumn("propertyB", "ColumnB"),
                MapColumn("propertyC", "ColumnC", "Default C"),

                MapLookup("propertyD", "ColumnD", "test-lookup"),
                MapLookup("propertyE", "ColumnE", "test-lookup", "Default E"),

                Unmapped("unmappedProperty"),

                MapObject("complexProperty",
                    MapColumn("nestedPropertyF", "ColumnF"),
                    MapColumn("nestedPropertyG", "ColumnG"),
                    Unmapped("nestedUnmappedProperty")),

                MapObject("unmappedComplexProperty",
                    Unmapped("nestedPropertyH"),
                    Unmapped("nestedPropertyI")),

                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("key", "ColumnH")),
                    MapObject("arrayItem", MapColumn("key", "ColumnI"))),

                MapArray("unmappedArrayProperty")
            };

            var jsonMap = @"{
                    ""propertyA"": ""Static A"",
                    ""propertyB"": {
                        ""Column"": ""ColumnB""
                    },
                    ""propertyC"": {
                        ""Column"": ""ColumnC"",
                        ""Default"": ""Default C""
                    },
                    ""propertyD"": {
                        ""Column"": ""ColumnD"",
                        ""Lookup"": ""test-lookup""
                    },
                    ""propertyE"": {
                        ""Column"": ""ColumnE"",
                        ""Lookup"": ""test-lookup"",
                        ""Default"": ""Default E""
                    },
                    ""complexProperty"": {
                        ""nestedPropertyF"": {
                            ""Column"": ""ColumnF""
                        },
                        ""nestedPropertyG"": {
                            ""Column"": ""ColumnG""
                        }
                    },
                    ""arrayProperty"": [
                        {
                            ""key"": {
                                ""Column"": ""ColumnH""
                            }
                        },
                        {
                            ""key"": {
                                ""Column"": ""ColumnI""
                            }
                        }
                    ]
                }";

            Deserialize(_resourceMetadata, JObject.Parse(jsonMap)).ShouldMatch(mappings);
        }

        [Test]
        public void ShouldDeserializeIncludingUnmappedPropertiesWhenJsonMapIsSubsetOfMetadata()
        {
            //Metadata describes many fields, some of which may not even be required by the ODS.
            //If a JSON Map to deserialize is a subset of the full potential described by
            //the metadata, we should be able to deserialize the given JSON Map anyway. For each
            //missing property, we get an explicit DataMapper representing the presence of an
            //unmapped field, giving the user the option to fill it out. The order each key appears
            //in the JSON Map to deserialize is not meaningful, so the output is normalized to
            //metadata order as a result of ensuring completeness of the result

            Deserialize(_resourceMetadata, "{}")
                .ShouldMatch(
                    Unmapped("propertyA"),
                    Unmapped("propertyB"),
                    Unmapped("propertyC"),
                    Unmapped("propertyD"),
                    Unmapped("propertyE"),
                    Unmapped("unmappedProperty"),

                    MapObject("complexProperty",
                        Unmapped("nestedPropertyF"),
                        Unmapped("nestedPropertyG"),
                        Unmapped("nestedUnmappedProperty")),

                    MapObject("unmappedComplexProperty",
                        Unmapped("nestedPropertyH"),
                        Unmapped("nestedPropertyI")),

                    MapArray("arrayProperty"),

                    MapArray("unmappedArrayProperty")
                );

            var partialJsonMap = @"{
                      ""propertyC"": {
                                ""Column"": ""ColumnC"",
                                ""Default"": ""Default C""
                            },
                      ""propertyB"": {
                                ""Column"": ""ColumnB""
                            }
                  }";
            Deserialize(_resourceMetadata, partialJsonMap)
                .ShouldMatch(
                    Unmapped("propertyA"),
                    MapColumn("propertyB", "ColumnB"),
                    MapColumn("propertyC", "ColumnC", "Default C"),
                    Unmapped("propertyD"),
                    Unmapped("propertyE"),
                    Unmapped("unmappedProperty"),

                    MapObject("complexProperty",
                        Unmapped("nestedPropertyF"),
                        Unmapped("nestedPropertyG"),
                        Unmapped("nestedUnmappedProperty")),

                    MapObject("unmappedComplexProperty",
                        Unmapped("nestedPropertyH"),
                        Unmapped("nestedPropertyI")),

                    MapArray("arrayProperty"),

                    MapArray("unmappedArrayProperty")
                );
        }

        [Test]
        public void ShouldFailToDeserializeTextThatIsNotJson()
        {
            //To assist with troubleshooting, when deserialization fails
            //because the plain text isn't even valid JSON to begin with,
            //report the problem with a clear message.

            var invalidJsonMap = @"This plain text is not JSON.";

            Action attemptToDeserializeInvalidJsonMap = () => Deserialize(_resourceMetadata, invalidJsonMap);

            var argumentException = attemptToDeserializeInvalidJsonMap
                .ShouldThrow<ArgumentException>();

            argumentException
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the map text is not a valid JSON object. " +
                    "Check the inner exception for details. Invalid JSON Map text:" +
                    $"{Environment.NewLine}{Environment.NewLine}This plain text is not JSON.");

            var innerException = argumentException.InnerException;
            innerException.ShouldBeOfType<JsonReaderException>();
            innerException.Message.ShouldBe(
                "Unexpected character encountered while parsing value: T. Path '', line 0, position 0.");
        }

        [Test]
        public void ShouldFailToDeserializeJsonMapWithKeysNotFoundInMetadata()
        {
            //If a JSON Map to be deserialized has { object } keys that are not defined by the metadata,
            //then we know the mapping is invalid and should refuse to proceed with deserialization.
            //In addition to merely being as suspicious request, the lack of metadata means we do
            //not have enough information to honor the request at all: is the unexpected node another
            //{ object }, and [ array ], a { singular value's column mapping }, a system defect, human
            //error? The request is ambiguous.

            var invalidJsonMap = @"
            {
                ""propertyC"": { ""Column"": ""ColumnC"", ""Default"": ""Default C"" },
                ""propertyB"": { ""Column"": ""ColumnB"" },
                ""unexpectedProperty"": { ""Column"": ""ColumnZ"" }
            }";

            Action attemptToDeserializeInvalidJsonMap = () => Deserialize(_resourceMetadata, invalidJsonMap);

            attemptToDeserializeInvalidJsonMap
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe("Cannot deserialize mappings from JSON, because the key 'unexpectedProperty' " +
                          "should not exist according to the metadata for resource '/testResource'.");
        }

        [Test]
        public void ShouldFailToDeserializeJsonMapWithUnexpectedFormat()
        {
            //The outermost JSON should be an { object }, not an [ array ].
            Action attemptToDeserializeInvalidTopLevelObject =
                () => Deserialize(_resourceMetadata, @"[]");
            attemptToDeserializeInvalidTopLevelObject
                .ShouldThrow<ArgumentException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the map text is not a valid JSON object. " +
                    "Check the inner exception for details. Invalid JSON Map text:" +
                    $"{Environment.NewLine}{Environment.NewLine}[]");

            //Metadata declares that "complexProperty" should be an { object }, not an [ array ].
            Action attemptToDeserializeInvalidObjectValue =
                () => Deserialize(_resourceMetadata, @"{ ""complexProperty"": [] }");
            attemptToDeserializeInvalidObjectValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because an object literal was expected. " +
                    "Instead, found: []");

            //Metadata declares that "arrayProperty" should be an [ array ], not a boolean.
            Action attemptToDeserializeInvalidArrayValue =
                () => Deserialize(_resourceMetadata, @"{ ""arrayProperty"": true }");
            attemptToDeserializeInvalidArrayValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because an array literal was expected. " +
                    "Instead, found: true");

            //Metadata declares that "arrayProperty" whose items should be { objects }, not booleans.
            Action attemptToDeserializeInvalidArrayItemValue =
                () => Deserialize(_resourceMetadata, @"{ ""arrayProperty"": [ true, false ] }");
            attemptToDeserializeInvalidArrayItemValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because an object literal was expected. " +
                    "Instead, found: true");

            //Metadata declares that "propertyA" should be Column Source object like a column mapping or static value, not an array.
            Action attemptToDeserializeInvalidColumnMapValue =
                () => Deserialize(_resourceMetadata, @"{ ""propertyA"": [] }");
            attemptToDeserializeInvalidColumnMapValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'propertyA' was expected " +
                    "to have a string value. Instead, the value was: []");

            //When a Column Source object is expected, it should have a "Column" property.
            Action attemptToDeserializeMissingSourceColumn =
                () => Deserialize(_resourceMetadata, @"{ ""propertyA"": {} }");
            attemptToDeserializeMissingSourceColumn
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'propertyA' was " +
                    "expected to have a Column Source declaration as its value, indicating the source column. " +
                    "Instead, the value was: {}");

            //When a Column Source object is expected, it can have optional "Default" and "Lookup" properties
            //in addition to "Column", but no other unexpected properties.
            Action attemptToDeserializeUnexpectedColumnSourceProperties =
                () => Deserialize(_resourceMetadata, @"
                    {
                        ""propertyA"": {
                            ""Column"": ""Col1"",
                            ""Lookup"": ""look-up"",
                            ""Default"": ""default"",
                            ""Unexpected"": ""Col1""
                        }
                    }");
            attemptToDeserializeUnexpectedColumnSourceProperties
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'propertyA' was " +
                    "expected to have a Column Source declaration as its value. Instead, the value " +
                    $"contains unexpected property 'Unexpected': {{{Environment.NewLine}  \"Column\": \"Col1\",{Environment.NewLine}  \"Lookup\": \"look-up\",{Environment.NewLine}  \"Default\": \"default\",{Environment.NewLine}  \"Unexpected\": \"Col1\"{Environment.NewLine}}}");

            //When a Column Source object is expected, and has expected keys, Columns and Lookup must be strings.
            Action attemptToDeserializeNonStringColumnSourceProperties =
                () => Deserialize(_resourceMetadata, @"
                    {
                        ""propertyA"": {
                            ""Column"": null,
                            ""Lookup"": [],
                            ""Default"": 0
                        }
                    }");
            attemptToDeserializeNonStringColumnSourceProperties
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'propertyA' was " +
                    "expected to have a valid Column Source declaration as its value. It has a Column Source, " +
                    $"but one with invalid content. 'Column', 'Lookup' should be strings: {{{Environment.NewLine}  \"Column\": null,{Environment.NewLine}  \"Lookup\": [],{Environment.NewLine}  \"Default\": 0{Environment.NewLine}}}");

            //When a Column Source object is expected, and has expected keys, Default must be a single value.
            Action attemptToDeserializeComplexDefault =
                () => Deserialize(_resourceMetadata, @"
                    {
                        ""propertyA"": {
                            ""Column"": ""Col1"",
                            ""Lookup"": ""look-up"",
                            ""Default"": { ""Complex"" : ""Value"" }
                        }
                    }");
            attemptToDeserializeComplexDefault
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'propertyA' was " +
                    "expected to have a valid Column Source declaration as its value. It has a Column Source, " +
                    $"but one with an invalid default. 'Default' should be a single value: {{{Environment.NewLine}  \"Column\": \"Col1\",{Environment.NewLine}  \"Lookup\": \"look-up\",{Environment.NewLine}  \"Default\": {{{Environment.NewLine}    \"Complex\": \"Value\"{Environment.NewLine}  }}{Environment.NewLine}}}");
        }

        [Test]
        public void ShouldSerializeAndDeserializeAtypicalArrayMappings()
        {
            //The ODS has many examples of arrays-of-objects:
            //      [ { ... }, { ... }, ...]
            //
            //Naturally the item objects may contain *properties* whose
            //*values* are also arrays.
            //
            //However, there are no known occurrences of arrays where the
            //items themselves are simple values ([1,2]), nor are there any
            //known occurrences of arrays where the items themselves
            //are arrays ([[...], [...]).
            //
            //Still, DataMapSerializer is written to handle such situations.
            //This test proves that such scenarios can be serialized and
            //deserialized without data loss.

            var atypicalResourceMetadata = new[]
            {
                Array(
                    "booleanArrayProperty",
                    new ResourceMetadata { Name = "booleanItem", DataType = "boolean" }),

                Array(
                    "outerArrayProperty",
                    Array(
                        "innerArrayProperty",
                        Object("innerArrayItem", "innerArrayItemType", Property("integerProperty", "integer"))
                    )
                )
            };

            var atypicalMappings = new[]
            {
                MapArray(
                    "booleanArrayProperty",
                    MapColumn("booleanItem", "ColumnK"),
                    MapColumn("booleanItem", "ColumnL"),
                    MapStatic("booleanItem", "true")
                ),

                MapArray(
                    "outerArrayProperty",
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem", MapColumn("integerProperty", "ColumnM")),
                        MapObject("innerArrayItem", MapColumn("integerProperty", "ColumnN"))
                    ),
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem", MapColumn("integerProperty", "ColumnO")),
                        MapObject("innerArrayItem", MapStatic("integerProperty", "123"))
                    )
                )
            };

            var atypicalJsonMap = @"{
                    ""booleanArrayProperty"": [
                        {
                            ""Column"": ""ColumnK""
                        },
                        {
                            ""Column"": ""ColumnL""
                        },
                        true
                    ],
                    ""outerArrayProperty"": [
                        [
                            {
                                ""integerProperty"": {
                                    ""Column"": ""ColumnM""
                                }
                            },
                            {
                                ""integerProperty"": {
                                    ""Column"": ""ColumnN""
                                }
                            }
                        ],
                        [
                            {
                                ""integerProperty"": {
                                    ""Column"": ""ColumnO""
                                }
                            },
                            {
                                ""integerProperty"": 123
                            }
                        ]
                    ]
                }";

            Serialize(atypicalResourceMetadata, atypicalMappings).ShouldMatch(atypicalJsonMap);
            Deserialize(atypicalResourceMetadata, atypicalJsonMap).ShouldMatch(atypicalMappings);

            //Metadata declares that "booleanArrayProperty" array elements should be Column Source object like a column mapping or static value, not an array.
            Action attemptToDeserializeInvalidColumnMapValue =
                () => Deserialize(atypicalResourceMetadata, @"{
                    ""booleanArrayProperty"": [
                        {
                            ""Column"": ""ColumnK""
                        },
                        true,
                        []
                    ]
                }");
            attemptToDeserializeInvalidColumnMapValue
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot deserialize mappings from JSON, because the key 'booleanItem' was expected " +
                    "to have a boolean value. Instead, the value was: []");
        }

        [Test]
        public void ShouldFailToSerializeAmbiguousMappings()
        {
            //If the UI ever permits an ambiguous mapping to be saved,
            //our last line of defense is to detect it at serialization
            //time. Rather than serialize while losing data, we refuse
            //to serialize at all.

            Action attemptToSerializeAmbiguousMapping =
                () => Serialize(_resourceMetadata, new[]
                {
                    new DataMapper
                    {
                        Name = "propertyA",
                        SourceColumn = "Col1",
                        SourceTable = "Lookup",
                        Value = "Static Value", //We can't handle BOTH a static value AND a column lookup.
                        Default = "Default"
                    }
                });
            attemptToSerializeAmbiguousMapping
                .ShouldThrow<InvalidOperationException>()
                .Message
                .ShouldBe(
                    "Cannot serialize mappings to JSON, because the key 'propertyA' has an " +
                    $"ambiguous mapping definition: {{{Environment.NewLine}  \"Name\": \"propertyA\",{Environment.NewLine}  \"SourceColumn\": \"Col1\",{Environment.NewLine}  \"SourceTable\": \"Lookup\",{Environment.NewLine}  \"Default\": \"Default\",{Environment.NewLine}  \"Value\": \"Static Value\",{Environment.NewLine}  \"Children\": []{Environment.NewLine}}}");
        }

        [Test]
        public void ShouldSerializeAndDeserializeStaticValuesRespectingDataTypeWhenLossless()
        {
            //When the user specifies static values, they do so with a string.
            //While serializing, the given values should be turned into their
            //most natural JSON representation as suggested by the metadata.
            //However, if the user provides an incompatible value, we fall
            //back to serializing as a quoted string. This way, we can serialize
            //and deserialize without data loss, giving the user a chance to
            //fix/complete their mapping before later POSTing to the ODS.

            var resourceMetadata = new[]
            {
                Property("stringProperty", "string"),
                Property("dateTimeProperty", "date-time"),
                Property("integerProperty", "integer"),
                Property("booleanProperty", "boolean"),
                Property("numberProperty", "number"),
                Property("invalidInteger", "integer"),
                Property("invalidBoolean", "boolean"),
                Property("invalidNumber", "number"),
                Property("currencyNumber", "number"),
                Property("lossyNumber", "number"),
                Property("unanticipatedType", "some-future-unanticipated-swagger-type")
            };

            var mappings = new[]
            {
                MapStatic("stringProperty", " ABC123 "), //User included whitespace.
                MapStatic("dateTimeProperty", " 08-01-2017 "), //User included whitespace.
                MapStatic("integerProperty", " 123 "), //User included whitespace.
                MapStatic("booleanProperty", " TrUe "), //User included whitespace and varying case.
                MapStatic("numberProperty", " 12.34 "), //User included whitespace.
                MapStatic("invalidInteger", "int typo"),
                MapStatic("invalidBoolean", "bool typo"),
                MapStatic("invalidNumber", "number typo"),
                MapStatic("currencyNumber", " $23.456 "), //User included whitespace.
                MapStatic("lossyNumber", " .567 "), //User included whitespace. Although decimal can parse this, the resulting ToString() would gain a leading zero, so it is a simple example of avoiding data loss for user-provided literals.
                MapStatic("unanticipatedType", " some future unanticipated swagger type value ") //User included whitespace.
            };

            var jsonMap = @"{
                    ""stringProperty"": ""ABC123"",
                    ""dateTimeProperty"": ""08-01-2017"",
                    ""integerProperty"": 123,
                    ""booleanProperty"": true,
                    ""numberProperty"": 12.34,
                    ""invalidInteger"": ""int typo"",
                    ""invalidBoolean"": ""bool typo"",
                    ""invalidNumber"": ""number typo"",
                    ""currencyNumber"": ""$23.456"",
                    ""lossyNumber"": "".567"",
                    ""unanticipatedType"": ""some future unanticipated swagger type value""
                }";

            Serialize(resourceMetadata, mappings)
                .ShouldMatch(jsonMap);

            Deserialize(resourceMetadata, jsonMap)
                .ShouldMatch(
                    MapStatic("stringProperty", "ABC123"), //Trimmed.
                    MapStatic("dateTimeProperty", "08-01-2017"), //Trimmed.
                    MapStatic("integerProperty", "123"), //Trimmed.
                    MapStatic("booleanProperty", "true"), //Normalized to lower case.
                    MapStatic("numberProperty", "12.34"), //Trimmed.
                    MapStatic("invalidInteger", "int typo"),
                    MapStatic("invalidBoolean", "bool typo"),
                    MapStatic("invalidNumber", "number typo"),
                    MapStatic("currencyNumber", "$23.456"), //Trimmed
                    MapStatic("lossyNumber", ".567"), //Trimmed. No extra leading zero introduced.
                    MapStatic("unanticipatedType", "some future unanticipated swagger type value") //Trimmed.
                );
        }

        [Test]
        public void ShouldSerializeAndDeserializeDefaultValuesRespectingDataTypeWhenLossless()
        {
            //When the user specifies default values, they do so with a string.
            //While serializing, the given values should be turned into their
            //most natural JSON representation as suggested by the metadata.
            //However, if the user provides an incompatible value, we fall
            //back to serializing as a quoted string. This way, we can serialize
            //and deserialize without data loss, giving the user a chance to
            //fix/complete their mapping before later POSTing to the ODS.

            var resourceMetadata = new[]
            {
                Property("stringProperty", "string"),
                Property("dateTimeProperty", "date-time"),
                Property("integerProperty", "integer"),
                Property("booleanProperty", "boolean"),
                Property("numberProperty", "number"),
                Property("invalidInteger", "integer"),
                Property("invalidBoolean", "boolean"),
                Property("invalidNumber", "number"),
                Property("currencyNumber", "number"),
                Property("lossyNumber", "number"),
                Property("unanticipatedType", "some-future-unanticipated-swagger-type")
            };

            var mappings = new[]
            {
                MapColumn("stringProperty", "ColumnA", " ABC123 "), //User included whitespace.
                MapColumn("dateTimeProperty", "ColumnB", " 08-01-2017 "), //User included whitespace.
                MapColumn("integerProperty", "ColumnC", " 123 "), //User included whitespace.
                MapColumn("booleanProperty", "ColumnD", " TrUe "), //User included whitespace and varying case.
                MapColumn("numberProperty", "ColumnE", " 12.34 "), //User included whitespace.
                MapColumn("invalidInteger", "ColumnF", "int typo"),
                MapColumn("invalidBoolean", "ColumnG", "bool typo"),
                MapColumn("invalidNumber", "ColumnH", "number typo"),
                MapColumn("currencyNumber", "ColumnI", " $23.456 "), //User included whitespace.
                MapColumn("lossyNumber", "ColumnJ", " .567 "), //User included whitespace. Although decimal can parse this, the resulting ToString() would gain a leading zero, so it is a simple example of avoiding data loss for user-provided literals.
                MapColumn("unanticipatedType", "ColumnK", " some future unanticipated swagger type value ") //User included whitespace.
            };

            var jsonMap = @"{
                    ""stringProperty"": {
                        ""Column"": ""ColumnA"",
                        ""Default"": ""ABC123""
                    },
                    ""dateTimeProperty"": {
                        ""Column"": ""ColumnB"",
                        ""Default"": ""08-01-2017""
                    },
                    ""integerProperty"": {
                        ""Column"": ""ColumnC"",
                        ""Default"": 123
                    },
                    ""booleanProperty"": {
                        ""Column"": ""ColumnD"",
                        ""Default"": true
                    },
                    ""numberProperty"": {
                        ""Column"": ""ColumnE"",
                        ""Default"": 12.34
                    },
                    ""invalidInteger"": {
                        ""Column"": ""ColumnF"",
                        ""Default"": ""int typo""
                    },
                    ""invalidBoolean"": {
                        ""Column"": ""ColumnG"",
                        ""Default"": ""bool typo""
                    },
                    ""invalidNumber"": {
                        ""Column"": ""ColumnH"",
                        ""Default"": ""number typo""
                    },
                    ""currencyNumber"": {
                        ""Column"": ""ColumnI"",
                        ""Default"": ""$23.456""
                    },
                    ""lossyNumber"": {
                        ""Column"": ""ColumnJ"",
                        ""Default"": "".567""
                    },
                    ""unanticipatedType"": {
                        ""Column"": ""ColumnK"",
                        ""Default"": ""some future unanticipated swagger type value""
                    }
                }";

            Serialize(resourceMetadata, mappings)
                .ShouldMatch(jsonMap);

            Deserialize(resourceMetadata, jsonMap)
                .ShouldMatch(
                    MapColumn("stringProperty", "ColumnA", "ABC123"), //Trimmed.
                    MapColumn("dateTimeProperty", "ColumnB", "08-01-2017"), //Trimmed.
                    MapColumn("integerProperty", "ColumnC", "123"), //Trimmed.
                    MapColumn("booleanProperty", "ColumnD", "true"), //Normalized to lower case.
                    MapColumn("numberProperty", "ColumnE", "12.34"), //Trimmed.
                    MapColumn("invalidInteger", "ColumnF", "int typo"),
                    MapColumn("invalidBoolean", "ColumnG", "bool typo"),
                    MapColumn("invalidNumber", "ColumnH", "number typo"),
                    MapColumn("currencyNumber", "ColumnI", "$23.456"), //Trimmed.
                    MapColumn("lossyNumber", "ColumnJ", ".567"), //Trimmed. No extra leading zero introduced.
                    MapColumn("unanticipatedType", "ColumnK", "some future unanticipated swagger type value") //Trimmed.
                );
        }

        private static JToken Serialize(ResourceMetadata[] resourceMetadata, DataMapper[] mappings)
        {
            var dataMapSerializer = new DataMapSerializer("/testResource", resourceMetadata);

            return JToken.Parse(dataMapSerializer.Serialize(mappings, false));
        }

        private static DataMapper[] Deserialize(ResourceMetadata[] resourceMetadata, string jsonMap)
        {
            var dataMapSerializer = new DataMapSerializer("/testResource", resourceMetadata);

            return dataMapSerializer.Deserialize(jsonMap, false);
        }

        private static DataMapper[] Deserialize(ResourceMetadata[] resourceMetadata, JObject jsonMap)
        {
            var dataMapSerializer = new DataMapSerializer("/testResource", resourceMetadata);

            return dataMapSerializer.Deserialize(jsonMap, false);
        }
    }
}
