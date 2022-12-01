// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.BootstrapData;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.TestHelpers.TestHelpers;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.BootstrapData
{
    internal class AddEditBootstrapDataTests
    {
        [Test]
        public void ShouldRequireMinimumFields()
        {
            new AddBootstrapData.Command()
                .ShouldNotValidate("'Bootstrap Name' must not be empty.", "'Resource' must not be empty.", "Please enter valid JSON.", "'API Version' must not be empty.");

            new EditBootstrapData.Command()
                .ShouldNotValidate("'Bootstrap Name' must not be empty.", "Please enter valid JSON.");
        }

        [Test]
        public async Task ShouldSuccessfullyAddBootstrapDataWhenCompatibleWithResourceMetadata()
        {
            await AddBootstrapData(RandomResource());

            var resource = RandomResource();

            var apiVersion = Query<ApiVersion>(resource.ApiVersionId);

            var compatibleSamples = new[] { "{}", "[{}, {}]" };

            foreach (var sampleJson in compatibleSamples)
            {
                var bootstrapName = SampleString();

                var addForm = await Send(new AddBootstrapData.Query());
                addForm.Name = bootstrapName;
                addForm.ResourcePath = resource.Path;
                addForm.Data = sampleJson;
                addForm.ApiVersionId = resource.ApiVersionId;

                var response = await Send(addForm);
                response.AssertToast($"Bootstrap Data '{bootstrapName}' was created.");

                var actual = Query(d => d.BootstrapDatas.Include(x => x.BootstrapDataApiServers).Single(x => x.Id == response.BootstrapDataId));
                actual.ResourcePath.ShouldBe(resource.Path);
                actual.Data.ShouldBe(sampleJson);
                actual.Metadata.ShouldBe(resource.Metadata);
                actual.CreateDate.ShouldNotBe(null);
                actual.UpdateDate.ShouldNotBe(null);

                actual.BootstrapDataApiServers.ShouldBeEmpty();
                actual.ApiVersionId.ShouldBe(resource.ApiVersionId);

                var editForm = await Send(new EditBootstrapData.Query
                {
                    BootstrapDataId = response.BootstrapDataId
                });

                editForm.ShouldMatch(new EditBootstrapData.ViewModel
                {
                    Id = response.BootstrapDataId,
                    Name = bootstrapName,
                    ResourcePath = resource.Path,
                    ResourceName = resource.ToResourceName(),
                    Data = sampleJson,
                    MetadataIsIncompatible = false,
                    ApiVersion = apiVersion.Version
                });
            }
        }

        [Test]
        public async Task ShouldPreventAddingBootstrapDataWhenIncompatibleWithResourceMetadata()
        {
            await AddBootstrapData(RandomResource());

            var resource = RandomResource();

            var badKey = SampleString();
            var randomObject = $"{{\"{badKey}\": \"{SampleString()}\"}}";

            var incompatibleSamples = new[] { randomObject, $"[{{}}, {randomObject}]" };

            foreach (var incompatibleSampleJson in incompatibleSamples)
            {
                var addForm = await Send(new AddBootstrapData.Query());
                addForm.Name = SampleString();
                addForm.ResourcePath = resource.Path;
                addForm.Data = incompatibleSampleJson;
                addForm.ApiVersionId = resource.ApiVersionId;

                addForm.ShouldNotValidate(
                    "Bootstrap JSON is not compatible with your definition of resource " +
                    $"'{resource.Path}'. Cannot deserialize mappings from JSON, " +
                    $"because the key '{badKey}' should not exist according to the " +
                    $"metadata for resource '{resource.Path}'."
                    );
            }
        }

        [Test]
        public async Task ShouldSuccessfullyEditBootstrapDataWhenCompatibleWithResourceMetadata()
        {
            await AddBootstrapData(RandomResource());
            await AddBootstrapData(RandomResource());

            var resource = RandomResource();
            var apiVersion = Query<ApiVersion>(resource.ApiVersionId);
            var initialJson = "[]";

            var compatibleSamples = new[] { "{}", "[{}, {}]" };

            foreach (var updatedJson in compatibleSamples)
            {
                var bootstrapName = SampleString();

                var addForm = await Send(new AddBootstrapData.Query());
                addForm.Name = bootstrapName;
                addForm.ResourcePath = resource.Path;
                addForm.Data = initialJson;
                addForm.ApiVersionId = resource.ApiVersionId;

                var response = await Send(addForm);

                var editForm = await Send(new EditBootstrapData.Query
                {
                    BootstrapDataId = response.BootstrapDataId
                });

                editForm.Data = updatedJson;

                var toastResponse = await Send(new EditBootstrapData.Command
                {
                    Id = editForm.Id,
                    Name = editForm.Name,
                    Data = editForm.Data
                });
                toastResponse.AssertToast($"Bootstrap Data '{editForm.Name}' was modified.");

                var actual = Query(d => d.BootstrapDatas.Include(x => x.BootstrapDataApiServers).Single(x => x.Id == response.BootstrapDataId));
                actual.Name.ShouldBe(bootstrapName);
                actual.ResourcePath.ShouldBe(resource.Path);
                actual.Data.ShouldBe(updatedJson);
                actual.Metadata.ShouldBe(resource.Metadata);
                actual.CreateDate.ShouldNotBe(null);
                actual.UpdateDate.ShouldNotBe(null);
                actual.BootstrapDataApiServers.ShouldBeEmpty();
                actual.ApiVersionId.ShouldBe(resource.ApiVersionId);

                var updatedEditForm = await Send(new EditBootstrapData.Query
                {
                    BootstrapDataId = response.BootstrapDataId
                });

                updatedEditForm.ShouldMatch(new EditBootstrapData.ViewModel
                {
                    Id = response.BootstrapDataId,
                    Name = bootstrapName,
                    ResourcePath = resource.Path,
                    ResourceName = resource.ToResourceName(),
                    Data = updatedJson,
                    MetadataIsIncompatible = false,
                    ApiVersion = apiVersion.Version
                });
            }
        }

        [Test]
        public async Task ShouldPreventEditingBootstrapDataWhenIncompatibleWithResourceMetadata()
        {
            await AddBootstrapData(RandomResource());
            await AddBootstrapData(RandomResource());

            var resource = RandomResource();
            var initialJson = "[]";

            var badKey = SampleString();
            var randomObject = $"{{\"{badKey}\": \"{SampleString()}\"}}";

            var incompatibleSamples = new[] { randomObject, $"[{{}}, {randomObject}]" };

            foreach (var updatedJson in incompatibleSamples)
            {
                var addForm = await Send(new AddBootstrapData.Query());
                addForm.Name = SampleString();
                addForm.ResourcePath = resource.Path;
                addForm.Data = initialJson;
                addForm.ApiVersionId = resource.ApiVersionId;

                var response = await Send(addForm);

                var editForm = await Send(new EditBootstrapData.Query
                {
                    BootstrapDataId = response.BootstrapDataId
                });

                editForm.Data = updatedJson;

                new EditBootstrapData.Command
                {
                    Id = editForm.Id,
                    Name = editForm.Name,
                    Data = editForm.Data
                }.ShouldNotValidate(
                    "Bootstrap JSON is not compatible with your definition of resource " +
                    $"'{resource.Path}'. Cannot deserialize mappings from JSON, because the " +
                    $"key '{badKey}' should not exist according " +
                    $"to the metadata for resource '{resource.Path}'.");
            }
        }

        [Test]
        public async Task ShouldWarnWhenEditingBootstrapDataThatHadBecomeIncompatibleWithLatestResourceMetadata()
        {
            var resource = RandomResource();

            var existingBootstrapData = await AddBootstrapData(resource);
            var bootstrapDataId = existingBootstrapData.Id;
            var bootstrapName = existingBootstrapData.Name;
            var apiVersion = Query<ApiVersion>(resource.ApiVersionId);

            //Simulate a drastic disconnect between the existing bootstrap JSON
            //and the latest metadata, by bypassing our validators and saving random JSON
            //into the record we just created. This way, revisiting the edit form will behave
            //as if the user pointed at a dramatically-different version of the ODS whose
            //metadata affects our previously-valid bootstrap. The user should be alerted
            //to the problem even before attempting to save.
            var randomObject = $"{{\"{SampleString()}\": \"{SampleString()}\"}}";
            Transaction(database =>
            {
                var bootstrap = database.BootstrapDatas.Single(x => x.Id == bootstrapDataId);
                bootstrap.Data = randomObject;
                database.SaveChanges();
            });

            var editForm = await Send(new EditBootstrapData.Query
            {
                BootstrapDataId = bootstrapDataId
            });

            editForm.ShouldMatch(new EditBootstrapData.ViewModel
            {
                Id = bootstrapDataId,
                Name = bootstrapName,
                ResourcePath = resource.Path,
                ResourceName = resource.ToResourceName(),
                Data = randomObject,
                MetadataIsIncompatible = true,
                ApiVersion = apiVersion.Version
            });
        }

        [Test]
        public async Task ShouldWarnAndPreventEditingWhenEditingBootstrapDataForResourceThatHasSinceBeenRemoved()
        {
            var resource = RandomResource();

            var existingBootstrapData = await AddBootstrapData(resource);
            var originalJson = existingBootstrapData.Data;
            var bootstrapDataId = existingBootstrapData.Id;
            var bootstrapName = existingBootstrapData.Name;
            var apiVersion = Query<ApiVersion>(resource.ApiVersionId);

            //Simulate a drastic disconnect between the existing bootstrap JSON
            //and the latest metadata, by bypassing our validators and saving an incorrect
            //resource name into the record we just created. This way, revisiting the edit
            //form will behave as if the user pointed at a dramatically-different version
            //of the ODS where there is no such resource, affecting our previously-valid
            //bootstrap. The user should be alerted to the problem even before attempting to save.
            var unrecognizedResourcePath = SampleString("/unrecognizedResource");
            Transaction(database =>
            {
                var bootstrap = database.BootstrapDatas.Single(x => x.Id == bootstrapDataId);
                bootstrap.ResourcePath = unrecognizedResourcePath;
                bootstrap.ApiVersionId = resource.ApiVersionId;
                database.SaveChanges();
            });

            var editForm = await Send(new EditBootstrapData.Query
            {
                BootstrapDataId = bootstrapDataId
            });

            editForm.ShouldMatch(new EditBootstrapData.ViewModel
            {
                Id = bootstrapDataId,
                Name = bootstrapName,
                ResourcePath = unrecognizedResourcePath,
                ResourceName = unrecognizedResourcePath.ToResourceName(),
                Data = originalJson,
                MetadataIsIncompatible = true, //The user is warned.
                ApiVersion = apiVersion.Version
            });

            //The user could never submit this successfully.
            new EditBootstrapData.Command
            {
                Id = editForm.Id,
                Name = editForm.Name,
                Data = editForm.Data
            }.ShouldNotValidate($"Resource '{unrecognizedResourcePath}' does not exist in the configured target ODS.");
        }

        [Test]
        public async Task ShouldRequireUniqueNameWhenAddingNewBootstrapData()
        {
            var existingBootstrap = await AddBootstrapData(RandomResource());

            new AddBootstrapData.Command
            {
                Name = existingBootstrap.Name,
                ResourcePath = existingBootstrap.ResourcePath,
                Data = existingBootstrap.Data,
                ApiVersionId = existingBootstrap.ApiVersionId
            }.ShouldNotValidate(
                $"A Bootstrap Data named '{existingBootstrap.Name}' already exists. Bootstraps must have unique names.");
        }

        [Test]
        public async Task ShouldAllowEditingBootstrapDataWithoutChangingName()
        {
            var existingBootstrap = await AddBootstrapData(RandomResource());

            new EditBootstrapData.Command
            {
                Id = existingBootstrap.Id,
                Name = existingBootstrap.Name,
                Data = existingBootstrap.Data
            }.ShouldValidate();
        }

        [Test]
        public async Task ShouldPreventEditingBootstrapDataWithDuplicateName()
        {
            var existingBootstrapName = (await AddBootstrapData(RandomResource())).Name;

            var bootstrapToEdit = await AddBootstrapData(RandomResource());

            new EditBootstrapData.Command
            {
                Id = bootstrapToEdit.Id,
                Name = existingBootstrapName,
                Data = bootstrapToEdit.Data
            }.ShouldNotValidate(
                $"A Bootstrap Data named '{existingBootstrapName}' already exists. Bootstraps must have unique names.");
        }

        [Test]
        public async Task ShouldSuccessfullyDeleteBootstrapData()
        {
            var existingBootstrap = await AddBootstrapData(RandomResource());

            var response = await Send(new DeleteBootstrapData.Command { BootstrapDataId = existingBootstrap.Id });
            response.AssertToast($"Bootstrap Data '{existingBootstrap.Name}' was deleted.");
            Query<DataImport.Models.BootstrapData>(existingBootstrap.Id).ShouldBeNull();
        }

        [Test]
        public async Task ShouldNotAllowDeletingBootstrapDataUsedByAgent()
        {
            var bootstrapData = await AddBootstrapData(RandomResource());

            var apiServer = GetDefaultApiServer();

            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Enabled = true,
                Name = SampleString(),
                ApiServerId = apiServer.Id,
                DdlBootstrapDatas = new List<string>
                {
                    Json(new AgentBootstrapData { BootstrapDataId = bootstrapData.Id })
                }
            };

            var addAgentResponse = await Send(new AddAgent.Command { ViewModel = viewModel });

            var response = await Send(new DeleteBootstrapData.Command { BootstrapDataId = bootstrapData.Id });
            response.AssertToast("Bootstrap Data cannot be deleted because it is used by one or more Agents.", false);
            Query<DataImport.Models.BootstrapData>(bootstrapData.Id).ShouldNotBeNull();

            viewModel = await Send(new EditAgent.Query { Id = addAgentResponse.AgentId });
            var editAgentResponse = await Send(new EditAgent.Command { ViewModel = viewModel });

            response = await Send(new DeleteBootstrapData.Command { BootstrapDataId = bootstrapData.Id });
            response.AssertToast($"Bootstrap Data '{bootstrapData.Name}' was deleted.");
            Query<DataImport.Models.BootstrapData>(bootstrapData.Id).ShouldBeNull();
        }

        [Test]
        public async Task ShouldBeAbleToDeleteProcessedBootstrapData()
        {
            var bootstrapData = await AddBootstrapData(RandomResource());

            var newApiServer = await AddApiServer();

            Query(d =>
            {
                var bootstrapDataApiServer = new BootstrapDataApiServer
                {
                    ApiServerId = newApiServer.Id,
                    BootstrapDataId = bootstrapData.Id,
                    ProcessedDate = DateTimeOffset.Now,
                };
                d.BootstrapDataApiServers.Add(bootstrapDataApiServer);

                d.SaveChanges();

                return bootstrapDataApiServer;
            });

            var deleteResponse = await Send(new DeleteBootstrapData.Command
            {
                BootstrapDataId = bootstrapData.Id
            });
            deleteResponse.AssertToast($"Bootstrap Data '{bootstrapData.Name}' was deleted.");

            Query(d => d.BootstrapDataApiServers.Where(x => x.BootstrapDataId == bootstrapData.Id).ToList()).ShouldBeEmpty();
        }
    }
}
