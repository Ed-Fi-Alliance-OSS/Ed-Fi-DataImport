// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace DataImport.Web.Infrastructure
{
    public class IdentityToRootPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            if (model.RelativePath.StartsWith("/Areas/Identity"))
            {
                foreach (var selector in model.Selectors)
                {
                    selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template.Replace("Identity", string.Empty);
                }
            }
        }
    }
}
