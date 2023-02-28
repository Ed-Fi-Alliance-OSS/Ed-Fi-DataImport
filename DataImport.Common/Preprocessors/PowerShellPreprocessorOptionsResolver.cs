// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using System.Linq;

namespace DataImport.Common.Preprocessors
{
    public class PowerShellPreprocessorOptionsResolver
    {
        private readonly DataImportDbContext _dataImportDbContext;

        public PowerShellPreprocessorOptionsResolver(DataImportDbContext dataImportDbContext)
        {
            _dataImportDbContext = dataImportDbContext;
        }

        public PowerShellPreprocessorOptions Resolve()
        {
            var powerShellPreprocessorOptions = new PowerShellPreprocessorOptions();

            var psOptions = _dataImportDbContext.Configurations.Select(x => new { x.AvailableCmdlets, ImportPSModules = x.ImportPSModules }).SingleOrDefault();
            if (psOptions != null)
            {
                powerShellPreprocessorOptions.MergeOptions(psOptions.AvailableCmdlets, psOptions.ImportPSModules);
            }

            return powerShellPreprocessorOptions;
        }
    }
}
