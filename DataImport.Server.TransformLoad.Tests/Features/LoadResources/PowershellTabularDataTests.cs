// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;
using DataImport.Server.TransformLoad.Features.LoadResources;
using DataImport.TestHelpers;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    public class PowershellTabularDataTests
    {
        private PowerShellPreprocessorService _service;

        private const string Script = @"
param ($row)

function ReformatDate($value, $from, $to) {
    $invariant = [System.Globalization.CultureInfo]::InvariantCulture
    return [DateTime]::ParseExact($value, $from, $invariant).ToString($to, $invariant)
}

$row.'ColumnToBeTrimmed' = $row.'ColumnToBeTrimmed'.Trim()
$row.'Date' = ReformatDate $row.'Date' -from 'M/d' -to '2019-MM-dd'

$row.'Concatenated' = $row.'Column1' + ':' + $row.'Column2'
$row.'Integer' = [int]123";

        [SetUp]
        public void Setup()
        {
            var oAuthRequestWrapper = A.Fake<IOAuthRequestWrapper>();
            A.CallTo(() => oAuthRequestWrapper.GetAccessCode(null, null)).WithAnyArguments().Returns("fake token");
            A.CallTo(() => oAuthRequestWrapper.GetBearerToken(null, null)).WithAnyArguments().Returns("fake token");
            A.CallTo(() => oAuthRequestWrapper.GetBearerToken(null, null, null)).WithAnyArguments().Returns("fake token");

            var appSettings = new AppSettings { EncryptionKey = Guid.NewGuid().ToString() };
            _service = new PowerShellPreprocessorService(appSettings, new PowerShellPreprocessorOptions(), oAuthRequestWrapper);
        }

        [Test]
        public void ShouldTransformUnderlyingTabularData()
        {
            var originalRow1 = new Dictionary<string, string>
            {
                { "ColumnToBeTrimmed", " Value " },
                { "Column1", "A" },
                { "Column2", "B" },
                { "Date", "1/1" }
            };

            var originalRow2 = new Dictionary<string, string>
            {
                { "ColumnToBeTrimmed", "  Value  " },
                { "Column1", "C" },
                { "Column2", "D" },
                { "Date", "12/31" }
            };

            var expectedTransformedRow1 = new Dictionary<string, string>
            {
                { "ColumnToBeTrimmed", "Value" },
                { "Column1", "A" },
                { "Column2", "B" },
                { "Date", "2019-01-01" },
                { "Concatenated", "A:B" },
                { "Integer", "123" }
            };

            var expectedTransformedRow2 = new Dictionary<string, string>
            {
                { "ColumnToBeTrimmed", "Value" },
                { "Column1", "C" },
                { "Column2", "D" },
                { "Date", "2019-12-31" },
                { "Concatenated", "C:D" },
                { "Integer", "123" }
            };

            var originalRows = new StubTabularData(originalRow1, originalRow2);

            originalRows.Disposed.ShouldBe(false);

            using (var powershellTable = new PowershellTabularData(_service, Script, originalRows))
            {
                powershellTable.GetRows()
                    .ShouldMatch(expectedTransformedRow1, expectedTransformedRow2);
            }

            //The original row Dictionaries are in fact mutated in place.
            //Powershell is operating on the original, rather than receiving
            //a serialized copy. Since ITabularData are single-pass and lazy
            //anyway, this is ok. For large inputs, it's also efficient.

            originalRow1.ShouldMatch(expectedTransformedRow1);
            originalRow2.ShouldMatch(expectedTransformedRow2);

            originalRows.Disposed.ShouldBe(true);
        }
    }
}
