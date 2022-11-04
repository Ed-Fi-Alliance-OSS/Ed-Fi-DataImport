// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataImport.Web.Features
{
    public static class ControllerExtensions
    {
        public static ActionResult RedirectToActionJson<TController>(this TController controller, string action, string controllerName)
            where TController : Controller
        {
            return controller.JsonNet(new
                {
                    redirect = controller.Url.Action(action, controllerName)
                }
            );
        }

        public static ActionResult RedirectToActionJson<TController>(this TController controller, string action)
            where TController : Controller
        {
            return controller.JsonNet(new
                {
                    redirect = controller.Url.Action(action)
                }
            );
        }

        public static ContentResult JsonNet(this Controller controller, object model)
        {
            var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return new ContentResult
            {
                Content = serialized,
                ContentType = "application/json"
            };
        }
    }
}