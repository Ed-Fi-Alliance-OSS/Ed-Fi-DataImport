// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Newtonsoft.Json.Linq;

namespace DataImport.Models
{
    public class SwaggerMetadataParser
    {
        public static ResourceMetadata[] Parse(string resourcePath, string resourceMetadata)
        {
            var resourceJson = JObject.Parse(resourceMetadata);

            if (resourceJson["swaggerVersion"].Value<string>() == "1.2")
            {
                var swaggerResourceName = resourcePath.StartsWith("/")
                    ? resourcePath.Substring(1)
                    : resourcePath;

                var model = swaggerResourceName.Singularize(inputIsKnownToBePlural: false);

                return GetFieldsForModelFromV1Swagger(resourceJson, model).ToArray();
            }

            if (resourceJson["swaggerVersion"].Value<string>() == "2.0")
                return GetFieldsForModelFromV2Swagger(resourceJson).ToArray();

            if (resourceJson["swaggerVersion"].Value<string>() == "3.0")
                return GetFieldsForModelFromV3Swagger(resourceJson).ToArray();

            throw new NotSupportedException("Swagger version number is missing.");
        }

        private static IEnumerable<ResourceMetadata> GetFieldsForModelFromV1Swagger(JObject resourceJson, string model)
        {
            foreach (var propertyToken in resourceJson["models"][model]["properties"])
            {
                var property = (JProperty) propertyToken;
                var field = new ResourceMetadata
                {
                    Name = property.Name
                };

                foreach (var detailToken in property)
                {
                    var detail = (JObject) detailToken;

                    if (detail["required"].ToString() == "True")
                        field.Required = true;

                    field.DataType = detail["type"].ToString();

                    if (field.DataType == "array")
                    {
                        var subModel = detail["items"]["$ref"].Value<string>();

                        //subModel is the type of a single item in this array.
                        //Create a node for that item, and then recur to describe
                        //that item's own details in full.

                        field.Children.Add(new ResourceMetadata
                        {
                            Name = subModel,
                            DataType = subModel,
                            Children = GetFieldsForModelFromV1Swagger(resourceJson, subModel).ToList()
                        });
                    }
                    else
                    {
                        // Infer whether this is a complex type requiring a recursive call, by checking whether that recursive call would
                        // successfully find a model definition for this type. In other words, we avoid recursion for trivial types like
                        // "string", "date-time", "boolean", "integer", and "number", while avoiding future bugs if other trivial types
                        // begin to appear in the metadata.

                        var subModel = field.DataType;
                        var subTypeJson = resourceJson["models"][subModel];

                        bool fieldTypeHasModelDefinition = subTypeJson != null;

                        if (fieldTypeHasModelDefinition)
                            field.Children = GetFieldsForModelFromV1Swagger(resourceJson, subModel).ToList();
                    }
                }

                if (IsApplicableField(field))
                    yield return field;
            }
        }

        private static IEnumerable<ResourceMetadata> GetFieldsForModelFromV2Swagger(JObject resourceJson, JObject fullResourceJson = null)
        {
            fullResourceJson = fullResourceJson ?? resourceJson;
            var requiredProperties = resourceJson["required"] != null
                ? resourceJson["required"].Select(x => x.ToString()).ToArray()
                : new string[0];

            foreach (var propertyToken in resourceJson["properties"])
            {
                var property = (JProperty) propertyToken;
                var field = new ResourceMetadata
                {
                    Name = property.Name,
                    Required = requiredProperties.Contains(property.Name)
                };

                foreach (var detailToken in property)
                {
                    var detail = (JObject) detailToken;
                    field.DataType = detail["$ref"] != null
                        ? GetReferenceName(detail["$ref"])
                        : detail["type"].ToString();

                    if (field.DataType == "array")
                    {
                        var subModel = GetReferenceName(detail["items"]["$ref"]);
                        var subModelJson = fullResourceJson["models"][subModel];
                        //subModel is the type of a single item in this array.
                        //Create a node for that item, and then recur to describe
                        //that item's own details in full.

                        field.Children.Add(new ResourceMetadata
                        {
                            Name = GetFormattedModelName(subModel),
                            DataType = subModel,
                            Children = GetFieldsForModelFromV2Swagger((JObject) subModelJson, fullResourceJson).ToList()
                        });
                    }
                    else
                    {
                        // Infer whether this is a complex type requiring a recursive call, by checking whether that recursive call would
                        // successfully find a model definition for this type. In other words, we avoid recursion for trivial types like
                        // "string", "date-time", "boolean", "integer", and "number", while avoiding future bugs if other trivial types
                        // begin to appear in the metadata.

                        var subModel = field.DataType;
                        var subTypeJson = fullResourceJson["models"][subModel];

                        bool fieldTypeHasModelDefinition = subTypeJson != null;

                        if (fieldTypeHasModelDefinition)
                            field.Children = GetFieldsForModelFromV2Swagger((JObject) subTypeJson, fullResourceJson).ToList();
                    }
                }

                if (IsApplicableField(field))
                    yield return field;
            }
        }

        private static IEnumerable<ResourceMetadata> GetFieldsForModelFromV3Swagger(JObject resourceJson, JObject fullResourceJson = null)
        {
            fullResourceJson = fullResourceJson ?? resourceJson;
            var requiredProperties = resourceJson != null && resourceJson["required"] != null
                ? resourceJson["required"].Select(x => x.ToString()).ToArray()
                : new string[0];

            foreach (var propertyToken in resourceJson["properties"])
            {
                var property = (JProperty) propertyToken;
                var field = new ResourceMetadata
                {
                    Name = property.Name,
                    Required = requiredProperties.Contains(property.Name)
                };

                foreach (var detailToken in property)
                {
                    var detail = (JObject) detailToken;
                    field.DataType = detail["$ref"] != null
                        ? GetReferenceNameFromComponentsSchemas(detail["$ref"])
                        : detail["type"].ToString();

                    if (field.DataType == "array")
                    {
                        var subModel = GetReferenceNameFromComponentsSchemas(detail["items"]["$ref"]);
                        var subModelJson = fullResourceJson["models"][subModel];
                        //subModel is the type of a single item in this array.
                        //Create a node for that item, and then recur to describe
                        //that item's own details in full.

                        field.Children.Add(new ResourceMetadata
                        {
                            Name = GetFormattedModelName(subModel),
                            DataType = subModel,
                            Children = GetFieldsForModelFromV3Swagger((JObject) subModelJson, fullResourceJson).ToList()
                        });
                    }
                    else
                    {
                        // Infer whether this is a complex type requiring a recursive call, by checking whether that recursive call would
                        // successfully find a model definition for this type. In other words, we avoid recursion for trivial types like
                        // "string", "date-time", "boolean", "integer", and "number", while avoiding future bugs if other trivial types
                        // begin to appear in the metadata.

                        var subModel = field.DataType;
                        var subTypeJson = fullResourceJson["models"][subModel];

                        bool fieldTypeHasModelDefinition = subTypeJson != null;

                        if (fieldTypeHasModelDefinition)
                            field.Children = GetFieldsForModelFromV3Swagger((JObject) subTypeJson, fullResourceJson).ToList();
                    }
                }

                if (IsApplicableField(field))
                    yield return field;
            }
        }

        private static readonly string[] _ignoredFields = { "id", "link", "_etag" };
        private static bool IsApplicableField(ResourceMetadata field)
        {
            return !_ignoredFields.Contains(field.Name);
        }

        private static string GetReferenceName(JToken reference)
            => reference.Value<string>().Replace("#/definitions/", "");

        private static string GetReferenceNameFromComponentsSchemas(JToken reference)
            => reference.Value<string>().Replace("#/components/schemas/", "");

        private static string GetFormattedModelName(string resource) => resource.Substring(resource.IndexOf('_') + 1);
    }
}
