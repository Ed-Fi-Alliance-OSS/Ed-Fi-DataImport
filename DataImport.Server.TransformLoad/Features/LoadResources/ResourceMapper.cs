// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class ResourceMapper
    {
        private readonly ILogger _logger;
        private readonly ResourceMetadata[] _resourceMetadata;
        private readonly DataMapper[] _mappings;
        private readonly LookupCollection _mappingLookups;

        public ResourceMapper(ILogger logger, DataMap dataMap, LookupCollection mappingLookups)
        {
            _logger = logger;

            var dataMapSerializer = new DataMapSerializer(dataMap);
            _mappings = dataMapSerializer.Deserialize(dataMap.Map, dataMap.IsDeleteOperation);
            _resourceMetadata = ResourceMetadata.DeserializeFrom(dataMap);
            _mappingLookups = mappingLookups;
        }

        public ResourceMapper(ILogger<ResourceMapper> logger, ResourceMetadata[] resourceMetadata, DataMapper[] mappings, LookupCollection mappingLookups)
        {
            _logger = logger;
            _mappings = mappings;
            _resourceMetadata = resourceMetadata;
            _mappingLookups = mappingLookups;
        }

        public JToken ApplyMap(Dictionary<string, string> csvRow)
        {
            var safeCsvRow = new CsvRow(csvRow);

            return MapToJsonObject(_resourceMetadata, _mappings, safeCsvRow);
        }

        public JToken ApplyMapForDeleteByIdOperation(Dictionary<string, string> csvRow)
        {
            var safeCsvRow = new CsvRow(csvRow);

            return MapToJsonObjectForDeleteById(_mappings, safeCsvRow);
        }

        private JObject MapToJsonObject(IReadOnlyList<ResourceMetadata> nodeMetadatas, IReadOnlyList<DataMapper> nodes, CsvRow csvRow)
        {
            var result = new JObject();

            foreach (var node in nodes)
            {
                var nodeMetadata = nodeMetadatas.Single(m => m.Name == node.Name);

                if (nodeMetadata.DataType == "array")
                {
                    var arrayItemMetadata = nodeMetadata.Children.Single();

                    var jsonArray = MapToJsonArray(arrayItemMetadata, node.Children, csvRow);

                    if (jsonArray.HasValues)
                        result.Add(new JProperty(nodeMetadata.Name, jsonArray));
                }
                else if (nodeMetadata.Children.Any())
                {
                    var jsonObject = MapToJsonObject(nodeMetadata.Children, node.Children, csvRow);

                    if (jsonObject.HasValues)
                        result.Add(new JProperty(nodeMetadata.Name, jsonObject));
                }
                else
                {
                    var value = MapToValue(nodeMetadata, node, csvRow);

                    if (value != null)
                        result.Add(new JProperty(nodeMetadata.Name, value));
                }
            }

            return result;
        }

        private JObject MapToJsonObjectForDeleteById(IReadOnlyList<DataMapper> nodes, CsvRow csvRow)
        {
            var result = new JObject();

            foreach (var node in nodes)
            {
                var rawValue = RawValue(node, csvRow);

                result.Add(new JProperty("Id", rawValue));
            }

            return result;
        }
        
        private JArray MapToJsonArray(ResourceMetadata arrayItemMetadata, IReadOnlyList<DataMapper> nodes, CsvRow csvRow)
        {
            var result = new JArray();

            foreach (var node in nodes)
            {
                if (arrayItemMetadata.DataType == "array")
                {
                    var nestedArrayItemMetadata = arrayItemMetadata.Children.Single();

                    var jsonArray = MapToJsonArray(nestedArrayItemMetadata, node.Children, csvRow);

                    if (jsonArray.HasValues)
                        result.Add(jsonArray);
                }
                else if (arrayItemMetadata.Children.Any())
                {
                    var jsonObject = MapToJsonObject(arrayItemMetadata.Children, node.Children, csvRow);

                    if (jsonObject.HasValues)
                    {
                        // Real-world use cases involve mapping "jagged" CSV data to ODS arrays-of-objects.
                        // For instance, student assessment CSV data may have columns for each of many
                        // specific types of scores, each of which appears as an array element.
                        // Such a CSV may have blanks for some of these score cells, indicating that
                        // the array element is irrelevant for that CSV row.
                        //
                        // Blanks in these CSV cells result in an *incomplete* jsonObject here.
                        // These are non-empty potential array elements that, nevertheless, need to be omitted
                        // here from the array because they correspond with blank cells.
                        //
                        // Specifically, the heuristic is to omit an array element if any of its required
                        // properties are missing here in jsonObject. Otherwise, the array element would
                        // of course be rejected by the ODS as incomplete.
                        //
                        // Because it is conceivable that such an item rejection is a mistake, such as
                        // when a *poorly-defined* map results in omitting data, we log all such exclusions.

                        var requiredPropertiesForArrayItemInclusion =
                            arrayItemMetadata.Children.Where(x => x.Required).Select(x => x.Name);

                        var actualProperties = jsonObject.Children<JProperty>().Select(x => x.Name);

                        bool include = requiredPropertiesForArrayItemInclusion.All(x => actualProperties.Contains(x));

                        if (include)
                        {
                            result.Add(jsonObject);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Discarding array item '{item}' because one of its required properties was blank. " +
                                "This can happen when an input file with 'jagged' data is mapped to an ODS array property.", arrayItemMetadata.Name);
                        }
                    }
                }
                else
                {
                    var value = MapToValue(arrayItemMetadata, node, csvRow);

                    if (value != null)
                        result.Add(value);
                }
            }

            return result;
        }

        private object MapToValue(ResourceMetadata nodeMetadata, DataMapper node, CsvRow csvRow)
        {
            var rawValue = RawValue(node, csvRow);

            return ConvertDataType(nodeMetadata, node, rawValue);
        }

        private string RawValue(DataMapper node, CsvRow csvRow)
        {
            if (!string.IsNullOrWhiteSpace(node.SourceColumn))
            {
                var valueFromCsv = csvRow[node.SourceColumn];

                string rawValue;

                if (string.IsNullOrWhiteSpace(valueFromCsv))
                {
                    rawValue = null;
                }
                else if (!string.IsNullOrWhiteSpace(node.SourceTable))
                {
                    rawValue = GetValueFromLookupTable(valueFromCsv, node.SourceColumn, node.SourceTable);
                }
                else
                {
                    rawValue = valueFromCsv;
                }

                if (string.IsNullOrWhiteSpace(rawValue) && !string.IsNullOrWhiteSpace(node.Default))
                    rawValue = node.Default;

                return rawValue;
            }

            if (!string.IsNullOrWhiteSpace(node.Value))
                return node.Value;

            return null;
        }

        private string GetValueFromLookupTable(string valueFromCsv, string sourceColumn, string sourceTable)
        {
            if (_mappingLookups.TryLookup(sourceTable, valueFromCsv, out string value))
                return value;

            throw new MissingLookupKeyException(sourceColumn, sourceTable);
        }

        private static object ConvertDataType(ResourceMetadata nodeMetadata, DataMapper node, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            switch (nodeMetadata.DataType)
            {
                case "string":
                case "date-time":
                    return rawValue;

                case "integer":
                    int intValue;
                    if (int.TryParse(rawValue, out intValue))
                        return intValue;
                    throw new TypeConversionException(node, nodeMetadata.DataType);

                case "number":
                    var cleanedValue = rawValue.TrimStart();

                    if (cleanedValue.StartsWith("$"))
                    {
                        // The row being processed may precede a 'number' value with a $,
                        // as it is reasonable that the input CSV would contain such a prefix
                        // in practice.
                        //
                        // We silently remove the $ so that the expected numeric literal will
                        // be output in the JSON as expected.

                        cleanedValue = cleanedValue[1..];
                    }

                    decimal decimalValue;
                    if (decimal.TryParse(cleanedValue, out decimalValue))
                        return decimalValue;
                    throw new TypeConversionException(node, nodeMetadata.DataType);

                case "boolean":
                    bool boolValue;
                    if (bool.TryParse(rawValue, out boolValue))
                        return boolValue;
                    throw new TypeConversionException(node, nodeMetadata.DataType);

                default:
                    throw new TypeConversionException(node, nodeMetadata.DataType, typeIsUnsupported: true);
            }
        }

        private class CsvRow
        {
            private readonly Dictionary<string, string> _csvRow;

            public CsvRow(Dictionary<string, string> csvRow)
            {
                _csvRow = csvRow;
            }

            public string this[string columnName]
            {
                get
                {
                    if (!_csvRow.ContainsKey(columnName))
                        throw new MissingColumnException(columnName);

                    return _csvRow[columnName];
                }
            }
        }
    }
}
