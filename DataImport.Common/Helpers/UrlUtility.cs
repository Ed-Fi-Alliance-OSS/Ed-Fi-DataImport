// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace DataImport.Common.Helpers
{
    public static class UrlUtility
    {
        public static string RemoveAfterLastInstanceOf(string text, string textToRemove)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var charLocation = text.LastIndexOf(textToRemove, StringComparison.Ordinal);

            return charLocation > 0
                ? text.Substring(0, charLocation)
                : string.Empty;
        }

        public static string ConvertLocalPathToUri(string localPath)
        {
            return new Uri(localPath).AbsoluteUri;
        }

        public static string CombineUri(params string[] uriParts)
        {
            string uri = string.Empty;
            if (uriParts != null && uriParts.Length > 0)
            {
                char[] trims = { '\\', '/' };
                uri = (uriParts[0] ?? string.Empty).TrimEnd(trims);
                for (int i = 1; i < uriParts.Length; i++)
                {
                    uri = $"{uri.TrimEnd(trims)}/{(uriParts[i] ?? string.Empty).TrimStart(trims)}";
                }
            }
            return uri;
        }
    }
}
