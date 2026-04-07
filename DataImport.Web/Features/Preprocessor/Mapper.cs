// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Preprocessor
{
    public static class Mapper
    {
        public static Script ToScript(this AddEditPreprocessorViewModel src) =>
            new()
            {
                // Id and DataMaps are intentionally omitted — managed by EF
                Name = src.Name,
                ScriptContent = src.ScriptContent,
                // When unset, default(ScriptType)=CustomFileProcessor (0).
                ScriptType = src.ScriptType ?? default,
                RequireOdsApiAccess = src.RequireOdsApiAccess,
                HasAttribute = src.HasAttribute,
                ExecutablePath = src.ExecutablePath,
                ExecutableArguments = src.ExecutableArguments
            };

        public static void ApplyToScript(this AddEditPreprocessorViewModel src, Script dest)
        {
            // Id and DataMaps are intentionally omitted — EF manages these
            dest.Name = src.Name;
            dest.ScriptContent = src.ScriptContent;
            // When unset, default(ScriptType)=CustomFileProcessor (0).
            dest.ScriptType = src.ScriptType ?? default;
            dest.RequireOdsApiAccess = src.RequireOdsApiAccess;
            dest.HasAttribute = src.HasAttribute;
            dest.ExecutablePath = src.ExecutablePath;
            dest.ExecutableArguments = src.ExecutableArguments;
        }

        public static AddEditPreprocessorViewModel ToViewModel(this Script src) =>
            new()
            {
                Id = src.Id,
                Name = src.Name,
                ScriptContent = src.ScriptContent,
                ScriptType = src.ScriptType,
                RequireOdsApiAccess = src.RequireOdsApiAccess,
                HasAttribute = src.HasAttribute,
                ExecutablePath = src.ExecutablePath,
                ExecutableArguments = src.ExecutableArguments
                // ScriptTypes and ExternalPreprocessorsEnabled are intentionally omitted — filled by handler
            };

        public static PreprocessorIndex.PreprocessorIndexModel ToIndexModel(this Script src) =>
            new()
            {
                Id = src.Id,
                Name = src.Name,
                ScriptType = src.ScriptType
                // UsedBy is intentionally omitted — filled by handler after mapping
            };
    }
}
