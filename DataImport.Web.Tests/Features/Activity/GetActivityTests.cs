// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Activity;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Activity
{
    public class GetActivityTests
    {
        private readonly TimeSpan _centralTime = TimeSpan.FromHours(-5);

        [Test]
        public async Task ShouldWarnWhenJobHasNeverRun()
        {
            ClearExistingRecords();
            Count<JobStatus>().ShouldBe(0);

            (await Send(new GetActivity.Query())).Health
                .ShouldMatch(new GetActivity.HealthModel("The Transform / Load process has not yet executed.", warning: true));

            Count<JobStatus>().ShouldBe(1);

            (await Send(new GetActivity.Query())).Health
                .ShouldMatch(new GetActivity.HealthModel("The Transform / Load process has not yet executed.", warning: true));

            Count<JobStatus>().ShouldBe(1);
        }

        [Test]
        public async Task ShouldInformUserAboutCurrentlyRunningJob()
        {
            var started = new DateTimeOffset(2019, 10, 24, 2, 9, 30, _centralTime);

            Save(new JobStatus
            {
                Started = started,
                Completed = null
            });

            StubClock.SetCurrentTime(started.Add(new TimeSpan(1, 15, 27)));

            (await Send(new GetActivity.Query())).Health
                .ShouldMatch(new GetActivity.HealthModel(
                    "The Transform / Load process has been running for 1 hour, 15 minutes, and 27 seconds.",
                    warning: false));
        }

        [Test]
        public async Task ShouldInformUserAboutLastCompletedJobWhenInactive()
        {
            var started = new DateTimeOffset(2019, 10, 24, 2, 9, 30, _centralTime);

            Save(new JobStatus
            {
                Started = started,
                Completed = started.Add(new TimeSpan(0, 1, 23, 45))
            });

            (await Send(new GetActivity.Query())).Health
                .ShouldMatch(new GetActivity.HealthModel(
                    "The Transform / Load process started at 2019-10-24 02:09 AM and ran for 1 hour, 23 minutes, and 45 seconds.",
                    warning: false));
        }

        [Test]
        public async Task ShouldDisplayFilesThatAreProblemOrPendingOrRecentlyLoaded()
        {
            Transaction(database =>
            {
                foreach (var record in database.Files)
                    database.Remove(record);
            });

            var sevenDaysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var old = sevenDaysAgo.Subtract(TimeSpan.FromDays(1));
            var recent = sevenDaysAgo.Add(TimeSpan.FromDays(1));

            var oldErrorLoading = await UploadFile();
            var oldErrorTransform = await UploadFile();
            var oldErrorUploaded = await UploadFile();
            var oldLoaded = await UploadFile();
            var oldLoading = await UploadFile();
            var oldTransforming = await UploadFile();
            var oldUploaded = await UploadFile();
            var oldRetry = await UploadFile();
            var oldCanceled = await UploadFile();

            var recentErrorLoading = await UploadFile();
            var recentErrorTransform = await UploadFile();
            var recentErrorUploaded = await UploadFile();
            var recentLoaded = await UploadFile();
            var recentLoading = await UploadFile();
            var recentTransforming = await UploadFile();
            var recentUploaded = await UploadFile();
            var recentRetry = await UploadFile();
            var recentCanceled = await UploadFile();

            SetFile(oldErrorLoading.Id, FileStatus.ErrorLoading, old.Subtract(TimeSpan.FromSeconds(1)));
            SetFile(oldErrorTransform.Id, FileStatus.ErrorTransform, old.Subtract(TimeSpan.FromSeconds(2)));
            SetFile(oldErrorUploaded.Id, FileStatus.ErrorUploaded, old.Subtract(TimeSpan.FromSeconds(3)));
            SetFile(oldLoaded.Id, FileStatus.Loaded, old.Subtract(TimeSpan.FromSeconds(4)));
            SetFile(oldLoading.Id, FileStatus.Loading, old.Subtract(TimeSpan.FromSeconds(5)));
            SetFile(oldTransforming.Id, FileStatus.Transforming, old.Subtract(TimeSpan.FromSeconds(6)));
            SetFile(oldUploaded.Id, FileStatus.Uploaded, old.Subtract(TimeSpan.FromSeconds(7)));
            SetFile(oldRetry.Id, FileStatus.Retry, old.Subtract(TimeSpan.FromSeconds(8)));
            SetFile(oldCanceled.Id, FileStatus.Canceled, old.Subtract(TimeSpan.FromSeconds(9)));

            SetFile(recentErrorLoading.Id, FileStatus.ErrorLoading, recent.Add(TimeSpan.FromSeconds(1)));
            SetFile(recentErrorTransform.Id, FileStatus.ErrorTransform, recent.Add(TimeSpan.FromSeconds(2)));
            SetFile(recentErrorUploaded.Id, FileStatus.ErrorUploaded, recent.Add(TimeSpan.FromSeconds(3)));
            SetFile(recentLoaded.Id, FileStatus.Loaded, recent.Add(TimeSpan.FromSeconds(4)));
            SetFile(recentLoading.Id, FileStatus.Loading, recent.Add(TimeSpan.FromSeconds(5)));
            SetFile(recentTransforming.Id, FileStatus.Transforming, recent.Add(TimeSpan.FromSeconds(6)));
            SetFile(recentUploaded.Id, FileStatus.Uploaded, recent.Add(TimeSpan.FromSeconds(7)));
            SetFile(recentRetry.Id, FileStatus.Retry, recent.Add(TimeSpan.FromSeconds(8)));
            SetFile(recentCanceled.Id, FileStatus.Canceled, recent.Add(TimeSpan.FromSeconds(9)));

            var files = (await Send(new GetActivity.Query())).Files;

            // oldLoaded is omitted because Loaded files are only worth displaying
            // if they are recent activity. Otherwise the activity log would grow
            // indefinitely.
            // oldCanceled and recentCanceled are both omitted, because canceled
            // files are deleted and there is no meaningful activity for the user
            // to take for them.
            // Everything else is either a problem to resolve, pending work, or recently-loaded
            // files, all of which should be seen, displayed descending chronologically.
            var expectedFiles = new string[] {
                recentRetry.FileName,
                recentUploaded.FileName,
                recentTransforming.FileName,
                recentLoading.FileName,
                recentLoaded.FileName,
                recentErrorUploaded.FileName,
                recentErrorTransform.FileName,
                recentErrorLoading.FileName,
                oldErrorLoading.FileName,
                oldErrorTransform.FileName,
                oldErrorUploaded.FileName,
                oldLoading.FileName,
                oldTransforming.FileName,
                oldUploaded.FileName,
                oldRetry.FileName
                };

            foreach (var expected in expectedFiles)
            {
                files.Select(file => file.FileName)
                  .ShouldContain(expected);
            };

            var notExpectedFiles = new string[] {
                oldLoaded.FileName,
                oldCanceled.FileName,
                recentCanceled.FileName
                };

            foreach (var notExpected in notExpectedFiles)
            {
                files.Select(file => file.FileName)
                  .ShouldNotContain(notExpected);
            };
        }

        [Test]
        public async Task ShouldAllowFilteringByApiConnection()
        {
            Transaction(database =>
            {
                database.RemoveRange(database.Files.ToList());
            });

            var sevenDaysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var recent = sevenDaysAgo.Add(TimeSpan.FromDays(1));

            var recentErrorLoading = await UploadFile();
            var recentErrorTransform = await UploadFile();
            var recentErrorUploaded = await UploadFile();
            var recentLoaded = await UploadFile();
            var recentLoading = await UploadFile();
            var recentTransforming = await UploadFile();
            var recentUploaded = await UploadFile();
            var recentRetry = await UploadFile();
            var recentCanceled = await UploadFile();

            var apiServer = await AddApiServer();
            Transaction(database =>
            {
                var a = database.Agents.Single(x => x.Id == recentRetry.AgentId);
                a.ApiServerId = apiServer.Id;
            });

            SetFile(recentErrorLoading.Id, FileStatus.ErrorLoading, recent.Add(TimeSpan.FromSeconds(1)));
            SetFile(recentErrorTransform.Id, FileStatus.ErrorTransform, recent.Add(TimeSpan.FromSeconds(2)));
            SetFile(recentErrorUploaded.Id, FileStatus.ErrorUploaded, recent.Add(TimeSpan.FromSeconds(3)));
            SetFile(recentLoaded.Id, FileStatus.Loaded, recent.Add(TimeSpan.FromSeconds(4)));
            SetFile(recentLoading.Id, FileStatus.Loading, recent.Add(TimeSpan.FromSeconds(5)));
            SetFile(recentTransforming.Id, FileStatus.Transforming, recent.Add(TimeSpan.FromSeconds(6)));
            SetFile(recentUploaded.Id, FileStatus.Uploaded, recent.Add(TimeSpan.FromSeconds(7)));
            SetFile(recentRetry.Id, FileStatus.Retry, recent.Add(TimeSpan.FromSeconds(8)));
            SetFile(recentCanceled.Id, FileStatus.Canceled, recent.Add(TimeSpan.FromSeconds(9)));

            var elements = new string[] {
                recentRetry.FileName,
                recentUploaded.FileName,
                recentTransforming.FileName,
                recentLoading.FileName,
                recentLoaded.FileName,
                recentErrorUploaded.FileName,
                recentErrorTransform.FileName,
                recentErrorLoading.FileName
                };

            var files = (await Send(new GetActivity.Query())).Files;

            foreach (var element in elements)
            {
                files.Select(file => file.FileName)
                  .ShouldContain(element);
            };

            files = (await Send(new GetActivity.Query { ApiServerId = apiServer.Id })).Files;

            files.Select(x => x.FileName)
                .ShouldMatch(
                    recentRetry.FileName
                );
        }

        private static void SetFile(int fileId, FileStatus status, DateTimeOffset created)
        {
            Transaction(database =>
            {
                var file = database.Files.Single(x => x.Id == fileId);
                file.Status = status;
                file.CreateDate = created;
            });
        }

        private static void ClearExistingRecords()
        {
            Transaction(database =>
            {
                foreach (var record in database.JobStatus)
                    database.Remove(record);
            });
        }

        private static void Save(JobStatus desiredState)
        {
            Transaction(database =>
            {
                var actual = database.EnsureSingle<JobStatus>();
                actual.Started = desiredState.Started;
                actual.Completed = desiredState.Completed;
            });
        }
    }
}