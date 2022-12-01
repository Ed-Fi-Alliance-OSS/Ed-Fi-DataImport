// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

namespace DataImport.Web.Services
{
    public interface IHtmlHelperService
    {
        HtmlString NavigationButton<TModel>(IHtmlHelper<TModel> html, string text, string action = null, string controller = null);
    }

    public class HtmlHelperService : IHtmlHelperService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly LinkGenerator _generator;

        public HtmlHelperService(IHttpContextAccessor accessor, LinkGenerator generator)
        {
            _accessor = accessor;
            _generator = generator;
        }

        public HtmlString NavigationButton<TModel>(IHtmlHelper<TModel> html, string text, string action = null, string controller = null)
        {
            if (controller == null)
                controller = (string) html.ViewContext.RouteData.Values["controller"];

            if (action == null)
                action = text;

            var actionUri = _generator.GetUriByAction(_accessor.HttpContext, action, controller);
            var input = new TagBuilder("a");
            input.Attributes.Add("href", actionUri);
            input.Attributes.Add("role", "button");
            input.AddCssClass("btn btn-primary");
            input.InnerHtml.AppendHtml(text);

            return input.ToHtmlString();
        }
    }
}
