// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.TestHelpers;
using DataImport.Web.Features.ApiServers;
using DataImport.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.ApiServers
{
    public class AddEditApiServerTests
    {
        private readonly string _originalEncryptionKeyValue = Testing.Services.GetRequiredService<IOptions<AppSettings>>().Value.EncryptionKey;

        [Test]
        public void ShouldRequireMinimumFields()
        {
            new AddEditApiServerViewModel()
                .ShouldNotValidate("'Name' must not be empty.", "'URL' must not be empty.", "'Key' must not be empty.", "'Secret' must not be empty.");

            var existingApiServer = Query(x => x.ApiServers.First());

            new AddEditApiServerViewModel
            {
                Name = existingApiServer.Name
            }
                .ShouldNotValidate($"API Connection with name '{existingApiServer.Name}' already exists.", "'URL' must not be empty.", "'Key' must not be empty.", "'Secret' must not be empty.");

            new AddEditApiServerViewModel
            {
                Name = existingApiServer.Name,
                Id = existingApiServer.Id
            }
                .ShouldNotValidate("'URL' must not be empty.", "'Key' must not be empty.", "'Secret' must not be empty.");
        }

        [Test]
        public async Task ShouldSuccessfullyAddEditApiServer()
        {
            // Delete resources for testing.
            var resources = Query(d => d.Resources.ToList());
            foreach (var resource in resources)
            {
                Delete(resource);
            }

            var ods25Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV25).ToList());
            ods25Resources.ShouldBeEmpty();
            var ods311Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV311).ToList());
            ods311Resources.ShouldBeEmpty();

            var viewModel = new AddEditApiServerViewModel
            {
                Name = SampleString("ApiServer"),
                ApiVersion = OdsApiV25,
                Url = StubSwaggerWebClient.ApiServerUrlV25,
                Key = SampleString("testKey"),
                Secret = SampleString("testSecret")
            };

            var addApiServerResponse = await Send(new AddApiServer.Command { ViewModel = viewModel });
            addApiServerResponse.AssertToast($"Connection '{viewModel.Name}' was created.");
            addApiServerResponse.ApiServerId.ShouldBeGreaterThan(0);

            var addEditViewModel = await Send(new EditApiServer.Query { Id = addApiServerResponse.ApiServerId });
            addEditViewModel.Name.ShouldBe(viewModel.Name);
            addEditViewModel.Url.ShouldBe(viewModel.Url);
            addEditViewModel.ApiVersion.ShouldBe(viewModel.ApiVersion);
            addEditViewModel.Key.ShouldNotBeEmpty();
            SensitiveText.IsMasked(addEditViewModel.Key).ShouldBeTrue();
            SensitiveText.IsMasked(addEditViewModel.Secret).ShouldBeTrue();

            ods25Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV25).ToList());
            ods25Resources.ShouldNotBeEmpty();
            ods311Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV311).ToList());
            ods311Resources.ShouldBeEmpty();

            viewModel = new AddEditApiServerViewModel
            {
                Id = addApiServerResponse.ApiServerId,
                Name = SampleString("ApiServer"),
                ApiVersion = OdsApiV311,
                Url = StubSwaggerWebClient.ApiServerUrlV311,
                Key = SampleString("testKey"),
                Secret = SampleString("testSecret")
            };
            var editApiServerResponse = await Send(new EditApiServer.Command { ViewModel = viewModel });
            editApiServerResponse.AssertToast($"Connection '{viewModel.Name}' was modified.");
            editApiServerResponse.ApiServerId.ShouldBeGreaterThan(0);

            var editApiServerViewModel = await Send(new EditApiServer.Query { Id = addApiServerResponse.ApiServerId });
            editApiServerViewModel.Name.ShouldBe(viewModel.Name);
            editApiServerViewModel.Url.ShouldBe(viewModel.Url);
            editApiServerViewModel.ApiVersion.ShouldBe(viewModel.ApiVersion);
            SensitiveText.IsMasked(editApiServerViewModel.Key).ShouldBeTrue();
            SensitiveText.IsMasked(editApiServerViewModel.Secret).ShouldBeTrue();

            ods25Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV25).ToList());
            ods25Resources.ShouldNotBeEmpty();
            ods311Resources = Query(d => d.Resources.Where(x => x.ApiVersion.Version == OdsApiV311).ToList());
            ods311Resources.ShouldNotBeEmpty();
        }

        [Test]
        public async Task ShouldFullyMaskKeyAndSecret()
        {
            var apiServer = Query(d => d.ApiServers.OrderBy(x => x.Id).First());

            var addEditViewModel = await Send(new EditApiServer.Query { Id = apiServer.Id });
            addEditViewModel.Key.ShouldAllBe(x => x == '*');
            addEditViewModel.Secret.ShouldAllBe(x => x == '*');
        }

        [Test]
        public async Task ShouldDisplayGuidanceForMissingEncryptionKey()
        {
            try
            {
                // Clearing the encryption key value for testing
                UpdateEncryptionKeyValueOnAppConfig(string.Empty);
                var apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).OrderBy(x => x.Id).First());

                var addEditViewModel = await Send(new EditApiServer.Query { Id = apiServer.Id });
                addEditViewModel.ShouldMatch(new AddEditApiServerViewModel
                {
                    Name = apiServer.Name,
                    Id = apiServer.Id,
                    EncryptionFailureMsg = Constants.ConfigDecryptionError,
                    ApiVersion = apiServer.ApiVersion.Version,
                    Url = apiServer.Url,
                    Key = string.Empty,
                    Secret = string.Empty,
                });
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public async Task ShouldDisplayGuidanceForDifferentEncryptionKey()
        {
            try
            {
                // Clearing the encryption key value for testing
                UpdateEncryptionKeyValueOnAppConfig("DifferentKey");
                var apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).OrderBy(x => x.Id).First());

                var addEditViewModel = await Send(new EditApiServer.Query { Id = apiServer.Id });
                addEditViewModel.ShouldMatch(new AddEditApiServerViewModel
                {
                    Name = apiServer.Name,
                    Id = apiServer.Id,
                    EncryptionFailureMsg = Constants.ConfigDecryptionError,
                    ApiVersion = apiServer.ApiVersion.Version,
                    Url = apiServer.Url,
                    Key = string.Empty,
                    Secret = string.Empty,
                });
            }
            finally
            {
                // Update the encryption key with original value
                UpdateEncryptionKeyValueOnAppConfig(_originalEncryptionKeyValue);
            }
        }

        [Test]
        public async Task ShouldDisplayGuidanceForFailedConfiguration()
        {
            var apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).OrderBy(x => x.Id).First());
            var editForm = await Send(new EditApiServer.Query
            {
                OdsApiServerException = true,
                Id = apiServer.Id
            });

            editForm.Name.ShouldBe(apiServer.Name);
            editForm.Id.ShouldBe(apiServer.Id);
            editForm.Url.ShouldBe(apiServer.Url);
            editForm.ApiVersion.ShouldBe(apiServer.ApiVersion.Version);
            editForm.ConfigurationFailureMsg.ShouldBe("An error occurred while attempting to contact the configured " +
                                                      "ODS API Server. Check the connection here and try again.");
            SensitiveText.IsMasked(editForm.Key).ShouldBeTrue();
            SensitiveText.IsMasked(editForm.Secret).ShouldBeTrue();
        }

        [Test]
        public void ShouldRequireMinimumFieldsWhenTestingOdsApiConfiguration()
        {
            var query = new TestApiServerConnection.Query();
            query.ShouldNotValidate(
                "'API Server Url' must not be empty.",
                "You must authorize access to the ODS API by providing your Key and Secret.",
                "The API Version could not be determined from the API URL provided.");

            query.ApiVersion = SampleString();
            query.Url = SampleString();
            query.Key = SampleString();
            query.Secret = SampleString();

            query.ShouldValidate();
        }

    }
}
