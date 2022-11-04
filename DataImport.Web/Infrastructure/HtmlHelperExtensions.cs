// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Services;
using Humanizer;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace DataImport.Web.Infrastructure
{
    public static class HtmlHelperExtensions
    {
        public static MvcForm Form<TModel>(this IHtmlHelper<TModel> html, string action = null, string controller = null)
        {
            if (controller == null)
                controller = ControllerName(html);

            if (action == null)
                action = ActionName(html);

            var form = html.BeginForm(action, controller, FormMethod.Post, new { @class = "form-horizontal" });

            html.ViewContext.Writer.Write(html.AntiForgeryToken());

            if (!html.ViewData.ModelState.IsValid)
                html.ViewContext.Writer.Write(html.ValidationSummary("", new { @class = "alert alert-danger" }));

            return form;
        }

        public static MvcForm AjaxForm<TModel>(this IHtmlHelper<TModel> html, string action = null, string controller = null, string callback = null)
        {
            if (controller == null)
                controller = ControllerName(html);

            if (action == null)
                action = ActionName(html);

            var form = html.BeginForm(action, controller, FormMethod.Post, new { @class = "form-horizontal opt-in-ajax", data_callback = callback ?? "redirect" });

            html.ViewContext.Writer.Write("<div id=\"validationSummary\" class=\"alert alert-danger hidden\"></div>");

            return form;
        }

        private static string ActionName<TModel>(IHtmlHelper<TModel> html)
        {
            return (string)html.ViewContext.RouteData.Values["action"];
        }

        private static string ControllerName<TModel>(IHtmlHelper<TModel> html)
        {
            return (string)html.ViewContext.RouteData.Values["controller"];
        }

        public static HtmlString ExternalLink<TModel>(this IHtmlHelper<TModel> html, string text, string url)
        {
            var input = new TagBuilder("a");
            input.Attributes.Add("target", "_blank");
            input.Attributes.Add("rel", "noopener noreferrer");
            input.Attributes.Add("href", url);
            input.InnerHtml.AppendHtml(text);

            return input.ToHtmlString();
        }

        public static IHtmlContent Display<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string contentId = null)
        {
            return html.FormGroup(
                html.LabelFor(expression, new { @class = "control-label", style = "padding-top:0" }),
                html.DisplayFor(expression),
                contentId);
        }

        public static HtmlString ReadOnlyInput<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string contentId = null)
        {
            var div = new TagBuilder("div");
            div.InnerHtml.AppendHtml(html.Display(expression, contentId));
            div.InnerHtml.AppendHtml(html.HiddenFor(expression));
            return div.ToHtmlString();
        }

        public static HtmlString Input<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
        {
            var input = new TagBuilder("div");
            input.AddCssClass("form-group");

            var inputDiv = new TagBuilder("div");
            inputDiv.AddCssClass("col-sm-offset-2 col-sm-10");

            inputDiv.InnerHtml.AppendHtml(html.CheckBoxFor(expression));
            inputDiv.InnerHtml.AppendHtml(" ");
            inputDiv.InnerHtml.AppendHtml(html.LabelFor(expression));

            input.InnerHtml.AppendHtml(inputDiv);
            return input.ToHtmlString();
        }

        public static HtmlString Input<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, string label)
        {
            var input = new TagBuilder("div");
            input.AddCssClass("form-group");

            var labelDiv = new TagBuilder("div");
            labelDiv.AddCssClass("col-sm-2");
            labelDiv.InnerHtml.AppendHtml(html.LabelFor(expression, new { @class = "control-label" }));
            input.InnerHtml.AppendHtml(labelDiv);

            var inputDiv = new TagBuilder("div");
            inputDiv.AddCssClass("col-sm-10 checkbox");

            var checkbox = new TagBuilder("label");
            checkbox.Attributes.Add("for", html.NameFor(expression));
            checkbox.InnerHtml.AppendHtml(html.CheckBoxFor(expression));
            checkbox.InnerHtml.AppendHtml(label);
            inputDiv.InnerHtml.AppendHtml(checkbox);

            input.InnerHtml.AppendHtml(inputDiv);
            return input.ToHtmlString();
        }

        public static HtmlString Input<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, IFormFile>> expression)
        {
            var input = DivFormTemplate(html, expression, out var inputDiv);

            var htmlAttributes = new Dictionary<string, object>
            {
                { "type", "file" },
                { "class", "form-control" }
            };

            var accept = Property.From(expression).GetCustomAttributes<AcceptAttribute>().SingleOrDefault();

            if (accept != null)
                htmlAttributes["accept"] = accept.FileTypeSpecifier;

            inputDiv.InnerHtml.AppendHtml(html.TextBoxFor(expression, htmlAttributes));
            inputDiv.InnerHtml.AppendHtml(html.ValidationMessageFor(expression));
            input.InnerHtml.AppendHtml(inputDiv);

            return input.ToHtmlString();
        }

        public static HtmlString Input<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlInputAttributes = null, HtmlString helpBlock = null, HtmlString helpButton = null)
        {
            var input = DivFormTemplate(html, expression, out var inputDiv, helpButton);
            var propertyName = GetPlaceholderName(expression);

            var defaultHtmlAttributes = new Dictionary<string, object>
            {
                { "class", "form-control" },
                { "placeholder", propertyName }
            };
            defaultHtmlAttributes.Merge(htmlInputAttributes);

            inputDiv.InnerHtml.AppendHtml(html.EditorFor(expression, new { htmlAttributes = defaultHtmlAttributes }));
            if (helpBlock != null)
                inputDiv.InnerHtml.AppendHtml(helpBlock);
            inputDiv.InnerHtml.AppendHtml(html.ValidationMessageFor(expression));
            input.InnerHtml.AppendHtml(inputDiv);

            return input.ToHtmlString();
        }

        public static HtmlString DropDown<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> selectList, object htmlInputAttributes = null, HtmlString helpBlock = null, HtmlString helpButton = null)
        {
            var input = DivFormTemplate(html, expression, out var inputDiv, helpButton);

            var defaultHtmlAttributes = new Dictionary<string, object> { { "class", "form-control" } };
            defaultHtmlAttributes.Merge(htmlInputAttributes);
            inputDiv.InnerHtml.AppendHtml(html.DropDownListFor(expression, selectList, defaultHtmlAttributes));
            inputDiv.InnerHtml.AppendHtml(html.ValidationMessageFor(expression));
            if (helpBlock != null)
                inputDiv.InnerHtml.AppendHtml(helpBlock);
            input.InnerHtml.AppendHtml(inputDiv);

            return input.ToHtmlString();
        }

        public static HtmlString Readonly<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string valueText = null, object htmlInputAttributes = null, HtmlString helpBlock = null, HtmlString helpButton = null)
        {
            var input = DivFormTemplate(html, expression, out var inputDiv, helpButton);

            var defaultHtmlAttributes = new Dictionary<string, object> { { "class", "form-control" } };
            defaultHtmlAttributes.Merge(htmlInputAttributes);
            inputDiv.InnerHtml.AppendHtml(html.HiddenFor(expression));
            if (string.IsNullOrEmpty(valueText))
            {
                inputDiv.InnerHtml.AppendHtml(html.LabelFor(expression));
            }
            else
            {
                inputDiv.InnerHtml.AppendHtml(html.Label(null, valueText, new { @class = "control-label" }));
            }

            inputDiv.InnerHtml.AppendHtml(html.ValidationMessageFor(expression));
            if (helpBlock != null)
                inputDiv.InnerHtml.AppendHtml(helpBlock);
            input.InnerHtml.AppendHtml(inputDiv);

            return input.ToHtmlString();
        }

        public static HtmlString SubmitButton<TModel>(this IHtmlHelper<TModel> html, string buttonLabel, object htmlAttributes = null)
        {
            var defaultHtmlInputAttributes = new Dictionary<string, object> { { "type", "submit" } };
            if (htmlAttributes != null)
                defaultHtmlInputAttributes.Merge(htmlAttributes);

            return html.Button(buttonLabel, defaultHtmlInputAttributes);
        }

        public static HtmlString Button<TModel>(this IHtmlHelper<TModel> html, string buttonLabel, object htmlAttributes = null)
        {
            var input = new TagBuilder("div");
            input.AddCssClass("form-group");

            var inputDiv = new TagBuilder("div");
            inputDiv.AddCssClass("col-sm-offset-2 col-sm-10");
            var defaultHtmlAttributes = new Dictionary<string, object>
            {
                { "class", "btn btn-primary" },
                { "type", "button" }
            };
            defaultHtmlAttributes.Merge(htmlAttributes);
            var buttonTag = new TagBuilder("button");
            buttonTag.InnerHtml.AppendHtml(buttonLabel);
            buttonTag.MergeAttributes(defaultHtmlAttributes);

            inputDiv.InnerHtml.AppendHtml(buttonTag);
            input.InnerHtml.AppendHtml(inputDiv);

            return input.ToHtmlString();
        }

        public static HtmlString DescriptorLookupButton(this IHtmlHelper html, string dataHeader, object htmlAttributes = null)
        {
            var button = new TagBuilder("button");
            var defaultHtmlAttributes = new Dictionary<string, object>
            {
                { "class", "descriptor-lookup-btn btn btn-default" },
                { "type", "button" },
                { "data-toggle","modal" },
                { "data-target","#SimpleModal" },
                { "data-header", dataHeader }
            };
            defaultHtmlAttributes.Merge(htmlAttributes);
            button.MergeAttributes(defaultHtmlAttributes);

            var searchIcon = new TagBuilder("span");
            searchIcon.AddCssClass("glyphicon glyphicon-search");
            button.InnerHtml.AppendHtml(searchIcon);

            return button.ToHtmlString();
        }

        public static HtmlString OdsApiUrlHelpModalButton(this IHtmlHelper html)
        {
            const string title = "How to configure your ODS/API URL";

            var bodyRawHtml = @"
            <p>The ODS/API URL will be the Base URI for your ODS instance. If you are unsure on what that would look like for your ODS/API, 
            please look at the examples below that best match your situation.</p>

            <b>ODS/API v2.x (Either Shared or Year-Specific mode):</b>
            <p>In this scenario, the URL should end with something like 'api/v2/2019'. For example, if a GET assessments API call looks like 
            <a>https://ods.example.com/api/v2.0/2019/assessments</a>, then the Base URI would be <a>https://ods.example.com/api/v2.0/2019</a>. 
            The year is required for both Shared or Year-Specific mode but is only relevant for Year-Specific mode.</p>

            <b>ODS/API 3.x (Shared mode):</b>
            <p>In this scenario, the URI should end with something like 'data/v3'. For example, if a GET assessments API call looks like 
            <a>https://ods.example.com/data/v3/ed-fi/assessments</a>, then the Base URI would be <a>https://ods.example.com/data/v3</a>.</p>

            <b>ODS/API 3.x (Year-Specific mode):</b>
            <p>In this scenario, the URI should end with something like 'data/v3/2019'. For example, if a GET assessments API call looks like 
            <a>https://ods.example.com/data/v3/2019/ed-fi/assessments</a>, then the Base URI would be <a>https://ods.example.com/data/v3/2019</a>.</p>";

            return html.HelpModalButton(title, bodyRawHtml);
        }

        public static HtmlString PreprocessorScriptTypeHelpModalButton(this IHtmlHelper html)
        {
            const string title = "Preprocessor Script Type";

            var bodyRawHtml = @"
            <p>Data Import provides support for incorporating scriptable logic into the file processing pipeline in various ways. Use the applicable option for your needs:</p>

            <h4>Custom File Processor</h4>
            <p>This integrates with a data map to preprocess a file format before it is mapped to an API resource. It can be used to achieve a wide variety of transformation resulting in a tabular output compatible with the mapping designer.</p>
            <p>When to use: </p>
            <ul>
                <li>Non-CSV format files (e.g. tab-delimited, fixed-width, or XML)</li>
                <li>Reshaping of the input data (for example one row becomes multiple or vice-versa)</li>
                <li>Exclusion of rows (e.g. student assessment rows that indicate the student was not tested)</li>
            </ul>

            <h4>Custom Row Processor</h4>
            <p>This integrates with an agent to modify field values within a row.</p>
            <p>When to use:</p>
            <ul>
                <li>The Custom File Processor script type is more versatile in most circumstances, but the Custom Row Processor can be useful if you want to apply some changes only for a specific agent rather than the data map.</li>
            </ul>

            <h4>Custom File Generator</h4>
            <p>This integrates with an agent as an agent type to provide the capability of scripting file generation on a predefined schedule.</p>
            <p>When to use:</p>
            <ul>
                <li>Extracting data from a database to produce a file.</li>
                <li>Joining multiple files together to create an input file.</li>
            </ul>";

            return html.HelpModalButton(title, bodyRawHtml);
        }

        private static HtmlString HelpModalButton(this IHtmlHelper html, string modalTitle, string modalBody)
        {
            var button = new TagBuilder("button");
            var defaultHtmlAttributes = new Dictionary<string, object>
            {
                { "class", "btn btn-link static-modal-btn" },
                { "type", "button" },
                { "data-toggle", "modal" },
                { "data-target", "#SimpleModal" },
                { "data-title", modalTitle },
                { "data-body", modalBody }
            };
            button.MergeAttributes(defaultHtmlAttributes);

            var span = new TagBuilder("span");
            span.AddCssClass("glyphicon glyphicon-question-sign");

            button.InnerHtml.AppendHtml(span);
            return button.ToHtmlString();
        }

        public static HtmlString HelpSpan<TModel>(this IHtmlHelper<TModel> html, string helpText)
        {
            var span = new TagBuilder("span");
            span.AddCssClass("help-block");
            span.InnerHtml.AppendHtml(helpText);
            return span.ToHtmlString();
        }

        public static HtmlString PagingControl<TModel, T>(this IHtmlHelper<TModel> html, string previousUrl,
            string nextUrl, PagedList<T> pagedContent, string behaviorOverrideName = null)
        {
            var previousLink = PreviousButton(previousUrl, pagedContent, behaviorOverrideName);
            var nextLink = NextButton(nextUrl, pagedContent, behaviorOverrideName);
            var pageNumber = PageNumber(pagedContent, previousLink, nextLink);

            var contentWrapper = new TagBuilder("ul");
            contentWrapper.AddCssClass("pagination");
            contentWrapper.InnerHtml.AppendHtml(previousLink);
            contentWrapper.InnerHtml.AppendHtml(pageNumber);
            contentWrapper.InnerHtml.AppendHtml(nextLink);

            var paginationNav = new TagBuilder("nav");
            paginationNav.InnerHtml.AppendHtml(contentWrapper);
            paginationNav.MergeAttribute("aria-label", "Page navigation");

            return paginationNav.ToHtmlString();
        }

        private static TagBuilder PageNumber<T>(PagedList<T> pagedContent, TagBuilder previousLink, TagBuilder nextLink)
        {
            if (previousLink == null && nextLink == null)
                return null;

            var pageNumber = new TagBuilder("span");

            pageNumber.InnerHtml.AppendHtml(" " + pagedContent.PageNumber + " ");

            var result = new TagBuilder("li");
            result.InnerHtml.AppendHtml(pageNumber);
            return result;
        }

        private static TagBuilder NextButton<T>(string nextUrl, PagedList<T> pagedContent, string behaviorOverrideName)
        {
            if (!pagedContent.NextPageHasResults)
                return null;

            var nextLink = new TagBuilder("a");

            nextLink.MergeAttribute("href", nextUrl);
            nextLink.MergeAttribute("aria-label", "Next");
            nextLink.AddCssClass("navigate-next-page" + (behaviorOverrideName == null ? null : "-" + behaviorOverrideName));

            var symbolSpan = new TagBuilder("span");
            symbolSpan.MergeAttribute("aria-hidden", "true");
            symbolSpan.InnerHtml.AppendHtml("&raquo;");

            nextLink.InnerHtml.AppendHtml(symbolSpan);

            var result = new TagBuilder("li");
            result.InnerHtml.AppendHtml(nextLink);
            return result;
        }

        private static TagBuilder PreviousButton<T>(string previousUrl, PagedList<T> pagedContent, string behaviorOverrideName)
        {
            if (pagedContent.PageNumber <= 1)
                return null;

            var previousLink = new TagBuilder("a");

            previousLink.MergeAttribute("href", previousUrl);
            previousLink.MergeAttribute("aria-label", "Previous");
            previousLink.AddCssClass("navigate-previous-page" + (behaviorOverrideName == null ? null : "-" + behaviorOverrideName));

            var symbolSpan = new TagBuilder("span");
            symbolSpan.MergeAttribute("aria-hidden", "true");
            symbolSpan.InnerHtml.AppendHtml("&laquo;");

            previousLink.InnerHtml.AppendHtml(symbolSpan);

            var result = new TagBuilder("li");
            result.InnerHtml.AppendHtml(previousLink);
            return result;
        }

        private static TagBuilder DivFormTemplate<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,
            out TagBuilder inputDiv, HtmlString helpButton = null)
        {
            var input = new TagBuilder("div");
            input.AddCssClass("form-group");

            var labelDiv = new TagBuilder("div");
            labelDiv.AddCssClass("col-sm-2");
            labelDiv.InnerHtml.AppendHtml(html.LabelFor(expression, new { @class = "control-label" }));
            labelDiv.InnerHtml.AppendHtml(helpButton);
            input.InnerHtml.AppendHtml(labelDiv);

            inputDiv = new TagBuilder("div");
            inputDiv.AddCssClass("col-sm-10");
            return input;
        }

        public static HtmlString FormGroup(this IHtmlHelper html, string labelText, HtmlString content)
        {
            var labelDiv = Tag("div", "col-sm-2", Tag("label", "control-label", labelText));
            var contentDiv = Tag("div", "col-sm-10", content);
            var formGroup = Tag("div", "form-group", labelDiv, contentDiv);

            return formGroup.ToHtmlString();
        }

        public static HtmlString FormGroup(this IHtmlHelper html, IHtmlContent label, IHtmlContent content, string contentId = null)
        {
            var labelDiv = Tag("div", "col-sm-2", label);
            var contentDiv = Tag("div", "col-sm-10", content);  

            if (!string.IsNullOrEmpty(contentId))
                contentDiv.GenerateId(contentId, "_");

            var formGroup = Tag("div", "form-group", labelDiv, contentDiv);

            return formGroup.ToHtmlString();
        }

        private static TagBuilder Tag(string tagName, string cssClass, params object[] contents)
        {
            var tag = new TagBuilder(tagName);
            tag.AddCssClass(cssClass);

            foreach (var content in contents)
            {
                if (content is string)
                    tag.InnerHtml.AppendHtml((string)content);
                else if (content is TagBuilder builder)
                    tag.InnerHtml.AppendHtml(builder);
                else if (content is HtmlString)
                    tag.InnerHtml.AppendHtml((HtmlString)content);
                else if (content is StringHtmlContent)
                    tag.InnerHtml.AppendHtml((StringHtmlContent)content);
                else
                    throw new Exception("Unexpected tag content: " + content);
            }
            
            return tag;
        }

        private static void AppendHtml(this TagBuilder tag, TagBuilder innerTag)
        {
            if (innerTag != null)
                tag.InnerHtml.AppendHtml(innerTag.ToString());
        }

        private static void AppendHtml(this TagBuilder tag, HtmlString innerHtml)
        {
            if (innerHtml != null)
                tag.InnerHtml.AppendHtml(innerHtml.ToString());
        }

        private static void AppendHtml(this TagBuilder tag, string plainText)
        {
            if (plainText != null)
                tag.InnerHtml.AppendHtml(WebUtility.HtmlEncode(plainText));
        }

        private static void Merge(this Dictionary<string, object> htmlAttributes, object overrideHtmlAttributes)
        {
            if (overrideHtmlAttributes != null)
            {
                if (overrideHtmlAttributes is Dictionary<string, object> overrideHtmlDictionary)
                {
                    foreach (var property in overrideHtmlDictionary)
                        htmlAttributes[property.Key] = property.Value;
                }
                else
                    foreach (var property in overrideHtmlAttributes.GetType().GetProperties())
                        htmlAttributes[property.Name] = property.GetValue(overrideHtmlAttributes);
            }
        }

        private static string GetPlaceholderName<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var property = Property.From(expression);
            return property.IsDefined(typeof(DisplayAttribute), true)
                ? property.GetAttribute<DisplayAttribute>().Name
                : property.Name.Humanize(LetterCasing.Title);
        }

        public static HtmlString BootstrapDataIncompatibleWithMetadataWarning(this IHtmlHelper html, string resourcePath)
        {
            var entityName = nameof(BootstrapData).Humanize(LetterCasing.Title);

            return html.Danger(
                $@"You are editing a {entityName} for the Resource ""{resourcePath}"", but it is
                   incompatible with the configured target ODS API version. Since the definition of ""{resourcePath}"" has changed, your {entityName}
                   is out of date.",

                $@"In its current state, this {entityName} cannot be safely used to import data to a target API for this version, since it has an incompatible definition of ""{resourcePath}"".",

                $@"In order to import ""{resourcePath}"" data to the configured target ODS API, update
                   this {entityName} to satisfy the ODS API version's definition of this Resource.");
        }

        public static HtmlString DataMapIncompatibleWithMetadataWarning(this IHtmlHelper html, string resourcePath)
        {
            var entityName = nameof(DataMap).Humanize(LetterCasing.Title);

            return html.Danger(
                $@"You are editing a {entityName} for the Resource ""{resourcePath}"", but it is
                   incompatible with the configured target ODS API version. Since the definition of ""{resourcePath}"" has changed, your {entityName}
                   is out of date.",

                $@"You can continue to edit your {entityName} safely, in terms of the original
                   ""{resourcePath}"" definition you began with, so your work won't be lost. However,
                   this {entityName} cannot be safely used to import data to a target ODS API of this version,
                   since it has an incompatible definition of ""{resourcePath}"".",

                $@"In order to import ""{resourcePath}"" data to the configured target ODS API, create
                   a new {entityName} for the desired Resource.");
        }

        public static HtmlString Info(this IHtmlHelper html, params string[] paragraphs)
            => BootstrapAlert(paragraphs, "info");

        public static HtmlString Warning(this IHtmlHelper html, params string[] paragraphs)
            => BootstrapAlert(paragraphs, "warning");

        public static HtmlString Danger(this IHtmlHelper html, params string[] paragraphs)
            => BootstrapAlert(paragraphs, "danger");

        private static HtmlString BootstrapAlert(string[] paragraphs, string alertType)
        {
            var div = new TagBuilder("div");
            div.AddCssClass($"alert alert-{alertType}");
            div.MergeAttribute("role", "alert");

            foreach (var p in paragraphs)
            {
                var paragraph = new TagBuilder("p");
                paragraph.AddCssClass("lead");
                paragraph.InnerHtml.AppendHtml(p);
                div.InnerHtml.AppendHtml(paragraph);
            }

            return div.ToHtmlString();
        }

        public static HtmlString ToHtmlString(this TagBuilder builder)
        {
            using var writer = new System.IO.StringWriter();
            builder.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return new HtmlString(writer.ToString());
        }

        public static HtmlString Detail<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var row = new TagBuilder("tr");

            var label = new TagBuilder("td");
            label.MergeAttribute("style", "font-weight: bold;");
            label.InnerHtml.AppendHtml(html.LabelFor(expression));

            var value = new TagBuilder("td");
            value.InnerHtml.AppendHtml(html.DisplayTextFor(expression));

            row.InnerHtml.AppendHtml(label);
            row.InnerHtml.AppendHtml(value);

            return row.ToHtmlString();
        }

        public static HtmlString SortableTableHeader(this IHtmlHelper html, string columnHeader, string sortByAttribute)
        {
            var header = new TagBuilder("th");
            header.AddCssClass("sortable");
            header.MergeAttribute("sort-by", sortByAttribute);
            header.MergeAttribute("style", "cursor: pointer");
            header.InnerHtml.AppendHtml(columnHeader);

            var span = new TagBuilder("span");
            span.AddCssClass("glyphicon glyphicon-sort");
            span.MergeAttribute("style", "float: right");

            header.InnerHtml.AppendHtml(span);
            return header.ToHtmlString();
        }
    }
}