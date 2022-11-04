// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using static System.Environment;
using static DataImport.TestHelpers.TestHelpers;

namespace DataImport.TestHelpers
{
    public class MatchException : Exception
    {
        public object Expected { get; }
        public object Actual { get; }

        public MatchException(object expected, object actual)
            : base(
                (Debugger.IsAttached ? "" : $"Run this test under the debugger to launch your diff tool.{NewLine}{NewLine}") +
                $"Expected two objects to match on all properties.{NewLine}{NewLine}" +
                $"Expected:{NewLine}{Json(expected)}{NewLine}{NewLine}" +
                $"Actual:{NewLine}{Json(actual)}")
        {
            Expected = expected;
            Actual = actual;

            if (Debugger.IsAttached)
                LaunchDiffTool();
        }

        private void LaunchDiffTool()
        {
            var environmentVariable = "DataImport.DiffTool";
            var diffTool = GetEnvironmentVariable(environmentVariable);

            if (diffTool == null)
                throw new Exception(
                    "To easily debug MatchException test failures, define environment " +
                    $"variable {environmentVariable} equal to the full path to your diff tool.",
                    this);

            if (!File.Exists(diffTool)) return;

            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            File.WriteAllText(expectedPath, Json(Expected));
            File.WriteAllText(actualPath, Json(Actual));

            using (Process.Start(diffTool, $"{expectedPath} {actualPath}")) { }
        }
    }
}
