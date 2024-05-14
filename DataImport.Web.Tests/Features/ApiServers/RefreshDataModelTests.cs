// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading.Tasks;
using DataImport.Web.Features.ApiServers;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.ApiServers
{
    public class RefreshDataModelTests
    {
        [Test]
        public async Task ShouldSuccessfullyRefreshDataModel()
        {
            var apiServer = GetDefaultApiVersion();
            var apiServerId = apiServer.Id;

            var refreshDataModelResponse = await Send(new RefreshDataModel.Command
            {
                ApiServerId = apiServerId
            });

            refreshDataModelResponse.Message.ShouldBe("Data model was updated");
            refreshDataModelResponse.ApiServerId.ShouldBe(apiServerId);
        }

        [Test]
        public void ShouldThrowAnExceptionWhenApiServerNotExist()
        {
            var apiServer = GetApiVersionNotExist();
            var apiServerId = apiServer.Id;

            var command = new RefreshDataModel.Command { ApiServerId = apiServerId };
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await Send(command));

            exception.Message.ShouldBe("Api server not found");
        }
    }
}
