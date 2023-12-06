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
    public class DeleteDataMapSerializer
    {
        public string Serialize(DataMapper[] mappings)
        {
            return SerializeObjectForDeleteById(mappings.Single()).ToString(Formatting.Indented);
        }

        public DataMapper[] Deserialize(string jsonMap)
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

            return Deserialize(jobject);
        }
        public DataMapper[] Deserialize(JObject jsonMap)
        {
            return DeserializeObjectForDeleteById(jsonMap).ToArray();
        }

        private JObject SerializeObjectForDeleteById(DataMapper node)
        {
            var result = new JObject { new JProperty("Id", new JObject { new JProperty("Column", node.SourceColumn) }) };
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
