// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.ApiServers;
using NUnit.Framework;

namespace DataImport.Web.Tests.Features.ApiServers
{
    public class ApiServerIndexTests
    {
        [Test]
        public async Task ShouldDisplayAllApiServers()
        {
            var newApiServer = await Testing.AddApiServer();
            var apiVersion = Testing.Query<ApiVersion>(newApiServer.ApiVersionId);
            var allApiServers = await Testing.Send(new ApiServerIndex.Query());

            CollectionAssert.IsNotEmpty(allApiServers.ApiServers);
            Assert.IsTrue(allApiServers.ApiServers.Count >= 2);

            allApiServers.ApiServers.Single(x => x.Id == newApiServer.Id)
                .ShouldMatch(new ApiServerIndex.ApiServerModel
                {
                    Id = newApiServer.Id,
                    Name = newApiServer.Name,
                    Url = newApiServer.Url,
                    ApiVersion = apiVersion.Version
                });
        }
    }
}
