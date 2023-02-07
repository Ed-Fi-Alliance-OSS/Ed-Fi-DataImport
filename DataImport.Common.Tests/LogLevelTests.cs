// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using LogLevelConstants = DataImport.Common.Enums.LogLevel;

namespace DataImport.Common.Tests
{
    [TestFixture]
    internal class LogLevelTests
    {
        [Test]
        public void ShouldReturnLogLevelsFiltered()
        {
            List<string> expected = new List<string>() { LogLevelConstants.Error, LogLevelConstants.Critical };
            List<string> actual = LogLevelConstants.GetValidList(LogLevelConstants.Error);
            Console.WriteLine($"Expected: {string.Join(",", expected)}");
            Console.WriteLine($"Actual: {string.Join(",", actual)}");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [Test]
        public void ShouldReturnLogLevelsIfFilterIsEmpty()
        {
            List<string> expected = LogLevelConstants.All.ToList();
            List<string> actual = LogLevelConstants.GetValidList("");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [Test]
        public void ShouldReturnLogLevelsIfFilterIsNotFound()
        {
            List<string> expected = LogLevelConstants.All.ToList();
            List<string> actual = LogLevelConstants.GetValidList("Test");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }
    }
}
