// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Features.ApiServers;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Services;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    [TestFixture]
    public class RetrieveDescriptorsTests
    {
        [Test]
        public async Task ShouldReturnAvailableDescriptorsIfApiServerIdIsSupplied()
        {
            var apiServer = GetDefaultApiServer();

            var viewModel = await Send(new RetrieveDescriptors.Query { ApiServerId = apiServer.Id });

            viewModel.AvailableDescriptors.ShouldNotBeEmpty();
            viewModel.ApiServers.ShouldNotBeEmpty();
            viewModel.DescriptorsFound.ShouldBeFalse();
            viewModel.ApiServerId.ShouldBe(apiServer.Id);
            viewModel.ApiVersion.ShouldNotBeEmpty();
            viewModel.Descriptors.ShouldBeNull();
            viewModel.DescriptorName.ShouldBeNull();
        }

        [Test]
        public async Task ShouldReturnApiServersIfApiServerIdIsMissingInQuery()
        {
            // Add additional api server to make sure there are more than one api server in the system.
            await AddApiServer();

            var viewModel = await Send(new RetrieveDescriptors.Query());

            viewModel.AvailableDescriptors.ShouldBeNull();
            viewModel.ApiServerId.ShouldBeNull();
            viewModel.ApiServers.ShouldNotBeEmpty();
            viewModel.DescriptorsFound.ShouldBeFalse();
            viewModel.ApiVersion.ShouldBeNull();
            viewModel.Descriptors.ShouldBeNull();
            viewModel.DescriptorName.ShouldBeNull();
        }

        [Test]
        public async Task ShouldThrowOdsApiExceptionIfNoConnectionsFound()
        {
            var apiServers = Query(x => x.ApiServers.ToList());

            try
            {
                // Deleting the api server records for testing. Disassociate existing API Agents from ApiServers first
                Query(d =>
                {
                    var agents = d.Agents.ToList();
                    agents.ForEach(a => a.ApiServerId = null);
                    d.SaveChanges();
                    return agents;
                });

                foreach (var apiServer in apiServers)
                {
                    await Send(new DeleteApiServer.Command { Id = apiServer.Id });
                }

                await Should.ThrowAsync<OdsApiServerException>(Send(new RetrieveDescriptors.Query()));
            }
            finally
            {
                // Adding the deleted api server configuration record
                await ConfigureForOdsApiV311();
            }
        }

        [Test]
        public async Task ShouldFilterAvailableApiServersByApiVersion()
        {
            // Add additional api server to make sure there are more than one api server in the system.
            var apiServer311 = await AddApiServer(StubSwaggerWebClient.ApiServerUrlV311, OdsApiV311);
            var apiServer25 = await AddApiServer(StubSwaggerWebClient.ApiServerUrlV25, OdsApiV25);
            var allApiServers = Query(d => d.ApiServers.ToList());
            var viewModel = await Send(new RetrieveDescriptors.Query { ApiVersionId = apiServer25.ApiVersionId });
            var apiVersion = Query<ApiVersion>(apiServer25.ApiVersionId);

            viewModel.ApiServers.ShouldNotBeEmpty();
            viewModel.ApiServers.ShouldContain(x => string.IsNullOrEmpty(x.Value));
            var apiServers = viewModel.ApiServers.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();
            apiServers.ShouldNotBeEmpty();
            apiServers.ShouldAllBe(x => allApiServers.Single(s => s.Id.ToString(CultureInfo.InvariantCulture) == x.Value).ApiVersionId == apiServer25.ApiVersionId);
            if (apiServers.Count == 1)
            {
                viewModel.ApiServerId.ShouldBe(int.Parse(apiServers[0].Value));
                viewModel.AvailableDescriptors.ShouldNotBeEmpty();
                viewModel.ApiVersion.ShouldBe(apiVersion.Version);
            }
            else
            {
                viewModel.ApiServerId.ShouldBeNull();
                viewModel.AvailableDescriptors.ShouldBeNull();
                viewModel.ApiVersion.ShouldBeNull();
            }
            viewModel.DescriptorsFound.ShouldBeFalse();
            viewModel.Descriptors.ShouldBeNull();
            viewModel.DescriptorName.ShouldBeNull();
        }
    }
}
