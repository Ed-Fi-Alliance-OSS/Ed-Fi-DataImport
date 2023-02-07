// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using IngestionLogLevelConstants = DataImport.Common.Constants.IngestionLogLevel;

namespace DataImport.Common.Tests
{
    [TestFixture]
    internal class ConstantsTests
    {
        [Test]
        public void ShouldReturnIngestionLogLevelsFiltered()
        {
            List<string> expected = new List<string>() { IngestionLogLevelConstants.Error, IngestionLogLevelConstants.Critical };
            List<string> actual = IngestionLogLevelConstants.GetValidList(IngestionLogLevelConstants.Error);
            Console.WriteLine($"Expected: {string.Join(",", expected)}");
            Console.WriteLine($"Actual: {string.Join(",", actual)}");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [Test]
        public void ShouldReturnIngestionLogLevelsIfFilterIsEmpty()
        {
            List<string> expected = IngestionLogLevelConstants.All.ToList();
            List<string> actual = IngestionLogLevelConstants.GetValidList("");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [Test]
        public void ShouldReturnIngestionLogLevelsIfFilterIsNotFound()
        {
            List<string> expected = IngestionLogLevelConstants.All.ToList();
            List<string> actual = IngestionLogLevelConstants.GetValidList("Test");
            Assert.IsTrue(actual.SequenceEqual(expected));
        }
    }
}
