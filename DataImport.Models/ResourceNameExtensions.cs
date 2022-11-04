// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using Humanizer;

namespace DataImport.Models
{
    public static class ResourceNameExtensions
    {
        public static string ToResourceName(this Resource resource)
            => resource.Path.ToResourceName();

        public static string ToResourceName(this BootstrapData bootstrapData)
            => bootstrapData.ResourcePath.ToResourceName();

        public static string ToResourceName(this DataMap dataMap)
            => dataMap.ResourcePath.ToResourceName();

        public static string ToResourceName(this string resourcePath)
        {
            var sections = (resourcePath ?? "")
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            string extensionName = null;

            if (sections.Count == 2)
            {
                var prefix = sections.First();
                sections.RemoveAt(0);

                if (prefix != "ed-fi")
                    extensionName = prefix;
            }

            sections = sections.Select(x => x.Titleize()).ToList();

            if (extensionName != null)
                sections.Add("[" + extensionName.Titleize() + "]");

            return string.Join(" ", sections);
        }
    }
}