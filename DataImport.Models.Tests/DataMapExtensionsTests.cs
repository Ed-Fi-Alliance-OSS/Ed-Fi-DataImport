// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.TestHelpers;
using NUnit.Framework;
using static DataImport.TestHelpers.ResourceMetadataBuilder;
using static DataImport.TestHelpers.DataMapperBuilder;

namespace DataImport.Models.Tests
{
    public class DataMapExtensionsTests
    {
        private DataMap _dataMap;

        [OneTimeSetUp]
        public void SetUp()
        {
            var resourceMetadata = new[]
            {
                Property("propertyA", "string"),
                Property("propertyB", "string"),
                Property("propertyC", "string"),
                Property("propertyD", "string"),

                Object("complexProperty", "complexPropertyType",
                    Property("propertyE", "string"),
                    Property("propertyF", "string")),

                Array(
                    "arrayProperty",
                    Object(
                        "arrayItem",
                        "arrayItemType",
                        Property("propertyG", "string")))
            };

            var mappings = new[]
            {
                Unmapped("propertyA"),
                MapStatic("propertyB", "Static Value"),
                MapColumn("propertyC", "Column1"),
                MapLookup("propertyD", "Column2", "Lookup3"),

                MapObject("complexProperty",
                    MapLookup("propertyE", "Column3", "Lookup3"),
                    MapColumn("propertyF", "Column4")),

                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapLookup("propertyG", "Column3", "Lookup2")),
                    MapObject("arrayItem", MapLookup("propertyG", "Column5", "Lookup1"))),
            };

            _dataMap = new DataMap
            {
                Name = "Test Resource Map",
                ResourcePath = "testResource",
                Metadata = ResourceMetadata.Serialize(resourceMetadata)
            };

            var dataMapSerializer = new DataMapSerializer(_dataMap);
            _dataMap.Map = dataMapSerializer.Serialize(mappings);
        }

        [Test]
        public void ShouldDetermineDistinctReferencedLookups()
        {
            _dataMap.ReferencedLookups().ShouldMatch("Lookup1", "Lookup2", "Lookup3");
        }

        [Test]
        public void ShouldDetermineDistinctReferencedColumns()
        {
            _dataMap.ReferencedColumns().ShouldMatch("Column1", "Column2", "Column3", "Column4", "Column5");
        }
    }
}