// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace DataImport.Web.Helpers
{
    public interface IJsonValidator
    {
        bool IsValidJson(string data);
    }

    public class JsonValidator : IJsonValidator
    {
        private readonly ILogger<JsonValidator> _logger;

        public JsonValidator(ILogger<JsonValidator> logger)
        {
            _logger = logger;
        }

        public bool IsValidJson(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            data = data.Trim();

            if (data.StartsWith("{") && data.EndsWith("}") || //For object
                data.StartsWith("[") && data.EndsWith("]")) //For array
            {
                try
                {
                    JToken.Parse(data);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot parse text as JSON.");
                    return false;
                }
            }

            return false;
        }
    }
}