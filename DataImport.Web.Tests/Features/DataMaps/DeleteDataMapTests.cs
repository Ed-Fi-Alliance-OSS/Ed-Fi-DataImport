// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DataImport.Web.Features.DataMaps;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;
using DataImport.Models;
using DataImport.Web.Features.Agent;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    internal class DeleteDataMapTests
    {
        [Test]
        public async Task ShouldDeleteDataMapAndItsAgentAssociations()
        {
            var dataMapId = await AddDataMap();
            var agentId = await AssociateAgentToDataMap(dataMapId);

            var actualDataMap = QueryDataMap(dataMapId);
            var actualAgent = QueryAgent(agentId);

            var actualDataMapAgent = actualDataMap.DataMapAgents.Single();
            actualDataMapAgent.Id.ShouldBe(actualAgent.DataMapAgents.Single().Id);

            var response = await Send(new DeleteDataMap.Command { Id = dataMapId });
            response.AssertToast($"Data Map '{actualDataMap.Name}' was deleted.");

            Query<DataMap>(dataMapId).ShouldBeNull();
            Query<DataMapAgent>(actualDataMapAgent.Id).ShouldBeNull();
            QueryAgent(agentId).DataMapAgents.ShouldBeEmpty();
        }

        private static async Task<int> AddDataMap()
        {
            var resource = RandomResource();
            var mapName = SampleString();
            var mappings = await TrivialMappings(resource);

            var response = await Send(new AddDataMap.Command
            {
                ApiVersionId = resource.ApiVersionId,
                ResourcePath = resource.Path,
                MapName = mapName,
                Mappings = mappings
            });

            return response.DataMapId;
        }

        private static async Task<int> AssociateAgentToDataMap(int dataMapId)
        {
            var apiServer = GetDefaultApiServer();

            return (await Send(new AddAgent.Command
            {
                ViewModel = new AddEditAgentViewModel
                {
                    AgentTypeCode = "Manual",
                    Name = SampleString(),
                    DdlDataMaps = new List<string>
                    {
                        TestHelpers.TestHelpers.Json(new MappedAgent { DataMapId = dataMapId })
                    },
                    ApiServerId = apiServer.Id
                }
            })).AgentId;
        }

        private static DataMap QueryDataMap(int dataMapId)
        {
            return Query(database => database.DataMaps.Include(x => x.DataMapAgents).Single(x => x.Id == dataMapId));
        }

        private static DataImport.Models.Agent QueryAgent(int agentId)
        {
            return Query(database => database.Agents.Include(x => x.DataMapAgents).Single(x => x.Id == agentId));
        }
    }
}
