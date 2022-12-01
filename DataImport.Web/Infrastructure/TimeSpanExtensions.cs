// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;

namespace DataImport.Web.Infrastructure
{
    public static class TimeSpanExtensions
    {
        public static string ToReadableDuration(this TimeSpan difference)
        {
            var duration = difference.Duration();

            var parts = new[]
                {
                    WithUnits(duration.Days, "day"),
                    WithUnits(duration.Hours, "hour"),
                    WithUnits(duration.Minutes, "minute"),
                    WithUnits(duration.Seconds, "second")
                }
                .Where(x => x != null)
                .ToList();

            switch (parts.Count)
            {
                case 0: return "0 seconds";
                case 1: return parts[0];
                case 2: return parts[0] + " and " + parts[1];
                default:
                    var last = parts.Count - 1;
                    parts[last] = "and " + parts[last];
                    return string.Join(", ", parts);
            }
        }

        private static string WithUnits(int count, string word)
            => count > 0 ? $"{count:0} {word}{(count == 1 ? "" : "s")}" : null;
    }
}
