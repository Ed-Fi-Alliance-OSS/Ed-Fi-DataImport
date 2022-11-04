// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Common;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Web.Tests.Features.Share
{
    using static Testing;

    [TestFixture]
    public class PreprocessorMigrationTests
    {
        [Test]
        public async Task ShouldSuccessfullyMigratePreprocessors()
        {
            await Query(async d =>
            {
                Script script = new Script
                {
                    Name = SampleString("Preprocessor.ps1"),
                    ScriptContent = null,
                    ScriptType = ScriptType.CustomFileGenerator
                };
                d.Scripts.Add(script);
                d.SaveChanges();

                var fileService = A.Fake<IFileService>(x => x.Strict());
                A.CallTo(() => fileService.GetFileGeneratorScript(script.Name)).Returns("Some Content").Once();

                var preprocessorMigration = new PreprocessorMigration(fileService, d);
                preprocessorMigration.CheckIfMigrationNeeded().ShouldBeTrue();

                await preprocessorMigration.Migrate();

                preprocessorMigration.CheckIfMigrationNeeded().ShouldBeFalse();

                var migratedScript = d.Scripts.Single(x => x.Id == script.Id);
                migratedScript.ScriptContent.ShouldBe("Some Content");

                return string.Empty;
            });
        }
    }
}
