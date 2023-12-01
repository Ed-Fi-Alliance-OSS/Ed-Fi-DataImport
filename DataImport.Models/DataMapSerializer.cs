// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataImport.Models
{
    // IMPORTANT
    //
    // DO NOT make breaking changes to the JSON Map format,
    // as those affect previously-saved mappings created
    // both within Data Import and those created/shared
    // externally by third parties.

    public class DataMapSerializer
    {
        private readonly string _resourcePath;
        private readonly ResourceMetadata[] _resourceMetadatas;

        public DataMapSerializer(Resource resource)
            : this(resource.Path, ResourceMetadata.DeserializeFrom(resource))
        {
        }

        public DataMapSerializer(DataMap dataMap)
            : this(dataMap.ResourcePath, ResourceMetadata.DeserializeFrom(dataMap))
        {
        }

        public DataMapSerializer(string resourcePath, ResourceMetadata[] resourceMetadatas)
        {
            _resourcePath = resourcePath;
            _resourceMetadatas = resourceMetadatas;
        }

        public string Serialize(DataMapper[] mappings, bool isDeleteByIdOperation)
        {
            return isDeleteByIdOperation
                ? SerializeObjectForDeleteById(mappings.Single()).ToString(Formatting.Indented)
                : SerializeObject(_resourceMetadatas, mappings).ToString(Formatting.Indented);
        }

        public DataMapper[] Deserialize(string jsonMap, bool isDeleteByIdOperation)
        {
            JObject jobject;
            try
            {
                jobject = JObject.Parse(jsonMap);
            }
            catch (Exception exception)
            {
                throw new ArgumentException(
                    "Cannot deserialize mappings from JSON, because the map text is not a valid JSON object. " +
                    "Check the inner exception for details. Invalid JSON Map text:" +
                    $"{Environment.NewLine}{Environment.NewLine}{jsonMap}"
                    , exception);
            }

            return Deserialize(jobject, isDeleteByIdOperation);
        }

        public DataMapper[] Deserialize(JObject jsonMap, bool isDeleteByIdOperation)
        {
            return isDeleteByIdOperation
                ? DeserializeObjectForDeleteById(jsonMap).ToArray()
                : DeserializeObject(_resourceMetadatas, jsonMap).ToArray();
        }

        private JObject SerializeObject(IReadOnlyList<ResourceMetadata> nodeMetadatas, IReadOnlyList<DataMapper> nodes)
        {
            var result = new JObject();

            foreach (var node in nodes)
            {
                var nodeMetadata = nodeMetadatas.SingleOrDefault(m => m.Name == node.Name);

                if (nodeMetadata == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot serialize mappings to JSON, because the key '{node.Name}' should not exist " +
                        $"according to the metadata for resource '{_resourcePath}'.");
                }

                if (node.ContainsAnyMappedProperty())
                {
                    var hasMappedValue = node.SourceColumn != null || node.Value != null;

                    if (nodeMetadata.DataType == "array")
                    {
                        if (hasMappedValue)
                            throw new InvalidOperationException(
                                "Cannot serialize mappings to JSON, because an array " +
                                $"literal was expected for key '{node.Name}', " +
                                "but instead it is being mapped to a single value.");

                        var arrayItemMetadata = nodeMetadata.Children.Single();

                        result.Add(new JProperty(nodeMetadata.Name, SerializeArray(arrayItemMetadata, node.Children)));
                    }
                    else if (nodeMetadata.Children.Any())
                    {
                        if (hasMappedValue)
                            throw new InvalidOperationException(
                                "Cannot serialize mappings to JSON, because an object " +
                                $"literal was expected for key '{node.Name}', " +
                                "but instead it is being mapped to a single value.");

                        result.Add(new JProperty(nodeMetadata.Name, SerializeObject(nodeMetadata.Children, node.Children)));
                    }
                    else
                    {
                        if (node.Children.Any())
                            throw new InvalidOperationException(
                                "Cannot serialize mappings to JSON, because a single " +
                                $"value was expected for key '{node.Name}', " +
                                "but instead it is being mapped to a complex value.");

                        result.Add(new JProperty(nodeMetadata.Name, SerializeMappedValue(nodeMetadata, node)));
                    }
                }
            }

            return result;
        }

        private JObject SerializeObjectForDeleteById(DataMapper node)
        {
            var result = new JObject { new JProperty("Id", new JObject { new JProperty("Column", node.SourceColumn) }) };
            return result;
        }

        private JArray SerializeArray(ResourceMetadata arrayItemMetadata, IReadOnlyList<DataMapper> nodes)
        {
            var result = new JArray();

            foreach (var node in nodes)
            {
                if (node.ContainsAnyMappedProperty())
                {
                    if (arrayItemMetadata.DataType == "array")
                    {
                        var nestedArrayItemMetadata = arrayItemMetadata.Children.Single();
                        result.Add(SerializeArray(nestedArrayItemMetadata, node.Children));
                    }
                    else if (arrayItemMetadata.Children.Any())
                    {
                        result.Add(SerializeObject(arrayItemMetadata.Children, node.Children));
                    }
                    else
                    {
                        result.Add(SerializeMappedValue(arrayItemMetadata, node));
                    }
                }
            }

            return result;
        }

        private List<DataMapper> DeserializeObject(IReadOnlyList<ResourceMetadata> nodeMetadatas, JToken objectToken)
        {
            var jobject = objectToken as JObject;

            if (jobject == null)
                throw new InvalidOperationException(
                    "Cannot deserialize mappings from JSON, because an object literal was expected. " +
                    "Instead, found: " +
                    $"{objectToken.ToString(Formatting.Indented)}");

            var result = new List<DataMapper>();

            var nodes = jobject.Children().Cast<JProperty>().ToArray();

            foreach (var node in nodes)
            {
                if (node.Name != "Id" && nodeMetadatas.All(x => x.Name != node.Name))
                    throw new InvalidOperationException(
                        $"Cannot deserialize mappings from JSON, because the key '{node.Name}' should not exist " +
                        $"according to the metadata for resource '{_resourcePath}'.");
            }

            foreach (var nodeMetadata in nodeMetadatas)
            {
                var node = nodes.SingleOrDefault(n => n.Name == nodeMetadata.Name);

                if (node == null)
                {
                    result.Add(nodeMetadata.BuildInitialMappings());
                }
                else
                {
                    var propertyValue = node.Children().Single();

                    if (nodeMetadata.DataType == "array")
                    {
                        var arrayItemMetadata = nodeMetadata.Children.Single();
                        result.Add(new DataMapper
                        {
                            Name = nodeMetadata.Name,
                            Children = DeserializeArray(arrayItemMetadata, propertyValue)
                        });
                    }
                    else if (nodeMetadata.Children.Any())
                    {
                        result.Add(new DataMapper
                        {
                            Name = nodeMetadata.Name,
                            Children = DeserializeObject(nodeMetadata.Children, propertyValue)
                        });
                    }
                    else
                    {
                        result.Add(DeserializeMappedValue(nodeMetadata, propertyValue));
                    }
                }
            }

            return result;
        }

        private List<DataMapper> DeserializeObjectForDeleteById(JToken objectToken)
        {
            var jobject = objectToken as JObject;

            if (jobject == null)
                throw new InvalidOperationException(
                    "Cannot deserialize mappings from JSON, because an object literal was expected. " +
                    "Instead, found: " +
                    $"{objectToken.ToString(Formatting.Indented)}");

            var result = new List<DataMapper>();

            var nodes = jobject.Children().Cast<JProperty>().ToArray();

            var node = nodes.Single(n => n.Name == "Id");

            var propertyValue = node.Children().Single();

            var sourceColumn = ((JObject) propertyValue).Children().Cast<JProperty>().Single().Value;

            result.Add(new DataMapper() { Name = "Id", SourceColumn = DeserializeRawValue(sourceColumn) });

            return result;
        }

        private List<DataMapper> DeserializeArray(ResourceMetadata arrayItemMetadata, JToken arrayToken)
        {
            var array = arrayToken as JArray;

            if (array == null)
                throw new InvalidOperationException(
                    "Cannot deserialize mappings from JSON, because an array literal was expected. " +
                    "Instead, found: " +
                    $"{arrayToken.ToString(Formatting.Indented)}");

            var nodes = array.Children().ToArray();

            var result = new List<DataMapper>();

            foreach (var node in nodes)
            {
                if (arrayItemMetadata.DataType == "array")
                {
                    var nestedArrayItemMetadata = arrayItemMetadata.Children.Single();
                    result.Add(new DataMapper
                    {
                        Name = arrayItemMetadata.Name,
                        Children = DeserializeArray(nestedArrayItemMetadata, node)
                    });
                }
                else if (arrayItemMetadata.Children.Any())
                {
                    result.Add(new DataMapper
                    {
                        Name = arrayItemMetadata.Name,
                        Children = DeserializeObject(arrayItemMetadata.Children, node)
                    });
                }
                else
                {
                    result.Add(DeserializeMappedValue(arrayItemMetadata, node));
                }
            }

            return result;
        }

        private DataMapper DeserializeMappedValue(ResourceMetadata metadata, JToken token)
        {
            if (token is JValue)
            {
                var staticValue = ((JValue) token).Value;

                return new DataMapper
                {
                    Name = metadata.Name,
                    Value = DeserializeRawValue(staticValue)
                };
            }

            if (token is JObject)
            {
                var columnSource = (JObject) token;
                return DeserializeColumnSource(metadata, columnSource);
            }

            throw new InvalidOperationException(
                $"Cannot deserialize mappings from JSON, because the key '{metadata.Name}' " +
                $"was expected to have a {metadata.DataType} value. Instead, the value was: " +
                $"{token.ToString(Formatting.Indented)}");
        }

        private object SerializeMappedValue(ResourceMetadata nodeMetadata, DataMapper node)
        {
            if (node.Value != null)
            {
                if (node.SourceColumn != null || node.SourceTable != null || node.Default != null)
                {
                    throw new InvalidOperationException(
                        $"Cannot serialize mappings to JSON, because the key '{node.Name}' has an ambiguous " +
                        $"mapping definition: {JsonConvert.SerializeObject(node, Formatting.Indented)}");
                }

                return SerializeRawValue(nodeMetadata.DataType, node.Value);
            }

            return SerializeColumnSource(nodeMetadata, node);
        }

        private static object SerializeColumnSource(ResourceMetadata nodeMetadata, DataMapper node)
        {
            var result = new JObject();

            if (node.SourceColumn != null) result.Add(new JProperty("Column", node.SourceColumn));
            if (node.SourceTable != null) result.Add(new JProperty("Lookup", node.SourceTable));
            if (node.Default != null) result.Add(new JProperty("Default", SerializeRawValue(nodeMetadata.DataType, node.Default)));

            return result;
        }

        private DataMapper DeserializeColumnSource(ResourceMetadata nodeMetadata, JObject columnSource)
        {
            var keys = columnSource.Children().Cast<JProperty>().Select(x => x.Name).ToArray();

            if (!keys.Contains("Column"))
            {
                throw new InvalidOperationException(
                    $"Cannot deserialize mappings from JSON, because the key '{nodeMetadata.Name}' was " +
                    "expected to have a Column Source declaration as its value, indicating the source column. " +
                    $"Instead, the value was: {columnSource.ToString(Formatting.Indented)}");
            }

            var allowedKeys = new[] { "Column", "Lookup", "Default" };
            foreach (var key in keys)
            {
                if (!allowedKeys.Contains(key))
                {
                    throw new InvalidOperationException(
                        $"Cannot deserialize mappings from JSON, because the key '{nodeMetadata.Name}' was " +
                        "expected to have a Column Source declaration as its value. Instead, the value " +
                        $"contains unexpected property '{key}': {columnSource.ToString(Formatting.Indented)}");
                }
            }

            var nonStringLiterals = new List<string>();
            foreach (var property in columnSource.Children().Cast<JProperty>())
            {
                if (property.Name != "Default")
                {
                    bool isStringLiteral = (property.Value as JValue)?.Value is string;

                    if (!isStringLiteral)
                        nonStringLiterals.Add(property.Name);
                }
            }

            if (nonStringLiterals.Any())
                throw new InvalidOperationException(
                    $"Cannot deserialize mappings from JSON, because the key '{nodeMetadata.Name}' was " +
                    "expected to have a valid Column Source declaration as its value. It has a Column Source, " +
                    $"but one with invalid content. {string.Join(", ", nonStringLiterals.Select(x => $"'{x}'"))} " +
                    $"should be strings: {columnSource.ToString(Formatting.Indented)}");

            var invalidDefault = new List<string>();
            foreach (var property in columnSource.Children().Cast<JProperty>())
            {
                if (property.Name == "Default")
                {
                    bool isValueLiteral = property.Value is JValue;

                    if (!isValueLiteral)
                        invalidDefault.Add(property.Name);
                }
            }

            if (invalidDefault.Any())
                throw new InvalidOperationException(
                    $"Cannot deserialize mappings from JSON, because the key '{nodeMetadata.Name}' was " +
                    "expected to have a valid Column Source declaration as its value. It has a Column Source, " +
                    $"but one with an invalid default. 'Default' should be a single value: {columnSource.ToString(Formatting.Indented)}");

            var @default = columnSource["Default"]?.Type == JTokenType.Boolean
                ? DeserializeRawValue(columnSource.Value<bool>("Default"))
                : DeserializeRawValue(columnSource.Value<string>("Default"));

            return new DataMapper
            {
                Name = nodeMetadata.Name,
                SourceColumn = columnSource.Value<string>("Column"),
                SourceTable = columnSource.Value<string>("Lookup"),
                Default = @default
            };
        }

        private static object SerializeRawValue(string dataType, string nodeValue)
        {
            if (dataType == "integer")
            {
                int parsed;
                if (int.TryParse(nodeValue, out parsed))
                    return parsed;
            }

            if (dataType == "boolean")
            {
                bool parsed;
                if (bool.TryParse(nodeValue, out parsed))
                    return parsed;
            }

            if (dataType == "number")
            {
                decimal parsed;
                if (decimal.TryParse(nodeValue, out parsed))
                    if (DeserializeRawValue(parsed) == nodeValue.Trim())
                        return parsed;
            }

            return nodeValue.Trim();
        }

        private static string DeserializeRawValue(object rawValue)
        {
            if (rawValue == null)
                return null;

            if (rawValue is bool)
                return rawValue.ToString().ToLower();

            return rawValue.ToString();
        }
    }
}
