// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.Events;
using NUnit.Framework;
using Shouldly;
using static DataImport.Server.TransformLoad.Tests.Testing;

namespace DataImport.Server.TransformLoad.Tests.Events
{
    public class JobCompletedTests
    {
        [Test]
        public async Task ShouldTrackCompletionTimeOfLatestJob()
        {
            await Send(new JobStarted.Command());

            var expectedStart = CurrentStatus().Started.Value;

            await Send(new JobCompleted.Command());

            var completedStatus = CurrentStatus();
            completedStatus.Started.Value.ShouldBe(expectedStart);
            completedStatus.Completed.Value.ShouldBeGreaterThanOrEqualTo(expectedStart);
        }

        private static JobStatus CurrentStatus()
            => Query(database => database.JobStatus.SingleOrDefault());
    }
}
