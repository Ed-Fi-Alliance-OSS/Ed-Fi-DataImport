// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace DataImport.Common.ExtensionMethods
{
    public static class FileExtensions
    {
        private static IOptions<ConnectionStrings> ConnectionStringsOptions => _connectionStringsOptions;
        private static IOptions<ConnectionStrings> _connectionStringsOptions;

        public static void SetConnectionStringsOptions(IOptions<ConnectionStrings> options)
        {
            if (_connectionStringsOptions == default(IOptions<ConnectionStrings>))
                _connectionStringsOptions = options;
            else
            {
                // this is to make sure that the global state isn't changed while running.
                throw new NotSupportedException("ConnectionStringOptions may not be set more than once.");
            }
        }

        public static int TotalLines(this Stream stream, bool isCsv)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var totalLines = 0;
            using (var r = new StreamReader(stream))
            {
                if (isCsv)
                {
                    while (!IsEmptyRecord(r.ReadLine()))
                    {
                        totalLines++;
                    }
                    totalLines--;
                }
                else
                {
                    while (r.ReadLine() != null) { totalLines++; }
                }
            }
            return totalLines;
        }

        private static bool IsEmptyRecord(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return true;
            var array = s.Split(',');
            return array.All(x => String.IsNullOrWhiteSpace(x));
        }

        public static bool IsCsvFile(this string fileName) =>
            string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase);

        public static string GetDirectory(this Agent agent) => Path.Combine("DataImport", $"Agent-{agent.Id}");
    }
}
