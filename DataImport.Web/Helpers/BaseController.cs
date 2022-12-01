// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Mvc;

namespace DataImport.Web.Helpers
{
    public abstract class BaseController : Controller
    {
        protected ActionResult AjaxRedirectToAction(string actionName)
        {
            return Json(new { redirect = Url.Action(actionName) });
        }

        protected void SuccessMessage(string message)
        {
            Toast(message, "success");
        }

        protected void ToastMessage(ToastResponse response)
        {
            if (response.IsSuccess)
            {
                SuccessMessage(response.Message);
            }
            else
            {
                ErrorMessage(response.Message);
            }
        }

        protected void ErrorMessage(string message = "The transaction could not be completed.")
        {
            Toast(message, "error");
        }

        private void Toast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }
    }
}
