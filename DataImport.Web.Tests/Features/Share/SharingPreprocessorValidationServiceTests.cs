// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Preprocessor;
using DataImport.Web.Features.Share;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Web.Tests.Features.Share
{
    using static Testing;

    [TestFixture]
    public class SharingPreprocessorValidationServiceTests
    {
        private string _template = String.Empty;
        private ApiVersion _apiVersion = null;
        private Resource _existingResource = null;

        [SetUp]
        public void Init()
        {
            _apiVersion = GetDefaultApiVersion();
            _existingResource = RandomResource(_apiVersion.Id);
            string preprocessorName = SampleString();
            _template = $@"{{
        ""maps"": [
            {{
                ""name"": ""Test Map-D03A81F5-3CB4-4107-97FB-9D84FAA1496E"",
                ""resourcePath"": ""{_existingResource.Path}"",
                ""columnHeaders"": [
                    ""Header1,Header2""
                ],
                ""map"": {{}},
                ""customFileProcessor"": ""{preprocessorName}""                
            }}
        ],
        ""bootstraps"": [],
        ""lookups"": [],
        ""supplementalInformation"": null,
        ""preprocessors"": [
            {{
                ""name"": ""{preprocessorName}"",
                ""scriptContent"": ""Write-Output \""Header1,Header2\"""",
                ""requireOdsApiAccess"": false,
                ""hasAttribute"": false
            }}
        ]
    }}";
        }

        [Test]
        public async Task ShouldReturnConflictingPreprocessors()
        {
            await With<SharingPreprocessorValidationService>(async service =>
                {
                    var form = new FileImport.Form
                    {
                        Template = _template,
                        Title = SampleString("Shared Template"),
                        Description = SampleString("Shared Template Description"),
                        ApiVersion = _apiVersion.Version,
                        OverwriteExistingPreprocessors = false
                    };

                    Validation(form).ShouldBeSuccessful();
                    var sharingModel = form.AsCommand().Import;
                    var sharingPreprocessor = sharingModel.Template.Preprocessors.First();
                    service.HasConflictingPreprocessors(sharingModel, out var conflictingPreprocessors).ShouldBeFalse("Should not have conflicts if the preprocessor is new to the DI instance.");
                    conflictingPreprocessors.ShouldBeEmpty();

                    // Add preprocessor
                    var viewModel = new AddEditPreprocessorViewModel
                    {
                        Name = sharingPreprocessor.Name,
                        ScriptType = ScriptType.CustomFileProcessor,
                        ScriptContent = sharingPreprocessor.ScriptContent,
                    };
                    var addPreprocessorResponse = await Send(new AddPreprocessor.Command { ViewModel = viewModel });
                    Validation(form).ShouldBeSuccessful();
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeFalse("Should not have conflicts as the preprocessor did not change");
                    conflictingPreprocessors.ShouldBeEmpty();

                    // Make form invalid
                    form.ApiVersion = null;
                    Validation(form).ShouldBeFailure("'Ed-Fi ODS / API Version' must not be empty.");
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeFalse("Should not have conflicts as the preprocessor did not change");
                    conflictingPreprocessors.ShouldBeEmpty();

                    // Correct the ApiVersion, but introduce a change to the preprocessor
                    form.ApiVersion = _apiVersion.Version;
                    sharingPreprocessor.RequireOdsApiAccess = true;
                    Validation(form).ShouldBeFailure($"The existing preprocessor '{sharingPreprocessor.Name}' differs from the one to be imported.");
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeTrue("Should return true since there is a conflicting preprocessor change.");
                    conflictingPreprocessors.Count.ShouldBe(1);

                    // Make the form valid by opting into overwriting the preprocessor.
                    form.OverwriteExistingPreprocessors = true;
                    Validation(form).ShouldBeSuccessful();
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeTrue("Should still return true because although the shared model does not have any validation errors, there are still preprocessor conflicts.");
                    conflictingPreprocessors.Count.ShouldBe(1);

                    var existingResource = RandomResource(_apiVersion.Id);
                    var trivialMappings = await TrivialMappings(existingResource);
                    var mapName = SampleString();
                    var addDataMapResponse = await Send(new AddDataMap.Command
                    {
                        ApiVersionId = existingResource.ApiVersionId,
                        MapName = mapName,
                        ResourcePath = existingResource.Path,
                        Mappings = trivialMappings,
                        PreprocessorId = addPreprocessorResponse.PreprocessorId
                    });
                    sharingPreprocessor.HasAttribute = true; // This is a destructive update to the existing data map.
                    sharingModel.Template.Maps[0].Attribute = SampleString();
                    Validation(form).ShouldBeFailure($"Updating the exiting preprocessor '{sharingPreprocessor.Name}' will break the following data maps due to a required Attribute field: {mapName}.");
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeTrue("Should return true because there is still one conflicting preprocessor.");
                    conflictingPreprocessors.Count.ShouldBe(1);

                    var editDataMapVm = await Send(new EditDataMap.Query
                    {
                        Id = addDataMapResponse.DataMapId
                    });
                    editDataMapVm.Attribute = SampleString();
                    await Send(new EditDataMap.Command
                    {
                        PreprocessorId = editDataMapVm.PreprocessorId,
                        Attribute = SampleString(),
                        ColumnHeaders = editDataMapVm.ColumnHeaders,
                        DataMapId = addDataMapResponse.DataMapId,
                        MapName = editDataMapVm.MapName,
                        ResourcePath = editDataMapVm.ResourcePath,
                        Mappings = trivialMappings
                    });
                    Validation(form).ShouldBeSuccessful();
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeTrue("Should return True because will not break the data map.");
                    conflictingPreprocessors.Count.ShouldBe(1);

                    sharingPreprocessor.ScriptContent = "Write-Output Column2";
                    sharingPreprocessor.HasAttribute = false;
                    Validation(form).ShouldBeSuccessful();
                    service.HasConflictingPreprocessors(sharingModel, out conflictingPreprocessors).ShouldBeTrue("A change to script content might break existing data maps if one or more columns were removed from the CSV output produced by the script. By design we should allow importing such preprocessors.");
                    conflictingPreprocessors.Count.ShouldBe(1);
                }
            );
        }
    }
}