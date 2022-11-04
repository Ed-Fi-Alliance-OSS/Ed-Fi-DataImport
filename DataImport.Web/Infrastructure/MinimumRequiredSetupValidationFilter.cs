// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.ApiServers;
using DataImport.Web.Features.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using DataImport.Web.Features.Home;
using DataImport.Web.Features.OpenIdConnect;

namespace DataImport.Web.Infrastructure
{
    /// <summary>
    /// Forces the end user to first provide a valid Template Sharing Service key in Configuration
    /// and then at least one API Connection before using the rest of the product.
    /// </summary>
    public class MinimumRequiredSetupValidationFilter : ActionFilterAttribute
    {
        private readonly DataImportDbContext _dbContext;

        public MinimumRequiredSetupValidationFilter(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is ConfigurationController ||
                filterContext.Controller is ApiServersController ||
                filterContext.Controller is OpenIdConnectController ||
                (filterContext.Controller is HomeController && (string)filterContext.RouteData.Values["action"] == "UserUnauthorized"))
            {
                return;
            }

            if (!_dbContext.ApiServers.Any())
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                            { "controller", "Configuration" },
                            { "action", "Index" },
                            { nameof(EditConfiguration.Query.OdsApiServerException), true }
                    });
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
