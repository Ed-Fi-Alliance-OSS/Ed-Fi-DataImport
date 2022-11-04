// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.TestHelpers;
using DataImport.Web.Features.Shared.SelectListProviders;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using System.Collections.Generic;
using DataImport.Common.Preprocessors;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Tests.Features.Shared
{
    class ScriptTypeSelectListProviderTests
    {
        [Test]
        public void ShouldSuccessfullyReturnScriptTypeSelectListItems()
        {
            var settings = new ExternalPreprocessorOptions { Enabled = true };
            var scriptTypeSelectListProvider = new ScriptTypeSelectListProvider(Options.Create(settings));
            scriptTypeSelectListProvider.GetSelectListItems().ShouldMatch(new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select Type",
                    Value = null,
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Processor (PowerShell)",
                    Value = "0",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom Row Processor (PowerShell)",
                    Value = "1",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Generator (PowerShell)",
                    Value = "2",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Processor (External)",
                    Value = "3",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Generator (External)",
                    Value = "4",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
            });
        }

        [Test]
        public void ShouldExcludeExternalPreProcessorsWhenDisabled()
        {
            var settings = new ExternalPreprocessorOptions { Enabled = false };
            var scriptTypeSelectListProvider = new ScriptTypeSelectListProvider(Options.Create(settings));
            scriptTypeSelectListProvider.GetSelectListItems().ShouldMatch(new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select Type",
                    Value = null,
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Processor (PowerShell)",
                    Value = "0",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom Row Processor (PowerShell)",
                    Value = "1",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
                new SelectListItem
                {
                    Text = "Custom File Generator (PowerShell)",
                    Value = "2",
                    Disabled = false,
                    Group = null,
                    Selected = false
                },
            });
        }
    }
}
