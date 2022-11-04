// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.Events;
using NUnit.Framework;
using Shouldly;
using static DataImport.Server.TransformLoad.Tests.Testing;

namespace DataImport.Server.TransformLoad.Tests.Events
{
    public class JobStartedTests
    {
        [Test]
        public async Task ShouldTrackStartTimeOfCurrentJob()
        {
            var testStartTime = DateTimeOffset.Now;

            ClearExistingRecords();
            CurrentStatus().ShouldBe(null);


            await Send(new JobStarted.Command());

            var first = CurrentStatus();
            var firstStarted = first.Started.Value;
            firstStarted.ShouldBeGreaterThanOrEqualTo(testStartTime);
            first.Completed.ShouldBe(null);


            await Send(new JobStarted.Command());

            var second = CurrentStatus();
            var secondStarted = second.Started.Value;
            secondStarted.ShouldBeGreaterThanOrEqualTo(firstStarted);
            second.Completed.ShouldBe(null);
        }

        private static void ClearExistingRecords()
        {
            Transaction(database =>
            {
                foreach (var record in database.JobStatus)
                    database.Remove(record);
            });
        }

        private static JobStatus CurrentStatus()
            => Query(database => database.JobStatus.SingleOrDefault());
    }
}
