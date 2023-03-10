// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.ApiServers;
using DataImport.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DataImport.Web.Infrastructure
{
    public class MvcTransactionFilter : ActionFilterAttribute
    {
        private readonly ILogger<MvcTransactionFilter> _logger;
        private readonly DataImportDbContext _dbContext;
        private readonly LinkGenerator _generator;

        public MvcTransactionFilter(ILogger<MvcTransactionFilter> logger, DataImportDbContext dbContext, LinkGenerator generator)
        {
            _logger = logger;
            _dbContext = dbContext;
            _generator = generator;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _dbContext.BeginTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var exception = filterContext.Exception;

            _dbContext.CloseTransaction(exception);

            if (exception != null)
            {
                _logger.LogError(exception, "Internal Server Error");

                if (exception is OdsApiServerException odsApiServerException)
                {
                    HandleOdsApiServerException(filterContext, odsApiServerException, _generator);
                }
            }
        }

        private static void HandleOdsApiServerException(ActionExecutedContext filterContext, OdsApiServerException odsApiServerException, LinkGenerator generator)
        {
            filterContext.ExceptionHandled = true;

            RedirectToRouteResult result = null;
            if (odsApiServerException.ApiServerId.HasValue)
            {
                result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                            { "controller", "ApiServers" },
                            { "action", "Edit" },
                            { nameof(EditApiServer.Query.OdsApiServerException), true },
                            { "Id", odsApiServerException.ApiServerId.Value }
                    });
            }
            //else
            //{
            //    result = new RedirectToRouteResult(
            //        new RouteValueDictionary
            //        {
            //            { "controller", "Configuration" },
            //            { "action", "Index" },
            //            { nameof(EditConfiguration.Query.OdsApiServerException), true }
            //        });
            //}

            if (!(filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                filterContext.Result = result;
            }
            else
            {
                AjaxRedirect(filterContext, result, generator);
            }
        }

        private static void AjaxRedirect(ActionExecutedContext filterContext, RedirectToRouteResult result, LinkGenerator generator)
        {
            var action = (string) result.RouteValues["action"];
            var controller = (string) result.RouteValues["controller"];
            result.RouteValues.Remove("action");
            result.RouteValues.Remove("controller");

            var redirectUrl = generator.GetUriByAction(action, controller, result.RouteValues,
                filterContext.HttpContext.Request.Scheme, filterContext.HttpContext.Request.Host);

            // JSON response is not allowed in GET requests by default in ASP.NET MVC due to security concerns
            if (filterContext.HttpContext.Request.Method == "GET")
            {
                // REDIRECT_LOCATION must be handled on the client side.
                filterContext.HttpContext.Response.Headers.Add("REDIRECT_LOCATION", redirectUrl);
            }
            else
            {
                filterContext.Result = new JsonResult(new { redirect = redirectUrl });
            }
        }
    }
}
