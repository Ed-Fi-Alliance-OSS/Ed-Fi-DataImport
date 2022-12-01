// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DataImport.Common.Preprocessors;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Features.Shared.SelectListProviders
{
    public class ScriptTypeSelectListProvider
    {
        private readonly ExternalPreprocessorOptions _settings;

        public ScriptTypeSelectListProvider(IOptions<ExternalPreprocessorOptions> settings)
        {
            _settings = settings.Value;
        }

        public IList<SelectListItem> GetSelectListItems()
        {
            var enums = new List<SelectListItem> { new SelectListItem { Text = "Select Type" } };
            foreach (var value in Enum.GetValues(typeof(ScriptType)))
            {
                if (!_settings.Enabled && ((ScriptType) value).IsExternal())
                    continue;

                enums.Add(new SelectListItem
                {
                    Value = ((int) value).ToString(),
                    Text = ((ScriptType) value).Humanize()
                });
            }

            return enums;
        }
    }
}
