// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text.RegularExpressions;

namespace DataImport.Common.ExtensionMethods
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replace the * with an .* and the ? with a dot. Put ^ at the beginning and a $ at the end
        /// </summary>
        /// <param name="value"></param>
        /// <param name="textWithWildCard"></param>
        /// <returns></returns>
        public static bool IsLike(this string value, string textWithWildCard)
        {
            var pattern = "^" + Regex.Escape(textWithWildCard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(value);
        }
    }
}
