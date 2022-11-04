// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Share;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Share
{
    public class FileExportImportTests : ExportImportTestBase
    {
        [Test]
        public async Task ShouldPresentBootstrapsAndDataMapsForSelectionWhenExporting()
        {
            var form = await Send(new FileExport.Query());

            AssertBootstrapAndDataMapSelections(form);
        }

        [Test]
        public void ShouldRequireMinimumFieldsWhenExporting()
        {
            new FileExport.Command()
                .ShouldNotValidate(
                    "Please configure Data Import with an ODS API before exporting.",
                    "'Description' must not be empty.",
                    "'Title' must not be empty.");
        }

        [Test]
        public async Task ShouldExportSelectedBootstrapsAndDataMapsAlongWithLookupDependencies()
        {
            var form = await Send(new FileExport.Query());

            MakeExportFormSelections(form);

            var result = await Send(form);

            AssertTemplatePreviewDuringExport(result);
        }

        [Test]
        public async Task ShouldExportMinimalJsonForMissingSelections()
        {
            // ASP.NET MVC Model binding will bind a null for array view models,
            // when there are no items available in the form, instead of binding
            // a more convenient empty array. Here, we prove that null bindings
            // for the arrays of selections are handled correctly.

            var form = new FileExport.Command
            {
                DataMaps = null,
                Bootstraps = null,

                Title = "Test Export",
                Description = "Test export with no bootstrap, data map, or lookup selections.",
                ApiVersion = ApiVersion
            };

            var result = await Send(form);

            AssertMinimalJsonTemplatePreview(result);
        }

        [Test]
        public void ShouldRequireTemplateFileWhenImporting()
        {
            var validation = Validation(new FileImport.FileUploadForm());
            validation.IsValid.ShouldBe(false);
            validation.Errors.Select(x => x.ErrorMessage)
                .ShouldMatch("'Import Template' must not be empty.");
        }

        [Test]
        public async Task ShouldImportDefinitionsFromTemplateFile()
        {
            DataImport.Models.BootstrapData[] expectedBootstraps;
            DataMap[] expectedMaps;
            string template;
            var command = GetImportCommand(out expectedBootstraps, out expectedMaps, out template);

            //Because test setup created all the records exhibited by the template,
            //importing it must fail with clear messages about conflicts.
            AssertImportValidationMessages(command, expectedMaps, expectedBootstraps);

            //Delete the set-up entities to get them out of the way of the import.
            DeleteDuplicateEntities(expectedMaps, expectedBootstraps);

            //Try again, expecting the import to take place.
            var response = await Send(command);
            response.AssertToast($"Template '{command.Import.Title}' was imported.");

            //Export these items, expecting an identical template to the original.
            var form = await Send(new FileExport.Query());
            ConstructExportPreview(response, form);

            var result = await Send(form);
            JObject.Parse(result.Serialize()).ShouldMatch(template);
        }
    }
}
