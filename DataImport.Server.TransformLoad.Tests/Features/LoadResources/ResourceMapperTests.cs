// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Server.TransformLoad.Features.LoadResources;
using DataImport.TestHelpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using static DataImport.TestHelpers.DataMapperBuilder;
using static DataImport.TestHelpers.ResourceMetadataBuilder;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    [TestFixture]
    public class ResourceMapperTests
    {
        [Test]
        public void ShouldMapFromStaticValue()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "string"),
                Property("PropertyC", "string")
            );

            var jsonMap = JsonMap(
                MapStatic("PropertyA", "static value 1"),
                MapStatic("PropertyB", "static value 2"),
                MapStatic("PropertyC", "static value 3")
                );

            var csvRow = new Dictionary<string, string> { {"Col1", "value1"} };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                  ""PropertyA"": ""static value 1"",
                  ""PropertyB"": ""static value 2"",
                  ""PropertyC"": ""static value 3""
                }");
        }

        [Test]
        public void ShouldMapKnownDataTypes()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "date-time"),
                Property("PropertyC", "integer"),
                Property("PropertyD", "boolean"),
                Property("PropertyE", "number"),
                Property("PropertyF", "number")
            );

            var jsonMap = JsonMap(
                MapStatic("PropertyA", "ABC123"),
                MapStatic("PropertyB", "2016-08-01"),
                MapStatic("PropertyC", "123"),
                MapStatic("PropertyD", "true"),
                MapStatic("PropertyE", "1.234"),
                MapStatic("PropertyF", "$23.45")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" } };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                  ""PropertyA"": ""ABC123"",
                  ""PropertyB"": ""2016-08-01"",
                  ""PropertyC"": 123,
                  ""PropertyD"": true,
                  ""PropertyE"": 1.234,
                  ""PropertyF"": 23.45
                }");
        }

        [Test]
        public void ShouldThrowOnValueTypedPropertiesWhenValuesCannotBeParsed()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Property("PropertyA", "integer"),
                Property("PropertyB", "boolean"),
                Property("PropertyC", "unexpected-type-from-swagger"),
                Property("PropertyD", "number")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1"),
                MapColumn("PropertyB", "Col2"),
                MapColumn("PropertyC", "Col3"),
                MapColumn("PropertyD", "Col4")
            );

            var csvRows = new[]
            {
                new Dictionary<string, string> { { "Col1", "123" }, { "Col2", "false" }, { "Col3", "" }, { "Col4", " $2.34 " } },
                new Dictionary<string, string> { { "Col1", "INVALID" }, { "Col2", "true" }, { "Col3", "" }, { "Col4", "2.34" } },
                new Dictionary<string, string> { { "Col1", "123" }, { "Col2", "INVALID" }, { "Col3", "" }, { "Col4", "2.34" } },
                new Dictionary<string, string> { { "Col1", "123" }, { "Col2", "true" }, { "Col3", "ValueForUnexpectedType" }, { "Col4", "2.34" } },
                new Dictionary<string, string> { { "Col1", "123" }, { "Col2", "true" }, { "Col3", "" }, { "Col4", "INVALID" } }
            };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRows[0]).ShouldMatch(
                @"{
                    ""PropertyA"": 123,
                    ""PropertyB"": false,
                    ""PropertyD"": 2.34
                  }");

            Action attemptInvalidInteger = () => mapper.ApplyMap(csvRows[1]);
            attemptInvalidInteger
                .ShouldThrow<TypeConversionException>()
                .Message
                .ShouldBe("Column \"Col1\" contains a value for property \"PropertyA\" which cannot be converted to type \"integer\".");

            Action attemptInvalidBoolean = () => mapper.ApplyMap(csvRows[2]);
            attemptInvalidBoolean
                .ShouldThrow<TypeConversionException>()
                .Message
                .ShouldBe("Column \"Col2\" contains a value for property \"PropertyB\" which cannot be converted to type \"boolean\".");

            Action attemptUnexpectedType = () => mapper.ApplyMap(csvRows[3]);
            attemptUnexpectedType
                .ShouldThrow<TypeConversionException>()
                .Message
                .ShouldBe("Column \"Col3\" contains a value for property \"PropertyC\" which cannot be converted to unsupported type \"unexpected-type-from-swagger\".");

            Action attemptInvalidNumber = () => mapper.ApplyMap(csvRows[4]);
            attemptInvalidNumber
                .ShouldThrow<TypeConversionException>()
                .Message
                .ShouldBe("Column \"Col4\" contains a value for property \"PropertyD\" which cannot be converted to type \"number\".");

            var staticJsonMap = JsonMap(
                MapStatic("PropertyA", "123"),
                MapStatic("PropertyB", "false"),
                MapStatic("PropertyC", ""),
                MapStatic("PropertyD", "INVALID")
            );

            var staticMapper = ResourceMapper(resourceMetadata, staticJsonMap, lookups);

            Action attemptInvalidStatic = () => staticMapper.ApplyMap(csvRows[0]);
            attemptInvalidStatic
                .ShouldThrow<TypeConversionException>()
                .Message
                .ShouldBe("Static value for property \"PropertyD\" cannot be converted to type \"number\".");
        }

        [Test]
        public void ShouldMapFromColumn()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "string"),
                Property("PropertyC", "string")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1"),
                MapColumn("PropertyB", "Col2", "default value"),
                MapColumn("PropertyC", "Col3", "default value")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" }, { "Col2", "value2" }, { "Col3", "" } };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                  ""PropertyA"": ""value1"",
                  ""PropertyB"": ""value2"",
                  ""PropertyC"": ""default value""
                }");
        }

        [Test]
        public void ShouldMapFromCaseSensitiveLookupTable()
        {
            var lookups = new[]
            {
                new Lookup { SourceTable = "other-lookup", Key = "value1", Value = "should not be used" },
                new Lookup { SourceTable = "other-lookup", Key = "value2", Value = "should not be used" },

                new Lookup { SourceTable = "test-lookup", Key = "value1", Value = "lookup for value1" },
                new Lookup { SourceTable = "test-lookup", Key = "value2", Value = "lookup for value2" },
                new Lookup { SourceTable = "test-lookup", Key = "value3", Value = "lookup for value3" }
            };

            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "string"),
                Property("PropertyC", "string"),
                Property("PropertyD", "string"),
                Property("PropertyE", "string")
            );

            var jsonMap = JsonMap(
                MapLookup("PropertyA", "Col1", "test-lookup"),
                MapLookup("PropertyB", "Col2", "test-lookup"),
                MapLookup("PropertyC", "Col3", "test-lookup"),
                MapLookup("PropertyD", "Col4", "test-lookup"),
                MapLookup("PropertyE", "Col5", "test-lookup", "default value")
            );

            var csvRow = new Dictionary<string, string>
            {
                { "Col1", "value1" }, { "Col2", "value2" }, { "Col3", "value3" }, { "Col4", "" }, { "Col5", "" }
            };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                  ""PropertyA"": ""lookup for value1"",
                  ""PropertyB"": ""lookup for value2"",
                  ""PropertyC"": ""lookup for value3"",
                  ""PropertyE"": ""default value""
                }");

            csvRow["Col3"] = csvRow["Col3"].ToUpper();

            Action attemptFailingLookupDueToCaseSensitivity = () => mapper.ApplyMap(csvRow);

            attemptFailingLookupDueToCaseSensitivity
                .ShouldThrow<MissingLookupKeyException>()
                .Message
                .ShouldBe("Column \"Col3\" contains a value which is not defined by Lookup \"test-lookup\".");
        }

        [Test]
        public void ShouldThrowOnMissingRequiredColumn()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Property("PropertyName", "string")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyName", "MissingColumn")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" } };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            Action action = () => mapper.ApplyMap(csvRow);
            action
                .ShouldThrow<MissingColumnException>()
                .Message.ShouldBe("Missing column(s) in source file: MissingColumn");
        }

        [Test]
        public void ShouldThrowOnMissingRequiredColumnDuringTableLookups()
        {
            var lookups = new[]
            {
                new Lookup { SourceTable = "test-lookup", Key = "foo", Value = "foo value" },
                new Lookup { SourceTable = "test-lookup", Key = "woo", Value = "woo value" }
            };

            var resourceMetadata = Metadata(
                Property("PropertyName", "string")
            );

            var jsonMap = JsonMap(
                MapLookup("PropertyName", "MissingColumn", "test-lookup")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" } };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            Action action = () => mapper.ApplyMap(csvRow);
            action
                .ShouldThrow<MissingColumnException>()
                .Message.ShouldBe("Missing column(s) in source file: MissingColumn");
        }

        [Test]
        public void ShouldMapArraysOfObjects()
        {
            var lookups = new[]
            {
                new Lookup { SourceTable = "test-lookup", Key = "value1", Value = "lookup for value1" },
                new Lookup { SourceTable = "test-lookup", Key = "value2", Value = "lookup for value2" }
            };

            var resourceMetadata = Metadata(
                Array(
                    "arrayProperty",
                    Object("arrayItem", "arrayItemType", Property("title", "string"))
                ));

            var jsonMap = JsonMap(
                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("title", "Col1")),
                    MapObject("arrayItem", MapLookup("title", "Col2", "test-lookup")),
                    MapObject("arrayItem", MapStatic("title", "static value"))
                ));

            var csvRow = new Dictionary<string, string> { {"Col1", "value1"}, {"Col2", "value2"} };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                    ""arrayProperty"": [
                        {
                            ""title"": ""value1""
                        },
                        {
                            ""title"": ""lookup for value2""
                        },
                        {
                            ""title"": ""static value""
                        }
                    ]
                }");
        }

        [Test]
        public void ShouldMapAtypicalArrays()
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
            //Still, ResourceMapper is written to handle such situations.
            //This test proves that such scenarios behave predictably.

            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                Array(
                    "stringArrayProperty",
                    new ResourceMetadata { Name = "simpleItem", DataType = "string" }
                ),

                Array(
                    "outerArrayProperty",
                    Array(
                        "innerArrayProperty",
                        Object("innerArrayItem", "innerArrayItemType", Property("title", "string"))
                    )
                ));

            var jsonMap = JsonMap(
                MapArray(
                    "stringArrayProperty",
                    MapColumn("simpleItem", "Col1"),
                    MapColumn("simpleItem", "Col2")
                ),

                MapArray(
                    "outerArrayProperty",
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem", MapColumn("title", "Col3")),
                        MapObject("innerArrayItem", MapColumn("title", "Col4"))
                    ),
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem", MapColumn("title", "Col5")),
                        MapObject("innerArrayItem", MapColumn("title", "Col6"))
                    )
                ));

            var csvRow = new Dictionary<string, string>
            {
                { "Col1", "value1" },
                { "Col2", "value2" },
                { "Col3", "value3" },
                { "Col4", "value4" },
                { "Col5", "value5" },
                { "Col6", "value6" }
            };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(@"{
                    ""stringArrayProperty"": [
                        ""value1"",
                        ""value2""
                    ],
                    ""outerArrayProperty"": [
                        [
                            {
                                ""title"": ""value3""
                            },
                            {
                                ""title"": ""value4""
                            }
                        ],
                        [
                            {
                                ""title"": ""value5""
                            },
                            {
                                ""title"": ""value6""
                            }
                        ]
                    ]
                }");
        }

        [Test]
        public void ShouldMapNestedObjects()
        {
            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(Object("Parent", "Parent",
                Object("ChildObject", "ChildObject",
                    Object("GrandchildObject", "GrandchildObject",

                        Object("GreatGrandchildObject", "GreatGrandchildObject",
                            Property("GreatGreatGrandchildProperty", "string")),

                        Property("GreatGrandchildProperty", "string")
                    ),

                    Property("GrandchildProperty", "string")
                ),

                Property("ChildProperty", "string")
            ));

            var jsonMap = JsonMap(MapObject("Parent",
                MapObject("ChildObject",
                    MapObject("GrandchildObject",

                        MapObject("GreatGrandchildObject",
                            MapColumn("GreatGreatGrandchildProperty", "Col1")),

                        MapColumn("GreatGrandchildProperty", "Col2")
                    ),

                    MapColumn("GrandchildProperty", "Col3")
                ),

                MapColumn("ChildProperty", "Col4")
            ));

            var csvRow = new Dictionary<string, string> { {"Col1", "value1"}, {"Col2", "value2"}, {"Col3", "value3"}, {"Col4", "value4" } };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                    ""Parent"": {
                        ""ChildObject"": {
                            ""GrandchildObject"": {
                                ""GreatGrandchildObject"": {
                                    ""GreatGreatGrandchildProperty"": ""value1""
                                },
                                ""GreatGrandchildProperty"": ""value2""
                            },
                            ""GrandchildProperty"": ""value3""
                        },
                        ""ChildProperty"": ""value4""
                    }
                }");
        }

        [Test]
        public void ShouldThrowOnMissingLookupKeyForNonEmptyCells()
        {
            var lookups = new[]
            {
                new Lookup
                {
                    SourceTable = "IncompleteLookup",
                    Key = "ExpectedCellValue",
                    Value = "Successful Lookup Value"
                }
            };

            var csvRows = new[]
            {
                new Dictionary<string, string> { { "Column1", "" } },
                new Dictionary<string, string> { { "Column1", "ExpectedCellValue" } },
                new Dictionary<string, string> { { "Column1", "UnexpectedCellValue" } },
            };

            var resourceMetadata = Metadata(
                Property("PropertyA", "string")
            );

            var jsonMap = JsonMap(
                MapLookup("PropertyA", "Column1", "IncompleteLookup", @default: "Default Value")
            );

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRows[0]).ShouldMatch(
                @"{
                      ""PropertyA"": ""Default Value""
                  }");

            mapper.ApplyMap(csvRows[1]).ShouldMatch(
                @"{
                      ""PropertyA"": ""Successful Lookup Value""
                  }");

            Action attemptFailingLookupDueToUnexpectedNonEmptyCellValue = () => mapper.ApplyMap(csvRows[2]);

            attemptFailingLookupDueToUnexpectedNonEmptyCellValue.ShouldThrow<MissingLookupKeyException>()
                .Message
                .ShouldBe("Column \"Column1\" contains a value which is not defined by Lookup \"IncompleteLookup\".");
        }

        [Test]
        public void ShouldHandleMissingValuesWhenDeterminingValuesToBeMappedToProperties()
        {
            var lookups = new[]
            {
                //Note how empty cell values won't match any of these present keys.
                new Lookup { SourceTable = "EmptyProducingLookup", Key = "PopulatedCellValue", Value = "" },
                new Lookup { SourceTable = "ValueProducingLookup", Key = "PopulatedCellValue", Value = "Lookup Value" }
            };

            var csvRow = new Dictionary<string, string>
            {
                { "EmptyColumn", "" },
                { "PopulatedColumn", "PopulatedCellValue" }
            };

            var resourceMetadata = Metadata(
                //Static Values
                Property("Static_Null", "string"),
                Property("Static_Empty", "string"),
                Property("Static_Populated", "string"),

                //Direct Column Reference to Empty Cells
                Property("EmptyColumn_NullDefault", "string"),
                Property("EmptyColumn_EmptyDefault", "string"),
                Property("EmptyColumn_WithDefault", "string"),

                //Direct Column Reference to Populated Cells
                Property("PopulatedColumn_NullDefault", "string"),
                Property("PopulatedColumn_EmptyDefault", "string"),
                Property("PopulatedColumn_WithDefault", "string"),

                //Lookups Applied to Empty Cells
                Property("EmptyColumn_EmptyProducingLookup_NullDefault", "string"),
                Property("EmptyColumn_ValueProducingLookup_NullDefault", "string"),

                Property("EmptyColumn_EmptyProducingLookup_EmptyDefault", "string"),
                Property("EmptyColumn_ValueProducingLookup_EmptyDefault", "string"),

                Property("EmptyColumn_EmptyProducingLookup_WithDefault", "string"),
                Property("EmptyColumn_ValueProducingLookup_WithDefault", "string"),

                //Lookups Applied to Populated Cells
                Property("PopulatedColumn_EmptyProducingLookup_NullDefault", "string"),
                Property("PopulatedColumn_ValueProducingLookup_NullDefault", "string"),

                Property("PopulatedColumn_EmptyProducingLookup_EmptyDefault", "string"),
                Property("PopulatedColumn_ValueProducingLookup_EmptyDefault", "string"),

                Property("PopulatedColumn_EmptyProducingLookup_WithDefault", "string"),
                Property("PopulatedColumn_ValueProducingLookup_WithDefault", "string")
            );

            var jsonMap = JsonMap(
                //Static Values
                MapStatic("Static_Null", value: null),
                MapStatic("Static_Empty", value: ""),
                MapStatic("Static_Populated", value: "Static Value"),

                //Direct Column Reference to Empty Cells
                MapColumn("EmptyColumn_NullDefault", "EmptyColumn", @default: null),
                MapColumn("EmptyColumn_EmptyDefault", "EmptyColumn", @default: ""),
                MapColumn("EmptyColumn_WithDefault", "EmptyColumn", @default: "Default Value"),

                //Direct Column Reference to Populated Cells
                MapColumn("PopulatedColumn_NullDefault", "PopulatedColumn", @default: null),
                MapColumn("PopulatedColumn_EmptyDefault", "PopulatedColumn", @default: ""),
                MapColumn("PopulatedColumn_WithDefault", "PopulatedColumn", @default: "Default Value"),

                //Lookups Applied to Empty Cells
                MapLookup("EmptyColumn_EmptyProducingLookup_NullDefault", "EmptyColumn", "EmptyProducingLookup", @default: null),
                MapLookup("EmptyColumn_ValueProducingLookup_NullDefault", "EmptyColumn", "ValueProducingLookup", @default: null),

                MapLookup("EmptyColumn_EmptyProducingLookup_EmptyDefault", "EmptyColumn", "EmptyProducingLookup", @default: ""),
                MapLookup("EmptyColumn_ValueProducingLookup_EmptyDefault", "EmptyColumn", "ValueProducingLookup", @default: ""),

                MapLookup("EmptyColumn_EmptyProducingLookup_WithDefault", "EmptyColumn", "EmptyProducingLookup", @default: "Default Value"),
                MapLookup("EmptyColumn_ValueProducingLookup_WithDefault", "EmptyColumn", "ValueProducingLookup", @default: "Default Value"),

                //Lookups Applied to Populated Cells
                MapLookup("PopulatedColumn_EmptyProducingLookup_NullDefault", "PopulatedColumn", "EmptyProducingLookup", @default: null),
                MapLookup("PopulatedColumn_ValueProducingLookup_NullDefault", "PopulatedColumn", "ValueProducingLookup", @default: null),

                MapLookup("PopulatedColumn_EmptyProducingLookup_EmptyDefault", "PopulatedColumn", "EmptyProducingLookup", @default: ""),
                MapLookup("PopulatedColumn_ValueProducingLookup_EmptyDefault", "PopulatedColumn", "ValueProducingLookup", @default: ""),

                MapLookup("PopulatedColumn_EmptyProducingLookup_WithDefault", "PopulatedColumn", "EmptyProducingLookup", @default: "Default Value"),
                MapLookup("PopulatedColumn_ValueProducingLookup_WithDefault", "PopulatedColumn", "ValueProducingLookup", @default: "Default Value")
            );

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(csvRow).ShouldMatch(
                @"{
                      ""Static_Populated"": ""Static Value"",

                      ""EmptyColumn_WithDefault"": ""Default Value"",

                      ""PopulatedColumn_NullDefault"": ""PopulatedCellValue"",
                      ""PopulatedColumn_EmptyDefault"": ""PopulatedCellValue"",
                      ""PopulatedColumn_WithDefault"": ""PopulatedCellValue"",

                      ""EmptyColumn_EmptyProducingLookup_WithDefault"": ""Default Value"",
                      ""EmptyColumn_ValueProducingLookup_WithDefault"": ""Default Value"",

                      ""PopulatedColumn_ValueProducingLookup_NullDefault"": ""Lookup Value"",

                      ""PopulatedColumn_ValueProducingLookup_EmptyDefault"": ""Lookup Value"",

                      ""PopulatedColumn_EmptyProducingLookup_WithDefault"": ""Default Value"",
                      ""PopulatedColumn_ValueProducingLookup_WithDefault"": ""Lookup Value""
                    }");
        }

        public static readonly Lookup[] SampleLookups =
        {
            new Lookup { Id=1, SourceTable="mclass-grade", Key="K", Value="Kindergarten"},
            new Lookup { Id=2, SourceTable="mclass-grade", Key="1", Value="First grade"},
            new Lookup { Id=3, SourceTable="mclass-grade", Key="2", Value="Second grade"},
            new Lookup { Id=4, SourceTable="mclass-grade", Key="3", Value="Third grade"},
            new Lookup { Id=5, SourceTable="mclass-grade", Key="4", Value="Fourth grade"},
            new Lookup { Id=6, SourceTable="mclass-grade", Key="5", Value="Fifth grade"},
            new Lookup { Id=7, SourceTable="mclass-testdate", Key="MOY", Value="43191"},
            new Lookup { Id=8, SourceTable="mclass-gender", Key="M", Value="Male"},
            new Lookup { Id=9, SourceTable="mclass-gender", Key="F", Value="Female"},
            new Lookup { Id=10, SourceTable="gender", Key="U", Value="Unknown"},
            new Lookup { Id=11, SourceTable="gender", Key="M", Value="Male"},
            new Lookup { Id=12, SourceTable="gender", Key="F", Value="Female"},
            new Lookup { Id=12, SourceTable="mclass-schools", Key="Randall Munroe Middle School", Value="123"}
        };

        public static Dictionary<string, string> SampleCsvRow()
        {
            return new Dictionary<string, string>
            {
                ["School Year"] = "2016-2017",
                ["School Name"] = "Randall Munroe Middle School",
                ["Student Last Name"] = "Jacobs",
                ["Student First Name"] = "John",
                ["Student Middle Name"] = "C",
                ["Grade"] = "",
                ["Primary ID - Student ID (State ID)"] = "3732544882",
                ["Date of Birth"] = "",
                ["GENDER"] = "M",
                ["RACE"] = "",
                ["Assessment"] = "mCLASS:DIBELS",
                ["Assessment Grade"] = "K",
                ["Benchmark Period"] = "MOY",
                ["Assessment Measure-Composite Score-Levels"] = "Benchmark",
                ["Assessment Measure-Composite Score-Score"] = "156",
                ["Assessment Measure-FSF-Levels"] = "Benchmark",
                ["Assessment Measure-FSF-Score"] = "32",
                ["Assessment Measure-LNF-Levels"] = "Not Determined",
                ["Assessment Measure-LNF-Score"] = "47",
                ["Assessment Measure-PSF-Levels"] = "Benchmark",
                ["Assessment Measure-PSF-Score"] = "53",
                ["Assessment Measure-NWF (CLS)-Levels"] = "Benchmark",
                ["Assessment Measure-NWF (CLS)-Score"] = "24",
                ["Assessment Measure-NWF (WWR)-Levels"] = "Not Determined",
                ["Assessment Measure-NWF (WWR)-Score"] = "8",
                ["Assessment Measure-DORF (Fluency)-Levels"] = "",
                ["Assessment Measure-DORF (Fluency)-Score"] = "",
                ["Assessment Measure-DORF (Accuracy)-Levels"] = "",
                ["Assessment Measure-DORF (Accuracy)-Score"] = "",
                ["Assessment Measure-DORF (Retell)-Levels"] = "",
                ["Assessment Measure-DORF (Retell)-Score"] = "",
                ["Assessment Measure-DORF (Retell Quality)-Levels"] = "",
                ["Assessment Measure-DORF (Retell Quality)-Score"] = "",
                ["Assessment Measure-DORF (Errors)-Score"] = "",
                ["Assessment Measure-Daze-Levels"] = "",
                ["Assessment Measure-Daze-Score"] = "",
                ["Assessment Measure-Daze (Correct)-Score"] = "",
                ["Assessment Measure-Daze (Incorrect)-Score"] = ""
            };
        }

        [Test]
        public void ShouldSuccessfullyMapFlatResources()
        {
            var studentMetadata = Metadata(
                Property("studentUniqueId", "string"),
                Property("firstName", "string"),
                Property("lastSurname", "string"),
                Property("sexType", "string"),
                Property("birthDate", "date-time"),
                Property("hispanicLatinoEthnicity", "boolean"),
                Property("hypotheticalProperty", "string")
            );
            var studentDataMap = JsonMap(
                    MapColumn("studentUniqueId", "Primary ID - Student ID (State ID)"),
                    MapColumn("firstName", "Student First Name"),
                    MapColumn("lastSurname", "Student Last Name"),
                    MapLookup("sexType", "GENDER", "mclass-gender", "Not Selected"),
                    MapColumn("birthDate", "Date of Birth", "1900-01-01"),
                    MapLookup("hispanicLatinoEthnicity", "RACE", "mclass-progress-hispanic", "false"),
                    MapStatic("hypotheticalProperty", "STATIC VALUE")
                );

            var mapper = ResourceMapper(studentMetadata, studentDataMap, SampleLookups);
            mapper.ApplyMap(SampleCsvRow()).ShouldMatch(
                @"{
                    ""studentUniqueId"": ""3732544882"",
                    ""firstName"": ""John"",
                    ""lastSurname"": ""Jacobs"",
                    ""sexType"": ""Male"",
                    ""birthDate"": ""1900-01-01"",
                    ""hispanicLatinoEthnicity"": false,
                    ""hypotheticalProperty"": ""STATIC VALUE""
                }");
        }

        [Test]
        public void ShouldMapSubsetOfMetadata()
        {
            //Metadata describes many fields, some of which may not even be required by the ODS.
            //If a JSON Map is a subset of the full potential described by the metadata, we should
            //be able to process the given JSON Map anyway. The order each key appears in the JSON
            //Map is not meaningful, but is preserved as the most natural behavior.

            var studentMetadata = Metadata(
                Property("studentUniqueId", "string"),
                Property("firstName", "string"),
                Property("lastSurname", "string"),
                Property("sexType", "string"),
                Property("birthDate", "date-time"),
                Property("hispanicLatinoEthnicity", "boolean")
            );
            var studentDataMap = JsonMap(
                MapColumn("lastSurname", "Student Last Name"),
                MapColumn("firstName", "Student First Name"),
                MapColumn("studentUniqueId", "Primary ID - Student ID (State ID)")
            );

            var mapper = ResourceMapper(studentMetadata, studentDataMap, SampleLookups);
            mapper.ApplyMap(SampleCsvRow()).ShouldMatch(
                @"{
                    ""lastSurname"": ""Jacobs"",
                    ""firstName"": ""John"",
                    ""studentUniqueId"": ""3732544882""
                }");
        }

        [Test]
        public void ShouldHandleUnmappedProperties()
        {
            var studentMetadata = Metadata(
                Property("studentUniqueId", "string"),
                Property("firstName", "string"),
                Property("lastSurname", "string"),
                Property("sexType", "string"),
                Property("birthDate", "date-time"),
                Property("hispanicLatinoEthnicity", "boolean"),
                Property("hypotheticalProperty", "string")
            );
            var studentDataMap = JsonMap(
                    MapColumn("studentUniqueId", "Primary ID - Student ID (State ID)"),

                    //Unmapped properties may exist because the Swagger metadata describes them, even
                    //if the user provided no mapping information.
                    Unmapped("firstName"),
                    Unmapped("lastSurname"),
                    Unmapped("sexType"),
                    Unmapped("birthDate"),
                    Unmapped("hispanicLatinoEthnicity"),
                    Unmapped("hypotheticalProperty")
                );

            var mapper = ResourceMapper(studentMetadata, studentDataMap, SampleLookups);
            mapper.ApplyMap(SampleCsvRow()).ShouldMatch(
                @"{
                    ""studentUniqueId"": ""3732544882""
                }");
        }

        [Test]
        public void ShouldSuccessfullyMapNestedResources()
        {
            var studentSchoolAssociationMetadata = Metadata(

                Object("schoolReference", "schoolReference",
                    Property("schoolId", "integer")),

                Object("studentReference", "studentReference",
                    Property("studentUniqueId", "string")),

                Property("entryDate", "date-time"),

                Property("entryGradeLevelDescriptor", "string")
            );
            var studentSchoolAssociationMap = JsonMap(

                    MapObject("schoolReference",
                        MapLookup("schoolId", "School Name", "mclass-schools")),

                    MapObject("studentReference",
                        MapColumn("studentUniqueId", "Primary ID - Student ID (State ID)")),

                    MapStatic("entryDate", "2016-08-01"),

                    MapStatic("entryGradeLevelDescriptor", "Twelfth grade")
                );

            var mapper = ResourceMapper(studentSchoolAssociationMetadata, studentSchoolAssociationMap, SampleLookups);
            mapper.ApplyMap(SampleCsvRow()).ShouldMatch(
                @"{
                    ""schoolReference"": {
                        ""schoolId"": 123
                    },
                    ""studentReference"": {
                        ""studentUniqueId"": ""3732544882""
                    },
                    ""entryDate"": ""2016-08-01"",
                    ""entryGradeLevelDescriptor"": ""Twelfth grade""
                }");
        }

        [Test]
        public void ShouldSuccessfullyMapComplexResourcesWithNestedArrays()
        {
            var studentAssessmentMetadata = Metadata(

                #region assessmentReference

                Object("assessmentReference", "assessmentReference",
                    Property("title", "string"),
                    Property("assessedGradeLevelDescriptor", "string"),
                    Property("academicSubjectDescriptor", "string"),
                    Property("version", "integer"))

                #endregion
                ,
                #region studentReference

                Object("studentReference", "studentReference",
                    Property("studentUniqueId", "string"))

                #endregion
                ,
                #region administrationDate

                Property("administrationDate", "date-time")

                #endregion
                ,
                #region performanceLevels

                Array("performanceLevels",
                    Object("studentAssessmentPerformanceLevel", "studentAssessmentPerformanceLevel",
                        Property("performanceLevelDescriptor", "string")))

                #endregion
                ,
                #region scoreResults

                Array("scoreResults",
                    Object("studentAssessmentScoreResult", "studentAssessmentScoreResult",
                        Property("assessmentReportingMethodType", "string"),
                        Property("result", "string"),
                        Property("resultDatatypeType", "string")))

                #endregion
                ,
                Array("studentObjectiveAssessments",
                    #region Item Metadata
                    Object(
                        "studentAssessmentStudentObjectiveAssessment",
                        "studentAssessmentStudentObjectiveAssessment",
                        Object(
                            "objectiveAssessmentReference",
                            "objectiveAssessmentReference",
                            Property("assessmentTitle", "string"),
                            Property("academicSubjectDescriptor", "string"),
                            Property("assessedGradeLevelDescriptor", "string"),
                            Property("version", "integer"),
                            Property("identificationCode", "string"))
                        ,
                        Array("performanceLevels",
                            Object(
                                "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                Property("performanceLevelDescriptor", "string")))
                        ,
                        Array("scoreResults",
                            Object(
                                "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                Property("assessmentReportingMethodType", "string"),
                                Property("result", "string"),
                                Property("resultDatatypeType", "string"))))
                    #endregion
                )
            );
            var studentAssessmentMap = JsonMap(

                    #region assessmentReference

                    MapObject("assessmentReference",
                        MapColumn("title", "Assessment"),
                        MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                        MapStatic("academicSubjectDescriptor", "Reading"),
                        MapStatic("version", "1"))

                    #endregion
                    ,
                    #region studentReference

                    MapObject("studentReference",
                        MapColumn("studentUniqueId", "Primary ID - Student ID (State ID)"))

                    #endregion
                    ,
                    #region administrationDate

                    MapLookup("administrationDate", "Benchmark Period", "mclass-testdate")

                    #endregion
                    ,
                    #region performanceLevels

                    MapArray("performanceLevels",
                        MapObject("studentAssessmentPerformanceLevel",
                            MapColumn("performanceLevelDescriptor",
                                "Assessment Measure-Composite Score-Levels")))

                    #endregion
                    ,
                    #region scoreResults

                    MapArray("scoreResults",
                        MapObject("studentAssessmentScoreResult",
                            MapStatic("assessmentReportingMethodType", "Number score"),
                            MapColumn("result", "Assessment Measure-Composite Score-Score"),
                            MapStatic("resultDatatypeType", "Integer")))

                    #endregion
                    ,
                    MapArray("studentObjectiveAssessments",
                        #region [0]
                        MapObject(
                            "studentAssessmentStudentObjectiveAssessment",
                            MapObject("objectiveAssessmentReference",
                                MapColumn("assessmentTitle", "Assessment"),
                                MapStatic("academicSubjectDescriptor", "Reading"),
                                MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                                MapStatic("version", "1"),
                                MapStatic("identificationCode", "First Sound Fluency"))
                            ,
                            MapArray("performanceLevels",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                    MapColumn("performanceLevelDescriptor", "Assessment Measure-FSF-Levels")))
                            ,
                            MapArray("scoreResults",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                    MapStatic("assessmentReportingMethodType", "Number score"),
                                    MapColumn("result", "Assessment Measure-FSF-Score"),
                                    MapStatic("resultDatatypeType", "Integer"))))
                        #endregion
                        ,
                        #region [1]
                        MapObject(
                            "studentAssessmentStudentObjectiveAssessment",
                            MapObject(
                                "objectiveAssessmentReference",
                                MapColumn("assessmentTitle", "Assessment"),
                                MapStatic("academicSubjectDescriptor", "Reading"),
                                MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                                MapStatic("version", "1"),
                                MapStatic("identificationCode", "Letter Naming Fluency"))
                            ,
                            MapArray("performanceLevels",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                    MapColumn("performanceLevelDescriptor", "Assessment Measure-LNF-Levels")))
                            ,
                            MapArray("scoreResults",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                    MapStatic("assessmentReportingMethodType", "Number score"),
                                    MapColumn("result", "Assessment Measure-LNF-Score"),
                                    MapStatic("resultDatatypeType", "Integer"))))
                        #endregion
                        ,
                        #region [2]
                        MapObject(
                            "studentAssessmentStudentObjectiveAssessment",
                            MapObject(
                                "objectiveAssessmentReference",

                                MapColumn("assessmentTitle", "Assessment"),
                                MapStatic("academicSubjectDescriptor", "Reading"),
                                MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                                MapStatic("version", "1"),
                                MapStatic("identificationCode", "Phoneme Segmentation Fluency"))

                            ,
                            MapArray("performanceLevels",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                    MapColumn("performanceLevelDescriptor", "Assessment Measure-PSF-Levels")))
                            ,
                            MapArray("scoreResults",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentScoreResult",

                                    MapStatic("assessmentReportingMethodType", "Number score"),
                                    MapColumn("result", "Assessment Measure-PSF-Score"),
                                    MapStatic("resultDatatypeType", "Integer"))))
                        #endregion
                        ,
                        #region [3]
                        MapObject(
                            "studentAssessmentStudentObjectiveAssessment",
                            MapObject(
                                "objectiveAssessmentReference",
                                MapColumn("assessmentTitle", "Assessment"),
                                MapStatic("academicSubjectDescriptor", "Reading"),
                                MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                                MapStatic("version", "1"),
                                MapStatic("identificationCode", "Nonsense Word Fluency - CLS"))

                            ,
                            MapArray("performanceLevels",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                    MapColumn("performanceLevelDescriptor",
                                        "Assessment Measure-NWF (CLS)-Levels")))
                            ,
                            MapArray("scoreResults",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentScoreResult",

                                    MapStatic("assessmentReportingMethodType", "Number score"),
                                    MapColumn("result", "Assessment Measure-NWF (CLS)-Score"),
                                    MapStatic("resultDatatypeType", "Integer"))))
                        #endregion
                        ,
                        #region [4]
                        MapObject(
                            "studentAssessmentStudentObjectiveAssessment",
                            MapObject(
                                "objectiveAssessmentReference",
                                MapColumn("assessmentTitle", "Assessment"),
                                MapStatic("academicSubjectDescriptor", "Reading"),
                                MapLookup("assessedGradeLevelDescriptor", "Assessment Grade", "mclass-grade"),
                                MapStatic("version", "1"),
                                MapStatic("identificationCode", "Nonsense Word Fluency - WWR"))
                            ,
                            MapArray("performanceLevels",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                    MapColumn("performanceLevelDescriptor",
                                        "Assessment Measure-NWF (WWR)-Levels")))
                            ,
                            MapArray("scoreResults",
                                MapObject(
                                    "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                    MapStatic("assessmentReportingMethodType", "Number score"),
                                    MapColumn("result", "Assessment Measure-NWF (WWR)-Score"),
                                    MapStatic("resultDatatypeType", "Integer"))))
                        #endregion
                    )
                );

            var mapper = ResourceMapper(studentAssessmentMetadata, studentAssessmentMap, SampleLookups);
            mapper.ApplyMap(SampleCsvRow()).ShouldMatch(
                @"{
                      ""assessmentReference"": {
                        ""title"": ""mCLASS:DIBELS"",
                        ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                        ""academicSubjectDescriptor"": ""Reading"",
                        ""version"": 1
                      },
                      ""studentReference"": {
                        ""studentUniqueId"": ""3732544882""
                      },
                      ""administrationDate"": ""43191"",
                      ""performanceLevels"": [
                        {
                          ""performanceLevelDescriptor"": ""Benchmark""
                        }
                      ],
                      ""scoreResults"": [
                        {
                          ""assessmentReportingMethodType"": ""Number score"",
                          ""result"": ""156"",
                          ""resultDatatypeType"": ""Integer""
                        }
                      ],
                      ""studentObjectiveAssessments"": [
                        {
                          ""objectiveAssessmentReference"": {
                            ""assessmentTitle"": ""mCLASS:DIBELS"",
                            ""academicSubjectDescriptor"": ""Reading"",
                            ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                            ""version"": 1,
                            ""identificationCode"": ""First Sound Fluency""
                          },
                          ""performanceLevels"": [
                            {
                              ""performanceLevelDescriptor"": ""Benchmark""
                            }
                          ],
                          ""scoreResults"": [
                            {
                              ""assessmentReportingMethodType"": ""Number score"",
                              ""result"": ""32"",
                              ""resultDatatypeType"": ""Integer""
                            }
                          ]
                        },
                        {
                          ""objectiveAssessmentReference"": {
                            ""assessmentTitle"": ""mCLASS:DIBELS"",
                            ""academicSubjectDescriptor"": ""Reading"",
                            ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                            ""version"": 1,
                            ""identificationCode"": ""Letter Naming Fluency""
                          },
                          ""performanceLevels"": [
                            {
                              ""performanceLevelDescriptor"": ""Not Determined""
                            }
                          ],
                          ""scoreResults"": [
                            {
                              ""assessmentReportingMethodType"": ""Number score"",
                              ""result"": ""47"",
                              ""resultDatatypeType"": ""Integer""
                            }
                          ]
                        },
                        {
                          ""objectiveAssessmentReference"": {
                            ""assessmentTitle"": ""mCLASS:DIBELS"",
                            ""academicSubjectDescriptor"": ""Reading"",
                            ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                            ""version"": 1,
                            ""identificationCode"": ""Phoneme Segmentation Fluency""
                          },
                          ""performanceLevels"": [
                            {
                              ""performanceLevelDescriptor"": ""Benchmark""
                            }
                          ],
                          ""scoreResults"": [
                            {
                              ""assessmentReportingMethodType"": ""Number score"",
                              ""result"": ""53"",
                              ""resultDatatypeType"": ""Integer""
                            }
                          ]
                        },
                        {
                          ""objectiveAssessmentReference"": {
                            ""assessmentTitle"": ""mCLASS:DIBELS"",
                            ""academicSubjectDescriptor"": ""Reading"",
                            ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                            ""version"": 1,
                            ""identificationCode"": ""Nonsense Word Fluency - CLS""
                          },
                          ""performanceLevels"": [
                            {
                              ""performanceLevelDescriptor"": ""Benchmark""
                            }
                          ],
                          ""scoreResults"": [
                            {
                              ""assessmentReportingMethodType"": ""Number score"",
                              ""result"": ""24"",
                              ""resultDatatypeType"": ""Integer""
                            }
                          ]
                        },
                        {
                          ""objectiveAssessmentReference"": {
                            ""assessmentTitle"": ""mCLASS:DIBELS"",
                            ""academicSubjectDescriptor"": ""Reading"",
                            ""assessedGradeLevelDescriptor"": ""Kindergarten"",
                            ""version"": 1,
                            ""identificationCode"": ""Nonsense Word Fluency - WWR""
                          },
                          ""performanceLevels"": [
                            {
                              ""performanceLevelDescriptor"": ""Not Determined""
                            }
                          ],
                          ""scoreResults"": [
                            {
                              ""assessmentReportingMethodType"": ""Number score"",
                              ""result"": ""8"",
                              ""resultDatatypeType"": ""Integer""
                            }
                          ]
                        }
                      ]
                    }");
        }

        [Test]
        public void ShouldProcessForStudents()
        {
            //Student resources provide a good example where only a few simple properties are required,
            //while many complex properties are optional. By only mapping to the required fields,
            //we demonstrate a realistic "pruning" of the ultimate result POSTed to the ODS.

            var resourceMetadata = Metadata(
                RequiredProperty("studentUniqueId", "string"),
                Property("birthCity", "string"),
                Property("birthCountryDescriptor", "string"),
                RequiredProperty("birthDate", "string"),
                Property("birthInternationalProvince", "string"),
                Property("birthSexDescriptor", "string"),
                Property("birthStateAbbreviationDescriptor", "string"),
                Property("citizenshipStatusDescriptor", "string"),
                Property("dateEnteredUS", "string"),
                RequiredProperty("firstName", "string"),
                Property("generationCodeSuffix", "string"),

                Array("identificationDocuments",
                    Object("studentIdentificationDocument", "edFi_studentIdentificationDocument",
                        RequiredProperty("identificationDocumentUseDescriptor", "string"),
                        RequiredProperty("personalInformationVerificationDescriptor", "string"),
                        Property("issuerCountryDescriptor", "string"),
                        Property("documentExpirationDate", "string"),
                        Property("documentTitle", "string"),
                        Property("issuerDocumentIdentificationCode", "string"),
                        Property("issuerName", "string"))),

                RequiredProperty("lastSurname", "string"),
                Property("maidenName", "string"),
                Property("middleName", "string"),
                Property("multipleBirthStatus", "boolean"),

                Array("otherNames",
                    Object("studentOtherName", "edFi_studentOtherName",
                        RequiredProperty("otherNameTypeDescriptor", "string"),
                        RequiredProperty("firstName", "string"),
                        Property("generationCodeSuffix", "string"),
                        RequiredProperty("lastSurname", "string"),
                        Property("middleName", "string"),
                        Property("personalTitlePrefix", "string"))),

                Array("personalIdentificationDocuments",
                    Object("studentPersonalIdentificationDocument", "edFi_studentPersonalIdentificationDocument",
                        RequiredProperty("identificationDocumentUseDescriptor", "string"),
                        RequiredProperty("personalInformationVerificationDescriptor", "string"),
                        Property("issuerCountryDescriptor", "string"),
                        Property("documentExpirationDate", "string"),
                        Property("documentTitle", "string"),
                        Property("issuerDocumentIdentificationCode", "string"),
                        Property("issuerName", "string"))),

                Property("personalTitlePrefix", "string"),

                Array("visas",
                    Object("studentVisa", "edFi_studentVisa",
                        RequiredProperty("visaDescriptor", "string"))),

                Object("_ext", "studentExtensions",
                    Object("Sample", "sample_studentExtension",
                        Array("pets",
                            Object("studentPet", "sample_studentPet",
                                RequiredProperty("petName", "string"),
                                Property("isFixed", "boolean"))),
                        Object("petPreference", "sample_studentPetPreference",
                            RequiredProperty("maximumWeight", "integer"),
                            RequiredProperty("minimumWeight", "integer"))))
            );

            //Map only top-level required properties.
            var jsonMap = JsonMap(
                MapStatic("studentUniqueId", "123456"),
                MapStatic("birthDate", "2018-01-01"),
                MapStatic("firstName", "First"),
                MapStatic("lastSurname", "Last")
            );

            //All irrelevant properties have been pruned from the mapped row.
            var expected = @"
                {
                    ""studentUniqueId"": ""123456"",
                    ""birthDate"": ""2018-01-01"",
                    ""firstName"": ""First"",
                    ""lastSurname"": ""Last""
                }";

            var lookups = new Lookup[] { };
            var csvRow = new Dictionary<string, string>();

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);
            var result = mapper.ApplyMap(csvRow);

            result.ShouldMatch(expected);
        }

        [Test]
        public void ShouldProcessForStudentAssessments()
        {
            var resourceMetadata = Metadata(

                Object(
                    "assessmentReference",
                    "assessmentReference",
                    Property("identifier", "string"),
                    Property("namespace", "string")),

                Object(
                    "studentReference",
                    "studentReference",
                    Property("studentUniqueId", "string")),

                Property("administrationDate", "string"),

                Property("identifier", "string"),

                Array("performanceLevels",
                    Object(
                        "studentAssessmentPerformanceLevel",
                        "studentAssessmentPerformanceLevel",
                        Property("assessmentReportingMethodType", "string"),
                        Property("performanceLevelDescriptor", "string"),
                        Property("performanceLevelMet", "boolean"))),

                Array("scoreResults",
                    Object(
                        "studentAssessmentPerformanceLevel",
                        "studentAssessmentPerformanceLevel",
                        Property("assessmentReportingMethodType", "string"),
                        Property("resultDatatypeType", "string"),
                        Property("result", "string"))),

                Array("studentObjectiveAssessments",
                    Object(
                        "studentAssessmentStudentObjectiveAssessment",
                        "studentAssessmentStudentObjectiveAssessment",
                        Object(
                            "objectiveAssessmentReference",
                            "objectiveAssessmentReference",
                            Property("assessmentIdentifier", "string"),
                            Property("identificationCode", "string"),
                            Property("namespace", "string")),
                        Array("performanceLevels",
                            Object(
                                "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                Property("assessmentReportingMethodType", "string"),
                                Property("performanceLevelDescriptor", "string"),
                                Property("performanceLevelMet", "boolean"))),
                        Array("scoreResults",
                            Object(
                                "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                Property("assessmentReportingMethodType", "string"),
                                Property("resultDatatypeType", "string"),
                                Property("result", "string")))))
            );

            var jsonMap = JsonMap(

                MapObject(
                    "assessmentReference",
                    MapColumn("identifier", "Overallss_adj", "0"),
                    MapStatic("namespace", "http://example-namespace")),

                MapObject(
                    "studentReference",
                    MapColumn("studentUniqueId", "sasid")),

                MapStatic("administrationDate", "2018-01-01"),

                MapStatic("identifier", "Test Identifier"),

                MapArray("performanceLevels", //Example of an array with two items mapped.
                    MapObject(
                        "studentAssessmentPerformanceLevel",
                        MapStatic("assessmentReportingMethodType", "A"),
                        MapStatic("performanceLevelDescriptor", "B"),
                        MapStatic("performanceLevelMet", "true")),
                    MapObject(
                        "studentAssessmentPerformanceLevel",
                        MapStatic("assessmentReportingMethodType", "D"),
                        MapStatic("performanceLevelDescriptor", "E"),
                        MapStatic("performanceLevelMet", "false"))),

                MapArray("scoreResults"), //Example of an array with zero items mapped.

                MapArray("studentObjectiveAssessments",
                    MapObject(
                        "studentAssessmentStudentObjectiveAssessment",
                        MapObject(
                            "objectiveAssessmentReference",
                            MapStatic("assessmentIdentifier", "G"),
                            MapStatic("identificationCode", "H"),
                            MapStatic("namespace", "I")),
                        MapArray("performanceLevels",
                            MapObject(
                                "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                MapStatic("assessmentReportingMethodType", "J"),
                                MapStatic("performanceLevelDescriptor", "K"),
                                MapStatic("performanceLevelMet", "TRUE"))),
                        MapArray("scoreResults",
                            MapObject(
                                "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                MapStatic("assessmentReportingMethodType", "M"),
                                MapStatic("resultDatatypeType", "N"),
                                MapStatic("result", "O")))))
            );

            var expected = @"
                {
                  ""assessmentReference"": {
                    ""identifier"": ""Overallss_adj Value"",
                    ""namespace"": ""http://example-namespace""
                  },
                  ""studentReference"": {
                    ""studentUniqueId"": ""sasid Value""
                  },
                  ""administrationDate"": ""2018-01-01"",
                  ""identifier"": ""Test Identifier"",
                  ""performanceLevels"": [
                    {
                      ""assessmentReportingMethodType"": ""A"",
                      ""performanceLevelDescriptor"": ""B"",
                      ""performanceLevelMet"": true
                    },
                    {
                      ""assessmentReportingMethodType"": ""D"",
                      ""performanceLevelDescriptor"": ""E"",
                      ""performanceLevelMet"": false
                    }
                  ],
                  ""studentObjectiveAssessments"": [
                    {
                      ""objectiveAssessmentReference"": {
                        ""assessmentIdentifier"": ""G"",
                        ""identificationCode"": ""H"",
                        ""namespace"": ""I""
                      },
                      ""performanceLevels"": [
                        {
                          ""assessmentReportingMethodType"": ""J"",
                          ""performanceLevelDescriptor"": ""K"",
                          ""performanceLevelMet"": true
                        }
                      ],
                      ""scoreResults"": [
                        {
                          ""assessmentReportingMethodType"": ""M"",
                          ""resultDatatypeType"": ""N"",
                          ""result"": ""O""
                        }
                      ]
                    }
                  ]
                }";

            var lookups = new Lookup[] { };

            var csvRow = new Dictionary<string, string>
            {
                { "Overallss_adj", "Overallss_adj Value" },
                { "sasid", "sasid Value" }
            };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);
            var result = mapper.ApplyMap(csvRow);

            result.ShouldMatch(expected);
        }

        [Test]
        public void ShouldPruneUnmappedValuesFromResult()
        {
            // Properties which would be mapped to null, [], or {} should instead simply be omitted from the result.

            var lookups = new Lookup[] { };

            var resourceMetadata = Metadata(
                //Top Level Values
                Property("topLevelString", "string"),
                Property("topLevelDateTime", "date-time"),
                Property("topLevelInteger", "integer"),
                Property("topLevelBoolean", "boolean"),
                Property("topLevelNumber", "number"),

                //Typical Objects
                Object("outerObject", "outerObject",
                    Object("nestedObject", "nestedObject",
                        Property("nestedString", "string"),
                        Property("nestedNumber", "number"),
                        Property("nestedInteger", "integer"))),

                //Typical Arrays
                Array(
                    "arrayProperty",
                    Object("arrayItem", "arrayItemType",
                        Property("itemString", "string"),
                        Property("itemInteger", "integer"))),

                //Typical Arrays With a Required Inner Property
                Array(
                    "arrayWithInnerRequiredProperty",
                    Object("arrayItemWithRequiredInnerProperty", "arrayItemWithRequiredInnerPropertyType",
                        RequiredProperty("itemRequiredString", "string"),
                        Property("itemInteger", "integer"))),

                //Atypical Arrays
                Array(
                    "integerArrayProperty",
                    new ResourceMetadata { Name = "simpleItem", DataType = "integer" }
                ),
                Array(
                    "outerArrayProperty",
                    Array(
                        "innerArrayProperty",
                        Object("innerArrayItem", "innerArrayItemType",
                            Property("itemDateTime", "date-time"),
                            Property("itemBoolean", "boolean"))
                    )
                )
            );

            var jsonMap = JsonMap(
                //Top Level Values
                MapColumn("topLevelString", "TopLevelString"),
                MapColumn("topLevelDateTime", "TopLevelDateTime"),
                MapColumn("topLevelInteger", "TopLevelInteger"),
                MapColumn("topLevelBoolean", "TopLevelBoolean"),
                MapColumn("topLevelNumber", "TopLevelNumber"),

                //Typical Objects
                MapObject("outerObject",
                    MapObject("nestedObject",
                        MapColumn("nestedString", "OuterObject.NestedObject.NestedString"),
                        MapColumn("nestedNumber", "OuterObject.NestedObject.NestedNumber"),
                        MapColumn("nestedInteger", "OuterObject.NestedObject.NestedInteger"))),

                //Typical Arrays
                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem",
                        MapColumn("itemString", "Array[0].ItemString"),
                        MapColumn("itemInteger", "Array[0].ItemInteger")),
                    MapObject("arrayItem",
                        MapColumn("itemString", "Array[1].ItemString"),
                        MapColumn("itemInteger", "Array[1].ItemInteger")),
                    MapObject("arrayItem",
                        MapColumn("itemString", "Array[2].ItemString"),
                        MapColumn("itemInteger", "Array[2].ItemInteger"))),

                //Typical Arrays With a Required Inner Property
                MapArray(
                    "arrayWithInnerRequiredProperty",
                    MapObject("arrayItemWithRequiredInnerProperty",
                        MapColumn("itemRequiredString", "ArrayWithInnerRequiredProperty[0].ItemRequiredString"),
                        MapColumn("itemInteger", "ArrayWithInnerRequiredProperty[0].ItemInteger")),
                    MapObject("arrayItemWithRequiredInnerProperty",
                        MapColumn("itemRequiredString", "ArrayWithInnerRequiredProperty[1].ItemRequiredString"),
                        MapColumn("itemInteger", "ArrayWithInnerRequiredProperty[1].ItemInteger")),
                    MapObject("arrayItemWithRequiredInnerProperty",
                        MapColumn("itemRequiredString", "ArrayWithInnerRequiredProperty[2].ItemRequiredString"),
                        MapColumn("itemInteger", "ArrayWithInnerRequiredProperty[2].ItemInteger"))),

                //Atypical Arrays
                MapArray(
                    "integerArrayProperty",
                    MapColumn("simpleItem", "IntegerArray[0]"),
                    MapColumn("simpleItem", "IntegerArray[1]"),
                    MapColumn("simpleItem", "IntegerArray[2]")),
                MapArray(
                    "outerArrayProperty",
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[0].InnerArray[0].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[0].InnerArray[0].ItemBoolean")
                            ),
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[0].InnerArray[1].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[0].InnerArray[1].ItemBoolean")
                            ),
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[0].InnerArray[2].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[0].InnerArray[2].ItemBoolean")
                            )
                    ),
                    MapArray(
                        "innerArrayProperty",
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[1].InnerArray[0].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[1].InnerArray[0].ItemBoolean")),
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[1].InnerArray[1].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[1].InnerArray[1].ItemBoolean")),
                        MapObject("innerArrayItem",
                            MapColumn("itemDateTime", "OuterArray[1].InnerArray[2].ItemDateTime"),
                            MapColumn("itemBoolean", "OuterArray[1].InnerArray[2].ItemBoolean"))
                    )
                )
            );

            var fullyPopulatedRow = new Dictionary<string, string>
            {
                //Top Level Values
                { "TopLevelString", "Top Level" },
                { "TopLevelDateTime", "2016-08-01" },
                { "TopLevelInteger", "10" },
                { "TopLevelBoolean", "true" },
                { "TopLevelNumber", "1.234" },

                //Typical Objects
                { "OuterObject.NestedObject.NestedString", "Nested Level" },
                { "OuterObject.NestedObject.NestedNumber", "2.345" },
                { "OuterObject.NestedObject.NestedInteger", "20" },

                //Typical Arrays
                { "Array[0].ItemString", "[0]" },
                { "Array[0].ItemInteger", "30" },
                { "Array[1].ItemString", "[1]" },
                { "Array[1].ItemInteger", "31" },
                { "Array[2].ItemString", "[2]" },
                { "Array[2].ItemInteger", "32" },

                //Typical Arrays With a Required Inner Property
                { "ArrayWithInnerRequiredProperty[0].ItemRequiredString", "[0]" },
                { "ArrayWithInnerRequiredProperty[0].ItemInteger", "30" },
                { "ArrayWithInnerRequiredProperty[1].ItemRequiredString", "[1]" },
                { "ArrayWithInnerRequiredProperty[1].ItemInteger", "31" },
                { "ArrayWithInnerRequiredProperty[2].ItemRequiredString", "[2]" },
                { "ArrayWithInnerRequiredProperty[2].ItemInteger", "32" },

                //Atypical Arrays
                { "IntegerArray[0]", "40" },
                { "IntegerArray[1]", "41" },
                { "IntegerArray[2]", "42" },
                { "OuterArray[0].InnerArray[0].ItemDateTime", "2016-05-01" },
                { "OuterArray[0].InnerArray[0].ItemBoolean", "true" },
                { "OuterArray[0].InnerArray[1].ItemDateTime", "2016-05-02" },
                { "OuterArray[0].InnerArray[1].ItemBoolean", "false" },
                { "OuterArray[0].InnerArray[2].ItemDateTime", "2016-05-03" },
                { "OuterArray[0].InnerArray[2].ItemBoolean", "true" },
                { "OuterArray[1].InnerArray[0].ItemDateTime", "2016-06-01" },
                { "OuterArray[1].InnerArray[0].ItemBoolean", "false" },
                { "OuterArray[1].InnerArray[1].ItemDateTime", "2016-06-02" },
                { "OuterArray[1].InnerArray[1].ItemBoolean", "true" },
                { "OuterArray[1].InnerArray[2].ItemDateTime", "2016-06-03" },
                { "OuterArray[1].InnerArray[2].ItemBoolean", "false" }
            };

            var partiallyPopulatedRow = new Dictionary<string, string>
            {
                //Many items are blank, but the non-blank items should provide enough
                //information to keep all the same *depth levels* as the fully-populated
                //row. This demonstrates, especially, how partially mapped arrays behave:
                //examples here include a missing first, last, and interior item.

                //Top Level Values
                { "TopLevelString", "Top Level" },
                { "TopLevelDateTime", "" },
                { "TopLevelInteger", "" },
                { "TopLevelBoolean", "true" },
                { "TopLevelNumber", "" },

                //Typical Objects
                { "OuterObject.NestedObject.NestedString", "Nested Level" },
                { "OuterObject.NestedObject.NestedNumber", "" },
                { "OuterObject.NestedObject.NestedInteger", "" },

                //Typical Arrays
                { "Array[0].ItemString", "" },
                { "Array[0].ItemInteger", "30" },
                { "Array[1].ItemString", "" },
                { "Array[1].ItemInteger", "" },
                { "Array[2].ItemString", "[2]" },
                { "Array[2].ItemInteger", "" },

                //Typical Arrays With a Required Inner Property
                { "ArrayWithInnerRequiredProperty[0].ItemRequiredString", "" },
                { "ArrayWithInnerRequiredProperty[0].ItemInteger", "30" },
                { "ArrayWithInnerRequiredProperty[1].ItemRequiredString", "" },
                { "ArrayWithInnerRequiredProperty[1].ItemInteger", "" },
                { "ArrayWithInnerRequiredProperty[2].ItemRequiredString", "[2]" },
                { "ArrayWithInnerRequiredProperty[2].ItemInteger", "" },

                //Atypical Arrays: First or Last Item Missing
                { "IntegerArray[0]", "" },
                { "IntegerArray[1]", "41" },
                { "IntegerArray[2]", "42" },
                { "OuterArray[0].InnerArray[0].ItemDateTime", "" },
                { "OuterArray[0].InnerArray[0].ItemBoolean", "" },
                { "OuterArray[0].InnerArray[1].ItemDateTime", "" },
                { "OuterArray[0].InnerArray[1].ItemBoolean", "" },
                { "OuterArray[0].InnerArray[2].ItemDateTime", "2016-05-03" },
                { "OuterArray[0].InnerArray[2].ItemBoolean", "true" },
                { "OuterArray[1].InnerArray[0].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[0].ItemBoolean", "" },
                { "OuterArray[1].InnerArray[1].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[1].ItemBoolean", "" },
                { "OuterArray[1].InnerArray[2].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[2].ItemBoolean", "" }
            };

            var emptyRow = new Dictionary<string, string>
            {
                //Top Level Values
                { "TopLevelString", "" },
                { "TopLevelDateTime", "" },
                { "TopLevelInteger", "" },
                { "TopLevelBoolean", "" },
                { "TopLevelNumber", "" },

                //Typical Objects
                { "OuterObject.NestedObject.NestedString", "" },
                { "OuterObject.NestedObject.NestedNumber", "" },
                { "OuterObject.NestedObject.NestedInteger", "" },

                //Typical Arrays
                { "Array[0].ItemString", "" },
                { "Array[0].ItemInteger", "" },
                { "Array[1].ItemString", "" },
                { "Array[1].ItemInteger", "" },
                { "Array[2].ItemString", "" },
                { "Array[2].ItemInteger", "" },

                //Typical Arrays With a Required Inner Property
                { "ArrayWithInnerRequiredProperty[0].ItemRequiredString", "" },
                { "ArrayWithInnerRequiredProperty[0].ItemInteger", "" },
                { "ArrayWithInnerRequiredProperty[1].ItemRequiredString", "" },
                { "ArrayWithInnerRequiredProperty[1].ItemInteger", "" },
                { "ArrayWithInnerRequiredProperty[2].ItemRequiredString", "" },
                { "ArrayWithInnerRequiredProperty[2].ItemInteger", "" },

                //Atypical Arrays
                { "IntegerArray[0]", "" },
                { "IntegerArray[1]", "" },
                { "IntegerArray[2]", "" },
                { "OuterArray[0].InnerArray[0].ItemDateTime", "" },
                { "OuterArray[0].InnerArray[0].ItemBoolean", "" },
                { "OuterArray[0].InnerArray[1].ItemDateTime", "" },
                { "OuterArray[0].InnerArray[1].ItemBoolean", "" },
                { "OuterArray[0].InnerArray[2].ItemDateTime", "" },
                { "OuterArray[0].InnerArray[2].ItemBoolean", "" },
                { "OuterArray[1].InnerArray[0].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[0].ItemBoolean", "" },
                { "OuterArray[1].InnerArray[1].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[1].ItemBoolean", "" },
                { "OuterArray[1].InnerArray[2].ItemDateTime", "" },
                { "OuterArray[1].InnerArray[2].ItemBoolean", "" }
            };

            var mapper = ResourceMapper(resourceMetadata, jsonMap, lookups);

            mapper.ApplyMap(fullyPopulatedRow).ShouldMatch(
                @"{
                    ""topLevelString"": ""Top Level"",
                    ""topLevelDateTime"": ""2016-08-01"",
                    ""topLevelInteger"": 10,
                    ""topLevelBoolean"": true,
                    ""topLevelNumber"": 1.234,

                    ""outerObject"": {
                        ""nestedObject"": {
                            ""nestedString"": ""Nested Level"",
                            ""nestedNumber"": 2.345,
                            ""nestedInteger"": 20
                        },
                    },

                    ""arrayProperty"": [
                        {
                            ""itemString"": ""[0]"",
                            ""itemInteger"": 30
                        },
                        {
                            ""itemString"": ""[1]"",
                            ""itemInteger"": 31
                        },
                        {
                            ""itemString"": ""[2]"",
                            ""itemInteger"": 32
                        }
                    ],

                    ""arrayWithInnerRequiredProperty"": [
                        {
                            ""itemRequiredString"": ""[0]"",
                            ""itemInteger"": 30
                        },
                        {
                            ""itemRequiredString"": ""[1]"",
                            ""itemInteger"": 31
                        },
                        {
                            ""itemRequiredString"": ""[2]"",
                            ""itemInteger"": 32
                        }
                    ],

                    ""integerArrayProperty"": [
                        40,
                        41,
                        42
                    ],
                    ""outerArrayProperty"": [
                        [
                            {
                                ""itemDateTime"": ""2016-05-01"",
                                ""itemBoolean"": true
                            },
                            {
                                ""itemDateTime"": ""2016-05-02"",
                                ""itemBoolean"": false
                            },
                            {
                                ""itemDateTime"": ""2016-05-03"",
                                ""itemBoolean"": true
                            }
                        ],
                        [
                            {
                                ""itemDateTime"": ""2016-06-01"",
                                ""itemBoolean"": false
                            },
                            {
                                ""itemDateTime"": ""2016-06-02"",
                                ""itemBoolean"": true
                            },
                            {
                                ""itemDateTime"": ""2016-06-03"",
                                ""itemBoolean"": false
                            }
                        ]
                    ]
                }");

            mapper.ApplyMap(partiallyPopulatedRow).ShouldMatch(
                @"{
                    ""topLevelString"": ""Top Level"",
                    ""topLevelBoolean"": true,

                    ""outerObject"": {
                        ""nestedObject"": {
                            ""nestedString"": ""Nested Level""
                        },
                    },

                    ""arrayProperty"": [
                        {
                            ""itemInteger"": 30
                        },
                        {
                            ""itemString"": ""[2]""
                        }
                    ],

                   ""arrayWithInnerRequiredProperty"": [
                        {
                            ""itemRequiredString"": ""[2]""
                        }
                    ],

                    ""integerArrayProperty"": [
                        41,
                        42
                    ],
                    ""outerArrayProperty"": [
                        [
                            {
                                ""itemDateTime"": ""2016-05-03"",
                                ""itemBoolean"": true
                            }
                        ]
                    ]
                }");

            mapper.ApplyMap(emptyRow).ShouldMatch("{}"); //No actual values were mapped, so the entire tree was pruned.
        }

        private static ResourceMetadata[] Metadata(params ResourceMetadata[] metadata)
        {
            return metadata;
        }

        private static DataMapper[] JsonMap(params DataMapper[] dataMappers)
        {
            return dataMappers;
        }

        private static ResourceMapper ResourceMapper(ResourceMetadata[] resourceMetadata, DataMapper[] mappings, Lookup[] lookups)
        {
            return new ResourceMapper(A.Fake<ILogger<ResourceMapper>>(), resourceMetadata, mappings, new LookupCollection(lookups));
        }
    }
}
