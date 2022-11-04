// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.ComponentModel;

namespace DataImport.Models
{
    public enum ScriptType
    {
        [Description("Custom File Processor (PowerShell)")]
        CustomFileProcessor = 0,
        [Description("Custom Row Processor (PowerShell)")]
        CustomRowProcessor = 1,
        [Description("Custom File Generator (PowerShell)")]
        CustomFileGenerator = 2,
        [Description("Custom File Processor (External)")]
        ExternalFileProcessor = 3,
        [Description("Custom File Generator (External)")]
        ExternalFileGenerator = 4,
    }

    public static class ScriptTypeExtensions
    {
        public static bool IsExternal(this ScriptType scriptType)
            => scriptType is ScriptType.ExternalFileProcessor or ScriptType.ExternalFileGenerator;

        public static bool IsPowerShell(this ScriptType scriptType)
            => scriptType is ScriptType.CustomFileProcessor or ScriptType.CustomRowProcessor or ScriptType.CustomFileGenerator;
    }
}
