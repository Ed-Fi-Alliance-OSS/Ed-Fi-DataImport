// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DataImport.Server.TransformLoad.Features.LoadResources;
using DataImport.TestHelpers;
using NUnit.Framework;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    [SetCulture("en-US")]
    public class CsvTabularDataTests
    {
        [Test]
        public void ShouldReadCsvRowsAsDictionaries()
        {
            using (var csvTable = new CsvTabularData(Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv")))
            {
                csvTable.GetRows()
                    .ShouldMatch(
                        new Dictionary<string, string>
                        {
                            { "TEST_YEAR", "2018" },
                            { "DISTRICT_NUM", "255901" },
                            { "DISTRICT_NAME", "Grand Bend ISD" },
                            { "SCHOOL_NUM", "255901107" },
                            { "SCHOOL_NAME", "Grand Bend Elementary School" },
                            { "STUDENT_ID", "604825" },
                            { "OVERALL_SCORE", "283" },
                            { "REG_FEE", "12.34" }
                        },
                        new Dictionary<string, string>
                        {
                            { "TEST_YEAR", "2018" },
                            { "DISTRICT_NUM", "255901" },
                            { "DISTRICT_NAME", "Grand Bend ISD" },
                            { "SCHOOL_NUM", "255901107" },
                            { "SCHOOL_NAME", "Grand Bend Elementary School" },
                            { "STUDENT_ID", "604826" },
                            { "OVERALL_SCORE", "174" },
                            { "REG_FEE", "$10.00" }
                        });
            }
        }

        public static string GetAssemblyPath()
            => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
