// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataImport.Web.Features.Shared.SelectListProviders
{
    public class PreprocessorSelectListProvider
    {
        private readonly DataImportDbContext _dbContext;

        public PreprocessorSelectListProvider(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<PreprocessorDropDownItem> GetCustomFileProcessors()
        {
            List<PreprocessorDropDownItem> selectList = new List<PreprocessorDropDownItem>
            {
                new PreprocessorDropDownItem { Text = "Select Processor", Value = string.Empty }
            };

            var preprocessors = _dbContext.Scripts
                .Where(x => x.ScriptType == ScriptType.CustomFileProcessor || x.ScriptType == ScriptType.ExternalFileProcessor)
                .Select(x => new PreprocessorDropDownItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(CultureInfo.InvariantCulture),
                    RequiresApiConnection = x.RequireOdsApiAccess,
                    RequiresAttribute = x.HasAttribute
                });
            selectList.AddRange(preprocessors);

            return selectList;
        }
    }

    public class PreprocessorDropDownItem : SelectListItem
    {
        public bool RequiresApiConnection { get; set; }

        public bool RequiresAttribute { get; set; }
    }
}
