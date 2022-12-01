// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using Microsoft.EntityFrameworkCore;

namespace DataImport.Web.Features.Share
{
    public class SharingPreprocessorValidationService
    {
        private readonly DataImportDbContext _database;
        private readonly IPowerShellPreprocessorService _powerShellPreprocessorService;


        public SharingPreprocessorValidationService(DataImportDbContext database, IPowerShellPreprocessorService powerShellPreprocessorService)
        {
            _database = database;
            _powerShellPreprocessorService = powerShellPreprocessorService;
        }

        public bool HasConflictingPreprocessors(SharingModel sharingModel, out List<SharingPreprocessor> conflictingPreprocessors)
        {
            if (sharingModel == null) throw new ArgumentNullException(nameof(sharingModel));

            conflictingPreprocessors = new List<SharingPreprocessor>();

            if (sharingModel.Template?.Preprocessors == null)
            {
                return false;
            }

            var preprocessorNames = sharingModel.Template.Preprocessors.Select(x => x.Name).ToArray();

            var existingPreprocessors = _database.Scripts.Where(x => x.ScriptType == ScriptType.CustomFileProcessor && preprocessorNames.Contains(x.Name)).AsNoTracking().ToList();

            conflictingPreprocessors = sharingModel.Template.Preprocessors.Where(x => HasConflict(x, existingPreprocessors)).ToList();
            if (conflictingPreprocessors.Count == 0)
            {
                return false;
            }

            return true;
        }

        private bool HasConflict(SharingPreprocessor preprocessor, IList<Script> existingPreprocessors)
        {
            var existingPreprocessor = existingPreprocessors.SingleOrDefault(x => x.Name == preprocessor.Name && (x.ScriptType == ScriptType.CustomFileProcessor || x.ScriptType == ScriptType.ExternalFileProcessor));
            if (existingPreprocessor != null)
            {
                return preprocessor.HasConflict(existingPreprocessor);
            }

            return false;
        }
    }
}
