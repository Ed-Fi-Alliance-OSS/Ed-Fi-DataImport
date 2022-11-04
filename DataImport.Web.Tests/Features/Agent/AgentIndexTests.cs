// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Agent;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Agent
{
    public class AgentIndexTests
    {
        [Test]
        public async Task ShouldDisplayAllAgentsWithFileCounts()
        {
            var file = await UploadFile();
            var agentWithFile = Find<DataImport.Models.Agent>(file.AgentId);
            var agentWithoutFile = await AddAgent(AgentTypeCodeEnum.Manual);

            var allAgents = await Send(new AgentIndex.Query());

            allAgents.Agents.Single(x => x.Id == agentWithFile.Id)
                .ShouldMatch(new AgentIndex.AgentModel
                {
                    Id = agentWithFile.Id,
                    Name = agentWithFile.Name,
                    AgentTypeCode = agentWithFile.AgentTypeCode,
                    LastExecuted = agentWithFile.LastExecuted,
                    Enabled = agentWithFile.Enabled,
                    FilesCount = 1
                });

            allAgents.Agents.Single(x => x.Id == agentWithoutFile.Id)
                .ShouldMatch(new AgentIndex.AgentModel
                {
                    Id = agentWithoutFile.Id,
                    Name = agentWithoutFile.Name,
                    AgentTypeCode = agentWithoutFile.AgentTypeCode,
                    LastExecuted = agentWithoutFile.LastExecuted,
                    Enabled = agentWithoutFile.Enabled,
                    FilesCount = 0
                });
        }

        [Test]
        public async Task ShouldDisplayAgentsInRunOrderThenById()
        {
            var apiServer = GetDefaultApiServer();
            DataImport.Models.Agent NewAgent(string name, int? runOrder)
            {
                return new DataImport.Models.Agent
                {
                    Name = name,
                    AgentTypeCode = AgentTypeCodeEnum.Manual,
                    ApiServerId = apiServer.Id,
                    RunOrder = runOrder,
                };
            }

            var earlyAgent = NewAgent("EarlierAgent", 1);
            var middleAgent1 = NewAgent("MiddleAgent1", 10);
            var middleAgent2 = NewAgent("MiddleAgent2", 10);
            var lateAgent = NewAgent("LateAgent", 100);
            var unorderedAgent = NewAgent("UnorderedAgent", null);

            With<DataImportDbContext>(db => db.Agents.AddRange(lateAgent, middleAgent1, unorderedAgent, middleAgent2, earlyAgent, unorderedAgent));

            var result = await Send(new AgentIndex.Query());

            result.Agents
                .ShouldBeInOrder(SortDirection.Ascending, new AgentOrderComparer());
        }

        [Test]
        public void ShouldCompareAgentsAsExpectedWithComparer()
        {
            AgentIndex.AgentModel NewAgent(int id, int? runOrder) =>
                new() { Id = id, RunOrder = runOrder, };

            var higher = NewAgent(2, 200);
            var lower = NewAgent(1, 2);
            var sameLowerId = NewAgent(3, 20);
            var sameHigherId = NewAgent(4, 20);
            var unordered = NewAgent(5, null);

            var comparer = new AgentOrderComparer();

            comparer.Compare(higher, lower).ShouldBe(1);
            comparer.Compare(lower, higher).ShouldBe(-1);
            comparer.Compare(sameHigherId, sameLowerId).ShouldBe(1);
            comparer.Compare(sameLowerId, sameHigherId).ShouldBe(-1);

            comparer.Compare(unordered, lower).ShouldBe(1);
            comparer.Compare(higher, unordered).ShouldBe(-1);
        }

        private class AgentOrderComparer : IComparer<AgentIndex.AgentModel>
        {
            public int Compare(AgentIndex.AgentModel x, AgentIndex.AgentModel y)
            {
                if (x == null || y == null)
                    throw new ArgumentNullException();

                if (x.RunOrder == y.RunOrder)
                    return x.Id > y.Id ? 1 : -1;

                if (x.RunOrder == null)
                    return 1;
                if (y.RunOrder == null)
                    return -1;

                return x.RunOrder > y.RunOrder ? 1 : -1;
            }
        }
    }
}