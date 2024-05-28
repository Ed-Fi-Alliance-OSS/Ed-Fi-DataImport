// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using DataImport.Models;
using DataImport.Web.Features.BootstrapData;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Lookup;
using DataImport.Web.Features.Preprocessor;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DataImport.Web.Features.Share
{
    public class SharingModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ApiVersion { get; set; }
        public SharingTemplate Template { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, UsingIndentedCamelCase());
        }

        public string SerializeTemplate()
        {
            return JsonConvert.SerializeObject(Template, UsingIndentedCamelCase());
        }

        public static SharingModel Deserialize(IFormFile json)
        {
            using var stream = json.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            using JsonReader jsonReader = new JsonTextReader(streamReader);
            return JsonSerializer
                .Create(UsingIndentedCamelCase())
                .Deserialize<SharingModel>(jsonReader);
        }

        public static SharingModel Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<SharingModel>(json, UsingIndentedCamelCase());
        }

        private static JsonSerializerSettings UsingIndentedCamelCase()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }
    }

    public class SharingTemplate
    {
        public SharingMap[] Maps { get; set; }
        public SharingBootstrap[] Bootstraps { get; set; }
        public SharingLookup[] Lookups { get; set; }
        public string SupplementalInformation { get; set; }
        public SharingPreprocessor[] Preprocessors { get; set; }
    }

    public class SharingContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }

        public static SharingContact Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<SharingContact>(json);
        }
    }

    public class SharingBootstrap
    {
        public string Name { get; set; }
        public string ResourcePath { get; set; }
        public JToken Data { get; set; }

        public AddBootstrapData.Command ToAddCommand(int apiVersionId)
        {
            return new AddBootstrapData.Command
            {
                Name = Name,
                ResourcePath = ResourcePath,
                Data = Data.ToString(Formatting.Indented),
                ApiVersionId = apiVersionId
            };
        }
    }

    public class SharingMap
    {
        public string Name { get; set; }
        public string ResourcePath { get; set; }
        public string[] ColumnHeaders { get; set; }
        public JObject Map { get; set; }
        public string CustomFileProcessor { get; set; }
        public string Attribute { get; set; }
        public string SelectedIngestionLogEdOrgIdColumn { get; set; }

        public AddDataMap.Command ToAddCommand(Resource resource, int? preprocessorId)
        {
            var serializer = new DataMapSerializer(resource);

            return new AddDataMap.Command
            {
                MapName = Name,
                ResourcePath = ResourcePath,
                ColumnHeaders = ColumnHeaders,
                Mappings = serializer.Deserialize(Map),
                ApiVersionId = resource.ApiVersionId,
                PreprocessorId = preprocessorId,
                Attribute = Attribute,
                SelectedIngestionLogEdOrgIdColumn = SelectedIngestionLogEdOrgIdColumn
            };
        }
    }

    public class SharingLookup
    {
        public string SourceTable { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public AddLookup.Command ToAddCommand()
        {
            return new AddLookup.Command
            {
                SourceTable = SourceTable,
                Key = Key,
                Value = Value
            };
        }
    }

    public class SharingPreprocessor
    {
        public string Name { get; set; }

        public string ScriptContent { get; set; }

        public bool RequireOdsApiAccess { get; set; }

        public bool HasAttribute { get; set; }

        public string ExecutablePath { get; set; }

        public string ExecutableArguments { get; set; }

        public AddPreprocessor.Command ToCustomFileProcessorAddCommand()
        {
            return new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    HasAttribute = HasAttribute,
                    Name = Name,
                    RequireOdsApiAccess = RequireOdsApiAccess,
                    ScriptContent = ScriptContent,
                    ExecutablePath = ExecutablePath,
                    ExecutableArguments = ExecutableArguments,
                    ScriptType = DetermineScriptTypeFromProperties(),
                }
            };
        }

        public EditPreprocessor.Command ToCustomFileProcessorEditCommand(int existingPreprocessorId)
        {
            return new EditPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    Id = existingPreprocessorId,
                    HasAttribute = HasAttribute,
                    Name = Name,
                    RequireOdsApiAccess = RequireOdsApiAccess,
                    ScriptContent = ScriptContent,
                    ExecutablePath = ExecutablePath,
                    ExecutableArguments = ExecutableArguments,
                    ScriptType = DetermineScriptTypeFromProperties(),
                }
            };
        }

        public bool HasConflict(Script existingPreprocessor)
        {
            return existingPreprocessor.ScriptContent != ScriptContent || existingPreprocessor.HasAttribute != HasAttribute
                || existingPreprocessor.RequireOdsApiAccess != RequireOdsApiAccess || existingPreprocessor.ExecutablePath != ExecutablePath
                || existingPreprocessor.ExecutableArguments != ExecutableArguments;
        }

        private ScriptType DetermineScriptTypeFromProperties()
        {
            if (!string.IsNullOrEmpty(ExecutablePath) && !string.IsNullOrEmpty(ScriptContent))
                throw new Exception("Unable to determine Script Type: Both External and PowerShell fields are populated.");
            if (string.IsNullOrEmpty(ExecutablePath) && string.IsNullOrEmpty(ScriptContent))
                throw new Exception("Unable to determine Script Type: Neither External nor PowerShell fields are populated.");

            return !string.IsNullOrEmpty(ExecutablePath) ? ScriptType.ExternalFileProcessor : ScriptType.CustomFileProcessor;
        }
    }
}
