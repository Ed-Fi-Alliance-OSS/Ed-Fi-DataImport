// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Preprocessor;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Shouldly;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    [SetCulture("en-US")]
    [TestFixture]
    internal class UploadCsvTests
    {
        [Test]
        public async Task ShouldReturnErrorMessageIfCsvIdIncorrect()
        {
            var preprocessor = await Send(new AddPreprocessor.Command
            {
                ViewModel = new AddEditPreprocessorViewModel
                {
                    Name = SampleString("Name"),
                    ScriptType = ScriptType.CustomFileProcessor,
                    ScriptContent = "write-output 'header,duplicate_header,duplicate_header'"
                }
            });

            var fileBase = A.Fake<IFormFile>();
            A.CallTo(() => fileBase.OpenReadStream()).Returns("".ToStream());

            var result = await Send(new UploadCsvFile.Command { PreprocessorId = preprocessor.PreprocessorId, FileBase = fileBase });
            result.CsvError.ShouldBe("A column named 'duplicate_header' already belongs to this DataTable.");
        }

        [Test]
        public async Task ShouldReturnErrorMessageIfFileIsEmpty()
        {
            var fileBase = A.Fake<IFormFile>();
            A.CallTo(() => fileBase.OpenReadStream()).Returns("".ToStream());

            var result = await Send(new UploadCsvFile.Command { FileBase = fileBase });
            result.CsvError.ShouldBe("File is empty.");
        }
    }
}
