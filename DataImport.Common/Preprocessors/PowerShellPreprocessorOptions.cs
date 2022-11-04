// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataImport.Common.Preprocessors
{
    public class PowerShellPreprocessorOptions
    {
        public static readonly IReadOnlyCollection<string> DefaultCommands = new ReadOnlyCollection<string>(
            new List<string>
            {
                "Write-Output", "Write-Error", "Write-Progress", "Write-Warning", "Write-Information", "Write-Debug", "Write-Verbose", "Write-Host",
                "Export-ModuleMember",
                "Get-Date", "Get-Unique", "New-TimeSpan",
                "ConvertFrom-Json", "ConvertTo-Json", "ConvertFrom-Csv", "ConvertTo-Csv",
                "Measure-Object", "Out-Null",
                "ForEach-Object", "%", "Where-Object", "?", "Select-Object", "Sort-Object"
            });

        public static readonly char CmdletsSeparator = ',';
        public static readonly string ModulesSeparator = "|~|"; // Can't use comma since it is a valid character in the file name

        public PowerShellPreprocessorOptions()
        {
            AllowedCommands = DefaultCommands.ToList();
        }

        public List<string> AllowedCommands { get; set; }

        public List<string> Modules { get; set; }

        public void MergeOptions(string zippedAvailableCmdlets, string zippedPowerShellModules)
        {
            if (!string.IsNullOrWhiteSpace(zippedAvailableCmdlets))
            {
                var commands = zippedAvailableCmdlets.Split(new[] { CmdletsSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (commands.Length > 0)
                {
                    AllowedCommands = AllowedCommands ?? new List<string>();
                    MergeList(AllowedCommands, commands);
                }
            }

            if (!string.IsNullOrWhiteSpace(zippedPowerShellModules))
            {
                var commands = zippedPowerShellModules.Split(new[] { ModulesSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (commands.Length > 0)
                {
                    Modules = Modules ?? new List<string>();
                    MergeList(Modules, commands);
                }
            }
        }

        private static void MergeList(List<string> list, string[] source)
        {
            var newItems = source.Except(list, StringComparer.OrdinalIgnoreCase);
            list.AddRange(newItems);
        }
    }
}
