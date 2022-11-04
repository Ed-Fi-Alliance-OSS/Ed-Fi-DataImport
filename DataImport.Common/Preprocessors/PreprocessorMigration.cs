// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;

namespace DataImport.Common.Preprocessors
{
    public class PreprocessorMigration
    {
        private readonly IFileService _fileService;
        private readonly DataImportDbContext _dataImportDbContext;

        public PreprocessorMigration(IFileService fileService, DataImportDbContext dataImportDbContext)
        {
            _fileService = fileService;
            _dataImportDbContext = dataImportDbContext;
        }

        public bool CheckIfMigrationNeeded()
        {
            return _dataImportDbContext.Scripts.Any(x => (x.ScriptType == ScriptType.CustomFileGenerator || x.ScriptType == ScriptType.CustomRowProcessor) && x.ScriptContent == null);
        }

        public async Task Migrate()
        {
            var scriptsToMigrate = _dataImportDbContext.Scripts.Where(x => (x.ScriptType == ScriptType.CustomFileGenerator || x.ScriptType == ScriptType.CustomRowProcessor) && x.ScriptContent == null).ToList();
            foreach (var script in scriptsToMigrate)
            {
                try
                {
                    script.ScriptContent = script.ScriptType == ScriptType.CustomRowProcessor
                        ? await _fileService.GetRowProcessorScript(script.Name)
                        : await _fileService.GetFileGeneratorScript(script.Name);
                }
                catch (Exception e)
                {
                    script.ScriptContent = $"Script Migration Error: {e}";
                }
            }

            _dataImportDbContext.SaveChanges();
        }
    }
}
