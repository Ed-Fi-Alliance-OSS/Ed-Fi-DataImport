// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using NUnit.Framework;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Shared
{
    /// <summary>
    /// These are integration tests exercising aspects of the Data Map JSON format,
    /// rather than tests of a specific class.
    /// </summary>
    public class JsonMapFormatTests
    {
        [Test]
        public async Task TestJsonMapFormatAgainstOdsApiV25()
        {
            await ConfigureForOdsApiV25();

            AllResourcesCanSerializeAndDeserializeWithoutDataLoss();
        }

        [Test]
        public async Task TestJsonMapFormatAgainstOdsApiV311()
        {
            await ConfigureForOdsApiV311();

            AllResourcesCanSerializeAndDeserializeWithoutDataLoss();
        }

        private static void AllResourcesCanSerializeAndDeserializeWithoutDataLoss()
        {
            var resources = Query(db => db.Resources.ToArray());

            foreach (var resource in resources)
            {
                var resourceMetadatas = ResourceMetadata.DeserializeFrom(resource);
                var originalMappings = MapToAllProperties(resourceMetadatas).ToArray();

                var dataMapSerializer = new DataMapSerializer(resource);

                var serializedToJsonMap = dataMapSerializer.Serialize(originalMappings, false);
                var deserializedFromJsonMap = dataMapSerializer.Deserialize(serializedToJsonMap, false);

                // Serializing and deserializing should be an identity operation.
                deserializedFromJsonMap.ShouldMatch(originalMappings);
            }
        }

        private static IEnumerable<DataMapper> MapToAllProperties(IReadOnlyList<ResourceMetadata> resourceMetadatas)
        {
            // Create a sample mapping in which all properties are specified, and all arrays have two items,
            // so that we can easily build up a mapping that realistically hits all properties at all depth levels.

            for (var i = 0; i < resourceMetadatas.Count; i++)
            {
                var resourceMetadata = resourceMetadatas[i];

                if (resourceMetadata.DataType == "array")
                {
                    var arrayItemMetadata = resourceMetadata.Children.Single();

                    yield return new DataMapper
                    {
                        Name = resourceMetadata.Name,
                        Children = MapToAllProperties(new[]
                        {
                            arrayItemMetadata,
                            arrayItemMetadata
                        }).ToList()
                    };
                }
                else if (resourceMetadata.Children.Any())
                {
                    yield return new DataMapper
                    {
                        Name = resourceMetadata.Name,
                        Children = MapToAllProperties(resourceMetadata.Children).ToList()
                    };
                }
                else
                {
                    var coinFlip = i % 2 == 0;
                    if (coinFlip)
                    {
                        yield return new DataMapper
                        {
                            Name = resourceMetadata.Name,
                            SourceColumn = "Column" + i,
                            SourceTable = "LookupTable",
                            Default = "X",
                            Children = MapToAllProperties(resourceMetadata.Children).ToList()
                        };
                    }
                    else
                    {
                        yield return new DataMapper
                        {
                            Name = resourceMetadata.Name,
                            Value = "S",
                            Children = MapToAllProperties(resourceMetadata.Children).ToList()
                        };
                    }
                }
            }
        }
    }
}
