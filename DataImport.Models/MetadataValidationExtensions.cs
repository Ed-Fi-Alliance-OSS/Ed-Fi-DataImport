// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DataImport.Models
{
    public enum MetadataCompatibilityLevel
    {
        Bootstrap = 0,
        DataMap = 1
    }

    public static class MetadataValidationExtensions
    {
        public static bool IsCompatibleWithResource(this JToken jsonToken, Resource resource, MetadataCompatibilityLevel level, out string errorMessage)
        {
            if (jsonToken.Type == JTokenType.Array)
            {
                if (level == MetadataCompatibilityLevel.Bootstrap)
                {
                    foreach (var singleResource in (JArray) jsonToken)
                    {
                        var singleResult = IsObjectCompatibleWithResource((JObject)singleResource, resource, level, out errorMessage);

                        if (!singleResult)
                            return false;
                    }

                    errorMessage = null;
                    return true;
                }
                else
                {
                    errorMessage = $"A single data map object for a single '{resource.Path}' resource " +
                                   "was expected, but an array of objects was provided.";
                    return false;
                }
            }

            return IsObjectCompatibleWithResource((JObject)jsonToken, resource, level, out errorMessage);
        }

        private static bool IsObjectCompatibleWithResource(this JObject jsonMap, Resource resource, MetadataCompatibilityLevel level, out string exceptionMessage)
        {
            try
            {
                var serializer = new DataMapSerializer(resource);
                var deserialized = serializer.Deserialize(jsonMap);

                if (level == MetadataCompatibilityLevel.Bootstrap && deserialized.ReferencedColumns().Any())
                    throw new Exception("Bootstrap JSON cannot include column references.");

                serializer.Serialize(deserialized);
                exceptionMessage = null;
                return true;
            }
            catch (Exception exception)
            {
                exceptionMessage = exception.Message;
                return false;
            }
        }
    }
}