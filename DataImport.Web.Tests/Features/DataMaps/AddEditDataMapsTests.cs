// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.DataMaps;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    internal class AddEditDataMapsTests
    {
        [Test]
        public void ShouldRequireMinimumFields()
        {
            new AddDataMap.Command()
                .ShouldNotValidate("'Map Name' must not be empty.", "'API Version' must not be empty.", "'Map To Resource' must not be empty.");

            new EditDataMap.Command()
                .ShouldNotValidate("'Map Name' must not be empty.");
        }

        [Test]
        public async Task ShouldRequireUniqueNameWhenAddingNewDataMap()
        {
            var existingName = SampleString();

            var existingResource = RandomResource();
            var anotherExistingResource = RandomResource();

            var trivialMappings = await TrivialMappings(existingResource);

            await Send(new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = existingName,
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings
            });

            new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = existingName,
                ResourcePath = anotherExistingResource.Path,
                Mappings = trivialMappings
            }.ShouldNotValidate(
                $"A Data Map named '{existingName}' already exists. Data Maps must have unique names.");
        }

        [Test]
        public async Task ShouldAllowEditingDataMapWithoutChangingName()
        {
            var sampleName = SampleString();

            var existingResource = RandomResource();

            var trivialMappings = await TrivialMappings(existingResource);

            var mapToEdit = (await Send(new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = sampleName,
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings
            })).DataMapId;

            new EditDataMap.Command { DataMapId = mapToEdit, MapName = sampleName }.ShouldValidate();
        }

        [Test]
        public async Task ShouldPreventEditingDataMapWithDuplicateName()
        {
            var existingName = SampleString();

            var existingResource = RandomResource();

            var trivialMappings = await TrivialMappings(existingResource);

            await Send(new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = existingName,
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings
            });

            var agentToEdit = (await Send(new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = SampleString(),
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings
            })).DataMapId;

            new EditDataMap.Command { DataMapId = agentToEdit, MapName = existingName }.ShouldNotValidate(
                $"A Data Map named '{existingName}' already exists. Data Maps must have unique names.");
        }

        [Test]
        public async Task ShouldSuccessfullyAddDataMap()
        {
            var resource = RandomResource();
            var mapName = SampleString();
            var mappings = await TrivialMappings(resource);
            var apiVersion = Query(d => d.ApiVersions.Single(x => x.Id == resource.ApiVersionId));

            var dataMapSerializer = new DataMapSerializer(resource);
            var expectedJsonMap = dataMapSerializer.Serialize(mappings, false);
            var sourceCsvHeaders = new[] { "ColA", "ColB", "ColC" };

            var addForm = await Send(new AddDataMap.Query { SourceCsvHeaders = sourceCsvHeaders });
            addForm.ColumnHeaders.ShouldMatch("ColA", "ColB", "ColC");
            addForm.FieldsViewModel.DataSources.ShouldMatch(
                new SelectListItem { Text = "Select Data Source", Value = "" },
                new SelectListItem { Text = "column", Value = "column" },
                new SelectListItem { Text = "lookup-table", Value = "lookup-table" },
                new SelectListItem { Text = "static", Value = "static" });
            addForm.FieldsViewModel.SourceTables.ShouldMatch(Query(DataMapperFields.MapLookupTablesToViewModel));
            addForm.FieldsViewModel.SourceColumns.ShouldMatch(
                new SelectListItem { Text = "Select Source Column", Value = "" },
                new SelectListItem { Text = "ColA", Value = "ColA" },
                new SelectListItem { Text = "ColB", Value = "ColB" },
                new SelectListItem { Text = "ColC", Value = "ColC" });
            addForm.FieldsViewModel.ResourceMetadata.ShouldBeEmpty();
            addForm.FieldsViewModel.Mappings.ShouldBeEmpty();

            var response = await Send(new AddDataMap.Command
            {
                ApiVersionId = resource.ApiVersionId,
                ResourcePath = resource.Path,
                MapName = mapName,
                Mappings = mappings,
                ColumnHeaders = addForm.ColumnHeaders
            });
            response.AssertToast($"Data Map '{mapName}' was created.");

            var actual = Query<DataMap>(response.DataMapId);
            actual.Name.ShouldBe(mapName);
            actual.ResourcePath.ShouldBe(resource.Path);
            actual.Map.ShouldBe(expectedJsonMap);
            actual.Metadata.ShouldBe(resource.Metadata);
            actual.CreateDate.ShouldNotBe(null);
            actual.UpdateDate.ShouldNotBe(null);
            actual.ApiVersionId.ShouldBe(resource.ApiVersionId);

            var editForm = await Send(new EditDataMap.Query { Id = response.DataMapId, SourceCsvHeaders = new string[] { } });

            editForm.ShouldMatch(new AddEditDataMapViewModel
            {
                DataMapId = response.DataMapId,
                ResourcePath = resource.Path,
                ResourceName = resource.ToResourceName(),
                MapName = mapName,
                ColumnHeaders = sourceCsvHeaders,
                FieldsViewModel = editForm.FieldsViewModel,
                MetadataIsIncompatible = false,
                ApiVersions = editForm.ApiVersions,
                ApiVersion = apiVersion.Version,
                ApiVersionId = apiVersion.Id,
                Preprocessors = editForm.Preprocessors,
                PreprocessorLogMessages = editForm.PreprocessorLogMessages,
                ApiServers = editForm.ApiServers
            });
        }

        [Test]
        public async Task ShouldSuccessfullyEditDataMap()
        {
            // This test deals with editing an empty map to one with a single static
            // mapped value, so the only usable resources for these tests are those
            // which have at least one mappable property. That *should* be all resources,
            // but the simplest way to find a field to map to is to search for resources
            // with at least one top-level non-array / non-object property. Most resources
            // do meet this requirement.

            var apiVersion = GetDefaultApiVersion();

            var usableResources = Query(x => x.Resources.Where(r => r.ApiVersionId == apiVersion.Id).ToArray())
                .Where(x => ResourceMetadata.DeserializeFrom(x).Any(IsStaticMappable))
                .ToArray();

            usableResources.Length.ShouldBeGreaterThan(0);

            foreach (var resource in usableResources)
            {
                var initialMapName = SampleString();
                var updatedMapName = SampleString();
                var initialMappings = await TrivialMappings(resource);
                var updatedMappings = await TrivialMappings(resource);

                FirstStaticMappableProperty(updatedMappings, resource).Value = "ABC";

                var columnHeaders = new[] { "ColA", "ColB", "ColC" };

                var dataMapSerializer = new DataMapSerializer(resource);
                var expectedJsonMap = dataMapSerializer.Serialize(updatedMappings, false);

                var response = await Send(new AddDataMap.Command
                {
                    ApiVersionId = resource.ApiVersionId,
                    ResourcePath = resource.Path,
                    MapName = initialMapName,
                    Mappings = initialMappings,
                    ColumnHeaders = columnHeaders
                });

                var editForm = await Send(new EditDataMap.Query
                { Id = response.DataMapId, SourceCsvHeaders = new string[] { } });

                editForm.MapName = updatedMapName;

                var toastResponse = await Send(new EditDataMap.Command
                {
                    DataMapId = response.DataMapId,
                    MapName = editForm.MapName,
                    Mappings = updatedMappings,
                    ColumnHeaders = editForm.ColumnHeaders
                });
                toastResponse.AssertToast($"Data Map '{editForm.MapName}' was modified.");

                var actual = Query<DataMap>(response.DataMapId);
                actual.Name.ShouldBe(updatedMapName);
                actual.ResourcePath.ShouldBe(resource.Path);
                actual.Map.ShouldBe(expectedJsonMap);
                actual.Metadata.ShouldBe(resource.Metadata);
                actual.CreateDate.ShouldNotBe(null);
                actual.UpdateDate.ShouldNotBe(null);

                var updatedEditForm = await Send(new EditDataMap.Query { Id = response.DataMapId, SourceCsvHeaders = new string[] { } });

                updatedEditForm.ShouldMatch(new AddEditDataMapViewModel
                {
                    DataMapId = response.DataMapId,
                    ResourcePath = resource.Path,
                    ResourceName = resource.ToResourceName(),
                    MapName = updatedMapName,
                    ColumnHeaders = columnHeaders,
                    FieldsViewModel = updatedEditForm.FieldsViewModel,
                    MetadataIsIncompatible = false,
                    ApiVersions = updatedEditForm.ApiVersions,
                    ApiVersion = apiVersion.Version,
                    ApiVersionId = apiVersion.Id,
                    ApiServers = updatedEditForm.ApiServers,
                    PreprocessorLogMessages = updatedEditForm.PreprocessorLogMessages,
                    Preprocessors = updatedEditForm.Preprocessors
                });
            }
        }

        [Test]
        public async Task ShouldRequireAttributeForPreprocessor()
        {
            var existingResource = RandomResource();

            var trivialMappings = await TrivialMappings(existingResource);

            var preprocessor = await AddPreprocessor(ScriptType.CustomFileProcessor, true);

            new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = SampleString("Data Map"),
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings,
                PreprocessorId = preprocessor.Id
            }.ShouldNotValidate(
                $"Preprocessor '{preprocessor.Name}' requires a map attribute.");
        }

        [Test]
        public async Task ShouldPersistAttributeValue()
        {
            var existingResource = RandomResource();

            var trivialMappings = await TrivialMappings(existingResource);

            var preprocessor = await AddPreprocessor(ScriptType.CustomFileProcessor, true);

            var dataMap = await Send(new AddDataMap.Command
            {
                ApiVersionId = existingResource.ApiVersionId,
                MapName = SampleString("Data Map"),
                ResourcePath = existingResource.Path,
                Mappings = trivialMappings,
                PreprocessorId = preprocessor.Id,
                Attribute = "Some attribute"
            });

            var editDataMap = await Send(new EditDataMap.Query { Id = dataMap.DataMapId });
            editDataMap.PreprocessorId.ShouldBe(preprocessor.Id);
            editDataMap.Attribute.ShouldBe("Some attribute");

            var editResponse = await Send(new EditDataMap.Command
            {
                PreprocessorId = preprocessor.Id,
                Attribute = "Updated attribute",
                DataMapId = editDataMap.DataMapId,
                ColumnHeaders = editDataMap.ColumnHeaders,
                MapName = editDataMap.MapName,
                Mappings = trivialMappings,
                ResourcePath = existingResource.Path
            });

            editDataMap = await Send(new EditDataMap.Query { Id = dataMap.DataMapId });
            editDataMap.PreprocessorId.ShouldBe(preprocessor.Id);
            editDataMap.Attribute.ShouldBe("Updated attribute");
        }

        private DataMapper FirstStaticMappableProperty(IReadOnlyList<DataMapper> mappings, Resource resource)
        {
            var metadatas = ResourceMetadata.DeserializeFrom(resource);

            foreach (var mapping in mappings)
                if (IsStaticMappable(metadatas.Single(m => m.Name == mapping.Name)))
                    return mapping;

            throw new Exception("Could not find a representative static mappable property.");
        }


        private bool IsStaticMappable(ResourceMetadata m)
        {
            return m.DataType != "array" && !m.Children.Any();
        }
    }
}
