// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Features.ApiServers;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.ApiServers
{
    public class DeleteApiServerTests
    {
        [Test]
        public async Task ShouldBeAbleToDeleteApiServer()
        {
            var newApiServer = await AddApiServer();
            var allApiServers = await Send(new ApiServerIndex.Query());

            allApiServers.ApiServers.ShouldNotBeEmpty();
            allApiServers.ApiServers.Any(x => x.Id == newApiServer.Id).ShouldBeTrue();

            var deleteResponse = await Send(new DeleteApiServer.Command
            {
                Id = newApiServer.Id
            });
            deleteResponse.AssertToast($"Connection '{newApiServer.Name}' was deleted.");

            allApiServers = await Send(new ApiServerIndex.Query());
            allApiServers.ApiServers.Any(x => x.Id == newApiServer.Id).ShouldBeFalse();
        }

        [Test]
        public async Task ShouldShowErrorMessageIfAgentIsAssociatedWithApiServer()
        {
            var newApiServer = await AddApiServer();
            await AddAgent(AgentTypeCodeEnum.Manual, newApiServer.Id);

            var deleteResponse = await Send(new DeleteApiServer.Command
            {
                Id = newApiServer.Id
            });
            deleteResponse.AssertToast("API connection cannot be deleted because there is at least one agent using it.", false);
        }

        [Test]
        public async Task ShouldBeAbleToDeleteApiServerWithProcessedBootstrapData()
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

            var deleteResponse = await Send(new DeleteApiServer.Command
            {
                Id = newApiServer.Id
            });
            deleteResponse.AssertToast($"Connection '{newApiServer.Name}' was deleted.");

            Query(d => d.BootstrapDataApiServers.Where(x => x.ApiServerId == newApiServer.Id).ToList()).ShouldBeEmpty();
        }
    }
}
