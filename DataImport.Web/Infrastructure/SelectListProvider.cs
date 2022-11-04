// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Web.Infrastructure
{
    public static class SelectListProvider
    {
        public static List<SelectListItem> ToSelectListItems<T>(
            this IReadOnlyList<T> items,
            string emptyValueText = null,
            Func<T, string> getValue = null,
            Func<T, string> getText = null,
            Func<T, SelectListGroup> getGroup = null)
        {
            if (getValue == null)
                getValue = x => x.ToString();

            if (getText == null)
                getText = getValue;

            var result = new List<SelectListItem>();

            if (emptyValueText != null)
                result.Add(new SelectListItem { Text = emptyValueText, Value = "" });

            result.AddRange(items.Select(x => new SelectListItem
            {
                Text = getText(x),
                Value = getValue(x),
                Group = getGroup?.Invoke(x)
            }));

            return result;
        }
    }
}