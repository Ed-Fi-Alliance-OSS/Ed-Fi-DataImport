// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CsvHelper;
using CsvHelper.Configuration;
using DataImport.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Configuration = CsvHelper.Configuration.CsvConfiguration;

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
            string[] headers = null;
            using (var r = new StreamReader(stream))
            {
                if (isCsv)
                {
                    var configuration = new Configuration(CultureInfo.InvariantCulture)
                    {
                        IgnoreBlankLines = true,
                        TrimOptions = TrimOptions.Trim,
                        ShouldSkipRecord = record => record.Record.All(string.IsNullOrWhiteSpace)
                    };

                    headers = r.ReadLine().Split(",");
                    var containsDuplicates = headers.Count() != headers.Distinct().Count();

                    if (containsDuplicates)
                    {
                        var renamedHeaders = headers.Select((e, i) => new { Item = e, Index = i })
                         .GroupBy(x => x.Item)
                         .SelectMany(g => g.Count() == 1 ?
                                          g.Take(1) :
                                          g.Select((x, i) => new
                                          {
                                              Item = i == 0 ? String.Format("{0}", x.Item) : String.Format("{0}_{1}", x.Item, i + 1),
                                              Index = x.Index
                                          }))
                         .OrderBy(x => x.Index)
                         .Select(x => x.Item)
                         .ToArray();

                        var content = r.ReadToEnd();

                        MemoryStream csvMemoryStream = new MemoryStream();
                        StreamWriter writer = new StreamWriter(csvMemoryStream);
                        writer.WriteLine(String.Join(",", renamedHeaders));
                        writer.WriteLine(content);
                        writer.Flush();
                        csvMemoryStream.Position = 0;

                        using (StreamReader newReader = new StreamReader(csvMemoryStream))
                        {
                            using (var csv = new CsvReader(newReader, configuration))
                            {
                                while (csv.Read())
                                {
                                    var record = csv.GetRecord<object>();
                                    if (record != null)
                                        totalLines++;
                                }
                            }
                            writer.Dispose();
                            csvMemoryStream.Dispose();
                        }
                    }
                    else
                    {
                        using (var csv = new CsvReader(r, configuration))
                        {
                            while (csv.Read())
                            {
                                var record = csv.GetRecord<object>();
                                if (record != null)
                                    totalLines++;
                            }
                        }
                    }
                }
                else
                {
                    while (r.ReadLine() != null) { totalLines++; }
                }

                return totalLines;
            }
        }

        public static bool IsCsvFile(this string fileName) =>
            string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase);

        public static string GetDirectory(this Agent agent) => Path.Combine("DataImport", $"Agent-{agent.Id}");
    }
}
