// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Share;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Share
{
    public abstract class ExportImportTestBase
    {
        protected string ApiVersion;
        protected string AdditionalApiVersion;
        protected Resource Resource0;
        protected Resource Resource1;
        protected Resource Resource2;
        protected Resource Resource3;
        protected Resource ResourceWithAdditionalApiVersion;

        protected DataImport.Models.BootstrapData BootstrapData0;
        protected DataImport.Models.BootstrapData BootstrapData1;
        protected DataImport.Models.BootstrapData BootstrapData2;
        protected DataImport.Models.BootstrapData BootstrapDataWithAdditionalApiVersion;

        public Script MapPreprocessorPowerShell { get; set; }
        public Script MapPreprocessorExternal { get; set; }
        public string MapAttribute { get; set; }

        protected DataMap DataMap0;
        protected DataMap DataMap1;
        protected DataMap DataMap2;
        protected DataMap DataMapWithExternalPreprocessor;
        protected DataMap DataMapWithAdditionalApiVersion;
        protected string SourceTable0;
        protected string SourceTable1;
        private string _sourceTable2;
        private bool _alternator;

        [SetUp]
        public async Task SetUp()
        {
            await AddApiServer(StubSwaggerWebClient.ApiServerUrlV311, OdsApiV311);
            await AddApiServer(StubSwaggerWebClient.ApiServerUrlV25, OdsApiV25);

            ApiVersion = OdsApiV311;
            AdditionalApiVersion = OdsApiV25;


            // These tests deal with mappings which use two lookups, so the only
            // usable resources for these tests are those which have at least two
            // mappable columns. The simplest way to find resources which support
            // that bare minimum of complexity is to search for resources with
            // at least two top-level non-array properties. Most resources do meet
            // this requirement.
            var usableResources = Query(x => x.Resources.Include(y => y.ApiVersion).ToArray())
                .Where(x =>
                {
                    var metadata = ResourceMetadata.DeserializeFrom(x);

                    var hasTwoOrMoreTopLevelNonArrayProperties =
                        metadata.Count(m => m.DataType != "array") >= 2;

                    return hasTwoOrMoreTopLevelNonArrayProperties;
                })
                .ToArray();

            var resourcesWithAdditionalApiVersion = usableResources.Where(x => x.ApiVersion.Version == AdditionalApiVersion).ToArray();
            usableResources = usableResources.Where(x => x.ApiVersion.Version == ApiVersion).ToArray();

            Resource0 = RandomItem(usableResources);
            Resource1 = RandomItem(usableResources);
            Resource2 = RandomItem(usableResources);
            Resource3 = RandomItem(usableResources);
            ResourceWithAdditionalApiVersion = RandomItem(resourcesWithAdditionalApiVersion);

            BootstrapData0 = await AddBootstrapData(Resource0);
            BootstrapData1 = await AddBootstrapData(Resource1);
            BootstrapData2 = await AddBootstrapData(Resource2, data: new JObject());
            BootstrapDataWithAdditionalApiVersion = await AddBootstrapData(ResourceWithAdditionalApiVersion);

            SourceTable0 = SampleString("SourceTable1");
            SourceTable1 = SampleString("SourceTable2");
            _sourceTable2 = SampleString("SourceTable3");

            await AddLookup(SourceTable0, "Key A", "Value A");
            await AddLookup(SourceTable0, "Key B", "Value B");
            await AddLookup(SourceTable1, "Key C", "Value C");
            await AddLookup(SourceTable1, "Key D", "Value D");
            await AddLookup(_sourceTable2, "Never Used Key E", "Never Used Value E");

            var mappings = await TrivialMappings(Resource2);
            SelectLookupTables(mappings, ResourceMetadata.DeserializeFrom(Resource2));

            var columnHeaders = new[] { "Csv Column A", "Csv Column B", "Csv Column C" };
            DataMap0 = await AddDataMap(Resource0, columnHeaders);
            MapPreprocessorPowerShell = await AddPreprocessor(ScriptType.CustomFileProcessor, true);
            MapPreprocessorExternal = await AddPreprocessor(ScriptType.ExternalFileProcessor, true);
            MapAttribute = "CAASPP";
            DataMap1 = await AddDataMap(Resource1, columnHeaders, null, MapPreprocessorPowerShell.Id, MapAttribute);
            DataMap2 = await AddDataMap(Resource2, columnHeaders, mappings);
            DataMapWithExternalPreprocessor = await AddDataMap(Resource3, columnHeaders, null, MapPreprocessorExternal.Id);
            DataMapWithAdditionalApiVersion = await AddDataMap(ResourceWithAdditionalApiVersion, columnHeaders);
        }


        private void SelectLookupTables(IReadOnlyList<DataMapper> mappings, IReadOnlyList<ResourceMetadata> metadatas)
        {
            foreach (var mapping in mappings)
            {
                var metadata = metadatas.Single(m => m.Name == mapping.Name);

                if (metadata.DataType == "array")
                    continue;

                if (mapping.Children.Any())
                {
                    SelectLookupTables(mapping.Children, metadata.Children);
                }
                else
                {
                    mapping.SourceColumn = "Csv Column A";
                    mapping.SourceTable = _alternator ? SourceTable0 : SourceTable1;
                    _alternator = !_alternator;
                }
            }
        }

        protected void AssertBootstrapAndDataMapSelections(ExportBase.CommandBase form)
        {
            form.Bootstraps.Length.ShouldBe(Count<DataImport.Models.BootstrapData>());
            var expectedBootstraps = new[] { BootstrapData0.Id, BootstrapData1.Id, BootstrapData2.Id, BootstrapDataWithAdditionalApiVersion.Id };
            form.Bootstraps.Where(x => expectedBootstraps.Contains(x.Id)).OrderBy(x => x.Id)
                    .ShouldMatch(new ExportBase.BootstrapSelection
                    {
                        Id = BootstrapData0.Id,
                        Name = BootstrapData0.Name,
                        ResourcePath = Resource0.Path,
                        Selected = false,
                        ApiVersion = Resource0.ApiVersion.Version
                    },
                    new ExportBase.BootstrapSelection
                    {
                        Id = BootstrapData1.Id,
                        Name = BootstrapData1.Name,
                        ResourcePath = Resource1.Path,
                        Selected = false,
                        ApiVersion = Resource1.ApiVersion.Version
                    },
                    new ExportBase.BootstrapSelection
                    {
                        Id = BootstrapData2.Id,
                        Name = BootstrapData2.Name,
                        ResourcePath = Resource2.Path,
                        Selected = false,
                        ApiVersion = Resource2.ApiVersion.Version
                    },
                    new ExportBase.BootstrapSelection
                    {
                        Id = BootstrapDataWithAdditionalApiVersion.Id,
                        Name = BootstrapDataWithAdditionalApiVersion.Name,
                        ResourcePath = ResourceWithAdditionalApiVersion.Path,
                        Selected = false,
                        ApiVersion = ResourceWithAdditionalApiVersion.ApiVersion.Version
                    });

            var expectedMaps = new[]
            {
                new ExportBase.DataMapSelection
                {
                    Id = DataMap0.Id,
                    Name = DataMap0.Name,
                    ResourcePath = Resource0.Path,
                    Lookups = "",
                    Selected = false,
                    ApiVersion = Resource0.ApiVersion.Version
                },
                new ExportBase.DataMapSelection
                {
                    Id = DataMap1.Id,
                    Name = DataMap1.Name,
                    ResourcePath = Resource1.Path,
                    Lookups = "",
                    Selected = false,
                    ApiVersion = Resource1.ApiVersion.Version,
                    CustomFileProcessor = MapPreprocessorPowerShell.Name
                },
                new ExportBase.DataMapSelection
                {
                    Id = DataMap2.Id,
                    Name = DataMap2.Name,
                    ResourcePath = Resource2.Path,
                    Lookups = $"{SourceTable0}, {SourceTable1}",
                    Selected = false,
                    ApiVersion = Resource2.ApiVersion.Version
                },
                new ExportBase.DataMapSelection
                {
                    Id = DataMapWithExternalPreprocessor.Id,
                    Name = DataMapWithExternalPreprocessor.Name,
                    ResourcePath = Resource3.Path,
                    Lookups = "",
                    Selected = false,
                    ApiVersion = Resource3.ApiVersion.Version,
                    CustomFileProcessor = MapPreprocessorExternal.Name
                },
                new ExportBase.DataMapSelection
                {
                    Id = DataMapWithAdditionalApiVersion.Id,
                    Name = DataMapWithAdditionalApiVersion.Name,
                    ResourcePath = ResourceWithAdditionalApiVersion.Path,
                    Lookups = "",
                    Selected = false,
                    ApiVersion = ResourceWithAdditionalApiVersion.ApiVersion.Version
                }
            }.OrderBy(x => x.Name).ToArray();

            form.DataMaps.Length.ShouldBe(Count<DataMap>());
            form.DataMaps
                .Where(x => x.Id == DataMap0.Id || x.Id == DataMap1.Id || x.Id == DataMap2.Id || x.Id == DataMapWithExternalPreprocessor.Id || x.Id == DataMapWithAdditionalApiVersion.Id)
                .ShouldMatch(expectedMaps);
        }

        protected void MakeExportFormSelections(ExportBase.CommandBase form)
        {
            //User select bootstraps 1 and 3.
            form.Bootstraps.Single(x => x.Id == BootstrapData0.Id).Selected = true;
            form.Bootstraps.Single(x => x.Id == BootstrapData2.Id).Selected = true;

            //User selects data maps 2, 3, and external preprocessor.
            form.DataMaps.Single(x => x.Id == DataMap1.Id).Selected = true;
            form.DataMaps.Single(x => x.Id == DataMap2.Id).Selected = true;
            form.DataMaps.Single(x => x.Id == DataMapWithExternalPreprocessor.Id).Selected = true;

            //User finalizes export form.
            form.Title = "Test Template";
            form.ApiVersion = form.Bootstraps.Single(x => x.Id == BootstrapData0.Id).ApiVersion;
            form.Description = "Test template with partial bootstrap, data map, and lookup selections.";
        }

        private string GetNameOfExpectedPreprocessorForMap(DataMap map)
        {
            if (!map.FileProcessorScriptId.HasValue) return "null";
            var name = map.Name == DataMapWithExternalPreprocessor.Name
                ? MapPreprocessorExternal.Name
                : MapPreprocessorPowerShell.Name;

            return $"\"{name}\"";
        }

        protected void AssertTemplatePreviewDuringExport(SharingModel result, string expectedSupplementalInformation = null)
        {
            var expectedBootstraps = new[] { BootstrapData0, BootstrapData2 }.OrderBy(x => x.Name).ToArray();
            var expectedMaps = new[] { DataMap1, DataMap2, DataMapWithExternalPreprocessor }.OrderBy(x => x.Name).ToArray();
            var expectedSupplementalInformationLiteral = expectedSupplementalInformation == null
                ? "null"
                : $"\"{expectedSupplementalInformation}\"";

            var expectedPreprocessors = new[] { MapPreprocessorPowerShell, MapPreprocessorExternal }.OrderBy(x => x.Name).ToArray();

            JObject.Parse(result.Serialize()).ShouldMatch($@"{{
                ""title"": ""Test Template"",
                ""description"": ""Test template with partial bootstrap, data map, and lookup selections."",
                ""apiVersion"": ""{ApiVersion}"",
                ""template"": {{
                    ""maps"": [
                        {{
                            ""name"": ""{expectedMaps[0].Name}"",
                            ""resourcePath"": ""{expectedMaps[0].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[0].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[0])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[0].Attribute) ? "null" : $"\"{expectedMaps[0].Attribute}\"")}
                        }},
                        {{
                            ""name"": ""{expectedMaps[1].Name}"",
                            ""resourcePath"": ""{expectedMaps[1].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[1].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[1])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[1].Attribute) ? "null" : $"\"{expectedMaps[1].Attribute}\"")}
                        }},
                        {{
                            ""name"": ""{expectedMaps[2].Name}"",
                            ""resourcePath"": ""{expectedMaps[2].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[2].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[2])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[2].Attribute) ? "null" : $"\"{expectedMaps[2].Attribute}\"")}
                        }},
                    ],
                    ""bootstraps"": [
                        {{
                            ""name"": ""{expectedBootstraps[0].Name}"",
                            ""resourcePath"": ""{expectedBootstraps[0].ResourcePath}"",
                            ""data"": {expectedBootstraps[0].Data}
                        }},
                        {{
                            ""name"": ""{expectedBootstraps[1].Name}"",
                            ""resourcePath"": ""{expectedBootstraps[1].ResourcePath}"",
                            ""data"": {expectedBootstraps[1].Data}
                        }}
                    ],
                    ""lookups"": [
                        {{
                          ""sourceTable"": ""{SourceTable0}"",
                          ""key"": ""Key A"",
                          ""value"": ""Value A"",
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable0}"",
                          ""key"": ""Key B"",
                          ""value"": ""Value B""
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable1}"",
                          ""key"": ""Key C"",
                          ""value"": ""Value C""
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable1}"",
                          ""key"": ""Key D"",
                          ""value"": ""Value D""
                        }}
                    ],
                    ""supplementalInformation"": {expectedSupplementalInformationLiteral},
                    ""preprocessors"": [
                        {{
                            ""name"": ""{expectedPreprocessors[0].Name}"",
                            ""scriptContent"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ScriptContent) ? "null" : $"\"{expectedPreprocessors[0].ScriptContent}\"")},
                            ""requireOdsApiAccess"": {expectedPreprocessors[0].RequireOdsApiAccess.ToString().ToLowerInvariant()},
                            ""hasAttribute"": {expectedPreprocessors[0].HasAttribute.ToString().ToLowerInvariant()},
                            ""executablePath"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ExecutablePath) ? "null" : $"\"{expectedPreprocessors[0].ExecutablePath}\"")},
                            ""executableArguments"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ExecutableArguments) ? "null" : $"\"{expectedPreprocessors[0].ExecutableArguments}\"")}
                        }},
                        {{
                            ""name"": ""{expectedPreprocessors[1].Name}"",
                            ""scriptContent"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ScriptContent) ? "null" : $"\"{expectedPreprocessors[1].ScriptContent}\"")},
                            ""requireOdsApiAccess"": {expectedPreprocessors[1].RequireOdsApiAccess.ToString().ToLowerInvariant()},
                            ""hasAttribute"": {expectedPreprocessors[1].HasAttribute.ToString().ToLowerInvariant()},
                            ""executablePath"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ExecutablePath) ? "null" : $"\"{expectedPreprocessors[1].ExecutablePath}\"")},
                            ""executableArguments"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ExecutableArguments) ? "null" : $"\"{expectedPreprocessors[1].ExecutableArguments}\"")}
                        }}
                    ]
                }}
            }}");
        }

        protected void AssertMinimalJsonTemplatePreview(SharingModel result)
        {
            JObject.Parse(result.Serialize()).ShouldMatch($@"{{
                ""title"": ""Test Export"",
                ""description"": ""Test export with no bootstrap, data map, or lookup selections."",
                ""apiVersion"": ""{ApiVersion}"",
                ""template"": {{
                    ""maps"": [
                    ],
                    ""bootstraps"": [
                    ],
                    ""lookups"": [
                    ],
                    ""supplementalInformation"": null,
                    ""preprocessors"": []
                }}
            }}");
        }

        protected ImportBase.Command GetImportCommand(out DataImport.Models.BootstrapData[] expectedBootstraps, out DataMap[] expectedMaps,
            out string template, string supplementalInformation = null)
        {
            expectedBootstraps = new[] { BootstrapData0, BootstrapData2 }.OrderBy(x => x.Name).ToArray();
            expectedMaps = new[] { DataMap1, DataMap2, DataMapWithExternalPreprocessor }.OrderBy(x => x.Name).ToArray();

            var expectedPreprocessors = new[] { MapPreprocessorPowerShell, MapPreprocessorExternal }.OrderBy(x => x.Name).ToArray();

            var supplementalInformationLiteral = supplementalInformation == null
                ? "null"
                : $"\"{supplementalInformation}\"";

            template = $@"{{
                ""title"": ""Test Template"",
                ""description"": ""Test template with partial bootstrap, data map, and lookup selections."",
                ""apiVersion"": ""{ApiVersion}"",
                ""template"": {{
                    ""maps"": [
                        {{
                            ""name"": ""{expectedMaps[0].Name}"",
                            ""resourcePath"": ""{expectedMaps[0].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[0].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[0])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[0].Attribute) ? "null" : $"\"{expectedMaps[0].Attribute}\"")}
                        }},
                        {{
                            ""name"": ""{expectedMaps[1].Name}"",
                            ""resourcePath"": ""{expectedMaps[1].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[1].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[1])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[1].Attribute) ? "null" : $"\"{expectedMaps[1].Attribute}\"")}
                        }},
                        {{
                            ""name"": ""{expectedMaps[2].Name}"",
                            ""resourcePath"": ""{expectedMaps[2].ResourcePath}"",
                            ""columnHeaders"": [
                               ""Csv Column A"",
                               ""Csv Column B"",
                               ""Csv Column C""
                            ],
                            ""map"": {expectedMaps[2].Map},
                            ""customFileProcessor"": {GetNameOfExpectedPreprocessorForMap(expectedMaps[2])},
                            ""attribute"": {(string.IsNullOrEmpty(expectedMaps[2].Attribute) ? "null" : $"\"{expectedMaps[2].Attribute}\"")}
                        }},
                    ],
                    ""bootstraps"": [
                        {{
                            ""name"": ""{expectedBootstraps[0].Name}"",
                            ""resourcePath"": ""{expectedBootstraps[0].ResourcePath}"",
                            ""data"": {expectedBootstraps[0].Data}
                        }},
                        {{
                            ""name"": ""{expectedBootstraps[1].Name}"",
                            ""resourcePath"": ""{expectedBootstraps[1].ResourcePath}"",
                            ""data"": {expectedBootstraps[1].Data}
                        }}
                    ],
                    ""lookups"": [
                        {{
                          ""sourceTable"": ""{SourceTable0}"",
                          ""key"": ""Key A"",
                          ""value"": ""Value A"",
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable0}"",
                          ""key"": ""Key B"",
                          ""value"": ""Value B""
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable1}"",
                          ""key"": ""Key C"",
                          ""value"": ""Value C""
                        }},
                        {{
                          ""sourceTable"": ""{SourceTable1}"",
                          ""key"": ""Key D"",
                          ""value"": ""Value D""
                        }}
                    ],
                    ""supplementalInformation"": {supplementalInformationLiteral},
                    ""preprocessors"": [
                        {{
                            ""name"": ""{expectedPreprocessors[0].Name}"",
                            ""scriptContent"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ScriptContent) ? "null" : $"\"{expectedPreprocessors[0].ScriptContent}\"")},
                            ""requireOdsApiAccess"": {expectedPreprocessors[0].RequireOdsApiAccess.ToString().ToLowerInvariant()},
                            ""hasAttribute"": {expectedPreprocessors[0].HasAttribute.ToString().ToLowerInvariant()},
                            ""executablePath"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ExecutablePath) ? "null" : $"\"{expectedPreprocessors[0].ExecutablePath}\"")},
                            ""executableArguments"": {(string.IsNullOrEmpty(expectedPreprocessors[0].ExecutableArguments) ? "null" : $"\"{expectedPreprocessors[0].ExecutableArguments}\"")}
                        }},
                        {{
                            ""name"": ""{expectedPreprocessors[1].Name}"",
                            ""scriptContent"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ScriptContent) ? "null" : $"\"{expectedPreprocessors[1].ScriptContent}\"")},
                            ""requireOdsApiAccess"": {expectedPreprocessors[1].RequireOdsApiAccess.ToString().ToLowerInvariant()},
                            ""hasAttribute"": {expectedPreprocessors[1].HasAttribute.ToString().ToLowerInvariant()},
                            ""executablePath"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ExecutablePath) ? "null" : $"\"{expectedPreprocessors[1].ExecutablePath}\"")},
                            ""executableArguments"": {(string.IsNullOrEmpty(expectedPreprocessors[1].ExecutableArguments) ? "null" : $"\"{expectedPreprocessors[1].ExecutableArguments}\"")}
                        }}
                    ]
                }}
            }}";

            const string submitter = @"{
                    ""name"": ""Test Smith"",
                    ""organization"": ""Test Organization"",
                    ""email"": ""test.smith @example.com""
                }";

            var command = new ImportBase.Command
            {
                Import = SharingModel.Deserialize(template),
                Submitter = SharingContact.Deserialize(submitter)
            };

            return command;
        }

        protected void AssertImportValidationMessages(ImportBase.Command command, DataMap[] expectedMaps, DataImport.Models.BootstrapData[] expectedBootstraps)
        {
            var validation = Validation(command.Import);
            validation.IsValid.ShouldBe(false);
            validation.Errors.Select(x => x.ErrorMessage)
                .ShouldMatch(
                    $"This template contains an invalid bootstrap '{expectedBootstraps[0].Name}': A Bootstrap Data named '{expectedBootstraps[0].Name}' already exists. Bootstraps must have unique names.",
                    $"This template contains an invalid bootstrap '{expectedBootstraps[1].Name}': A Bootstrap Data named '{expectedBootstraps[1].Name}' already exists. Bootstraps must have unique names.",
                    $"This template contains an invalid map '{expectedMaps[0].Name}': A Data Map named '{expectedMaps[0].Name}' already exists. Data Maps must have unique names.",
                    $"This template contains an invalid map '{expectedMaps[1].Name}': A Data Map named '{expectedMaps[1].Name}' already exists. Data Maps must have unique names.",
                    $"This template contains an invalid map '{expectedMaps[2].Name}': A Data Map named '{expectedMaps[2].Name}' already exists. Data Maps must have unique names.",
                    $"This template contains a lookup '{SourceTable0}' which conflicts with your existing '{SourceTable0}' lookup.",
                    $"This template contains a lookup '{SourceTable0}' which conflicts with your existing '{SourceTable0}' lookup.",
                    $"This template contains a lookup '{SourceTable1}' which conflicts with your existing '{SourceTable1}' lookup.",
                    $"This template contains a lookup '{SourceTable1}' which conflicts with your existing '{SourceTable1}' lookup."
                );
        }

        protected void DeleteDuplicateEntities(DataMap[] expectedMaps, DataImport.Models.BootstrapData[] expectedBootstraps)
        {
            Transaction(database =>
            {
                var sourceTables = new[] { SourceTable0, SourceTable1 };
                var lookups = database.Lookups.Where(x => sourceTables.Contains(x.SourceTable)).ToArray();
                foreach (var lookup in lookups)
                    database.Lookups.Remove(lookup);

                var dataMapIds = new[] { expectedMaps[0].Id, expectedMaps[1].Id, expectedMaps[2].Id };
                var dataMaps = database.DataMaps.Where(x => dataMapIds.Contains(x.Id)).ToArray();
                foreach (var dataMap in dataMaps)
                    database.DataMaps.Remove(dataMap);

                var bootstrapIds = new[] { expectedBootstraps[0].Id, expectedBootstraps[1].Id };
                var bootstraps = database.BootstrapDatas.Where(x => bootstrapIds.Contains(x.Id)).ToArray();
                foreach (var bootstrap in bootstraps)
                    database.BootstrapDatas.Remove(bootstrap);
            });
        }

        protected void ConstructExportPreview(ImportBase.Response response, ExportBase.CommandBase form)
        {
            foreach (var bootstrapId in response.BootstrapIds)
                form.Bootstraps.Single(x => x.Id == bootstrapId).Selected = true;
            foreach (var dataMapId in response.DataMapIds)
                form.DataMaps.Single(x => x.Id == dataMapId).Selected = true;
            form.Title = "Test Template";
            form.ApiVersion = ApiVersion;
            form.Description = "Test template with partial bootstrap, data map, and lookup selections.";
        }

        [Test]
        public void ShouldRequireDataMapMetadataCompatibilityWhenImporting()
        {
            var expectedMaps = new[] { DataMap1, DataMap2 }.OrderBy(x => x.Name).ToArray();

            var command = new ImportBase.Command
            {
                Import = SharingModel.Deserialize(
                    $@"{{
                            ""title"": ""Test Template"",
                            ""description"": ""Test template with data maps incompatible with the target ODS."",
                            ""apiVersion"": ""{ApiVersion}"",
                            ""submitter"": {{
                                ""name"": ""Test Smith"",
                                ""organization"": ""Test Organization"",
                                ""email"": ""test.smith@example.com""
                            }},
                            ""template"": {{
                                ""bootstraps"": [ ],
                                ""maps"": [
                                    {{
                                        ""name"": ""{expectedMaps[0].Name}"",
                                        ""resourcePath"": ""{expectedMaps[0].ResourcePath}"",
                                        ""columnHeaders"": [ ],
                                        ""map"": {{ ""unexpectedCustomMapProperty"": ""unexpectedValue"" }}
                                    }},
                                    {{
                                        ""name"": ""{expectedMaps[1].Name}"",
                                        ""resourcePath"": ""{expectedMaps[1].ResourcePath}"",
                                        ""columnHeaders"": [ ],
                                        ""map"": {{ ""unexpectedCustomMapProperty"": ""unexpectedValue"" }}
                                    }}
                                ],
                                ""lookups"": [ ]
                            }}
                        }}")
            };

            var validation = Validation(command.Import);
            validation.IsValid.ShouldBe(false);
            validation.Errors.Select(x => x.ErrorMessage)
                .ShouldMatch(
                    $"This template contains a map '{expectedMaps[0].Name}' which is not compatible with your definition " +
                    $"of resource '{expectedMaps[0].ResourcePath}'. Cannot deserialize mappings from JSON, because the " +
                    $"key 'unexpectedCustomMapProperty' should not exist according to the metadata for resource " +
                    $"'{expectedMaps[0].ResourcePath}'.",

                    $"This template contains a map '{expectedMaps[1].Name}' which is not compatible with your definition " +
                    $"of resource '{expectedMaps[1].ResourcePath}'. Cannot deserialize mappings from JSON, because the " +
                    $"key 'unexpectedCustomMapProperty' should not exist according to the metadata for resource " +
                    $"'{expectedMaps[1].ResourcePath}'."
                );
        }

        [Test]
        public async Task ShouldNotAllowExportingBootstrapsWithDifferentApiVersion()
        {
            var form = await Send(new FileExport.Query());

            //User select bootstraps 1 and 3.
            form.Bootstraps.Single(x => x.Id == BootstrapData0.Id).Selected = true;
            form.Bootstraps.Single(x => x.Id == BootstrapDataWithAdditionalApiVersion.Id).Selected = true;

            //User selects data maps 2 and 3.
            form.DataMaps.Single(x => x.Id == DataMap1.Id).Selected = true;
            form.DataMaps.Single(x => x.Id == DataMapWithAdditionalApiVersion.Id).Selected = true;

            //User finalizes export form.
            form.Title = "Test Template";
            form.ApiVersion = form.Bootstraps.Single(x => x.Id == BootstrapData0.Id).ApiVersion;
            form.Description = "Test template with partial bootstrap, data map, and lookup selections.";

            form.ShouldNotValidate("Exported Bootstrap Data Definitions and Data Maps must have the same API Version.",
                "Exported Bootstrap Data Definitions must have the same API Version.",
                "Exported Data Maps must have the same API Version.");

            form.Bootstraps.Single(x => x.Id == BootstrapDataWithAdditionalApiVersion.Id).Selected = false;
            form.ShouldNotValidate("Exported Bootstrap Data Definitions and Data Maps must have the same API Version.",
                "Exported Data Maps must have the same API Version.");

            form.DataMaps.Single(x => x.Id == DataMap1.Id).Selected = false;
            form.ShouldNotValidate("Exported Bootstrap Data Definitions and Data Maps must have the same API Version.");

            form.DataMaps.Single(x => x.Id == DataMap1.Id).Selected = true;
            form.DataMaps.Single(x => x.Id == DataMapWithAdditionalApiVersion.Id).Selected = false;
            form.ShouldValidate();
        }
    }
}
