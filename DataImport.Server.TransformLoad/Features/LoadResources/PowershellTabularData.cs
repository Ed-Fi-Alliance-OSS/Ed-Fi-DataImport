// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using DataImport.Common.Preprocessors;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class PowershellTabularData : ITabularData
    {
        private readonly string _script;
        private readonly ITabularData _original;
        private readonly ProcessOptions _options;
        private readonly IPowerShellPreprocessorService _powerShellPreprocessorService;
        private Runspace _runspace;
        private PowerShell _powershell;

        public PowershellTabularData(IPowerShellPreprocessorService powerShellPreprocessorService, string script, ITabularData original, ProcessOptions options = null)
        {
            _script = script;
            _original = original;
            _options = options;
            _powerShellPreprocessorService = powerShellPreprocessorService;
        }

        private void Open()
        {
            var psEnvironment = _powerShellPreprocessorService.CreatePowerShellEnvironment(_options);
            _runspace = psEnvironment.runspace;
            _powershell = psEnvironment.powerShell;
        }

        public IEnumerable<Dictionary<string, string>> GetRows()
        {
            if (_runspace == null || _powershell == null)
                Open();

            foreach (var row in _original.GetRows())
            {
                MutateRow(row);
                yield return row;
            }
        }

        private void MutateRow(Dictionary<string, string> row)
        {
            using (var pipeline = _runspace.CreatePipeline())
            {
                var transformRow = new Command(_script, isScript: true);
                transformRow.Parameters.Add("row", row);

                pipeline.Commands.Add(transformRow);
                pipeline.Invoke();
            }
        }

        public void Dispose()
        {
            _original?.Dispose();
            _runspace?.Dispose();
            _powershell?.Dispose();
        }
    }
}