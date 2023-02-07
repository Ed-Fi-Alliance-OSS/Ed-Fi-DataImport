// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataImport.Common.Enums
{
    public static class LogLevel
    {
        public const string Debug = "DEBUG";
        public const string Information = "INFORMATION";
        public const string Notice = "NOTICE";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Critical = "CRITICAL";

        public static readonly IList<string> All = new ReadOnlyCollection<string>(new List<string> { Debug, Information, Notice, Warning, Error, Critical });

        /// <summary>
        /// Generates the list based on the filter
        /// For example, if filter is set to "WARNING" => you will see "WARNING", "ERROR", and "CRITICAL" 
        /// </summary>
        /// <param name="filter">Valid values Constants LogLevel </param>
        /// <returns>A string list with levels to be applied in the Log</returns>
        public static List<string> GetValidList(string filter = Debug)
        {
            List<string> result = All.ToList();

            if (!string.IsNullOrEmpty(filter) && result.IndexOf(filter.ToUpperInvariant()) != -1)
            {
                result = result.Skip(result.IndexOf(filter.ToUpperInvariant())).ToList();
            }
            return result;
        }
    }
}
