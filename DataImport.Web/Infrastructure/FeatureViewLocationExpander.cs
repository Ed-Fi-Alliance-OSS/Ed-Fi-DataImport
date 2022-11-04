// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Razor;

// Jan 2022: R# doesn't see Feature Folders in ASP.NET Core yet – https://youtrack.jetbrains.com/issue/RSRP-461882
[assembly: AspMvcViewLocationFormat("~/Features/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/Shared/{0}.cshtml")]
[assembly: AspMvcPartialViewLocationFormat("~/Features/{1}/{0}.cshtml")]
[assembly: AspMvcPartialViewLocationFormat("~/Features/Shared/{0}.cshtml")]
namespace DataImport.Web.Infrastructure
{
    public class FeatureViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var featureFolderViewLocations = new[]
            {
                "~/Features/{1}/{0}.cshtml",
                "~/Features/{1}/{0}.vbhtml",
                "~/Features/Shared/{0}.cshtml",
                "~/Features/Shared/{0}.vbhtml",
            };

            return viewLocations.Concat(featureFolderViewLocations);
        }
    }
}