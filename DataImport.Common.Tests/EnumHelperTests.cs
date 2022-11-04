// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Models;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Common.Tests
{
    [TestFixture]
    internal class EnumHelperTests
    {
        [Test]
        public void ShouldCorrectlyReturnEnumDescriptionOrEnumValue()
        {
            // Test enum with description attribute
            EnumHelper.GetEnumDescription(ScriptType.CustomFileGenerator).ShouldBe("Custom File Generator (PowerShell)");
            // Test enum without description attribute
            EnumHelper.GetEnumDescription(FileStatus.ErrorLoading).ShouldBe("ErrorLoading");
        }
    }
}
