// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Features.UserReset;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace DataImport.Web.Infrastructure
{
    public class ValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller.GetType() != typeof(RecoverUserController))
            {
                var viewData = ((Controller) filterContext.Controller).ViewData;

                if (!viewData.ModelState.IsValid)
                {
                    if (filterContext.HttpContext.Request.Method == "GET")
                    {
                        filterContext.Result = new BadRequestResult();
                    }
                    else if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var result = new ContentResult();
                        string content = JsonConvert.SerializeObject(viewData.ModelState,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
                        result.Content = content;
                        result.ContentType = "application/json";

                        filterContext.HttpContext.Response.StatusCode = 400;
                        filterContext.Result = result;
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}
