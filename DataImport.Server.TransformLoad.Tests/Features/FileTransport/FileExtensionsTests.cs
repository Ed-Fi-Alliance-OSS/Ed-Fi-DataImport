// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.TestHelpers.TestHelpers;
using static DataImport.Server.TransformLoad.Tests.Testing;

namespace DataImport.Server.TransformLoad.Tests.Features.FileTransport
{
    [TestFixture]
    public class FileExtensionsTests
    {
        private Agent _ftpsAgent;
        private int _agentId;
        private IFileHelper fileHelper;

        [SetUp]
        public void Init()
        {
            var ftpsAgentName = SampleString("ftpsAgent");
            var schedule = DateTime.Now.AddMinutes(-15);
            var apiServer = GetDefaultApiServer();

            _ftpsAgent = new Agent
            {
                Name = ftpsAgentName,
                AgentTypeCode = AgentTypeCodeEnum.FTPS,
                Url = "127.0.0.1",
                Username = "username",
                Password = "password",
                Directory = "test/",
                FilePattern = "*.csv",
                ApiServerId = apiServer.Id
            };
            var agentSchedules = new List<AgentSchedule>
            {
                new AgentSchedule
                {
                    Day = (int) schedule.DayOfWeek,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute
                }
            };
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = agentSchedules;
            _agentId = AddAgent(_ftpsAgent);
            fileHelper = Services.GetService<IFileHelper>();
        }

        [Test]
        public async Task Should_Log_Files_Successfully_For_FTPS_Agent()
        {
            const string fileName = "FtpsFile1";

            var localFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");
            var url = new Uri(localFilePath).AbsoluteUri;

            await fileHelper.LogFileAsync(fileName, _agentId, url, FileStatus.Uploaded, 10);
            var result = GetLoggedAgentFiles(_agentId).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result[0].FileName.ShouldBe(fileName);
            result[0].Url.ShouldBe(url);
            result[0].Status.ShouldBe(FileStatus.Uploaded);
            result[0].Rows.ShouldBe(10);
        }

        [Test]
        public async Task Should_DoesFileExistInLog_For_Existing_Files_Returns_True()
        {
            const string fileName = "FtpsFile1";
            bool doesFileExist = false;

            var localFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");
            var url = new Uri(localFilePath).AbsoluteUri;

            await fileHelper.LogFileAsync(fileName, _agentId, url, FileStatus.Uploaded, 10);

            var result = GetLoggedAgentFiles(_agentId).ToList();

            using (var scope = Services.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetService<DataImportDbContext>();
                doesFileExist = await Helper.DoesFileExistInLog(dbContext, _agentId, fileName);
            }

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result[0].FileName.ShouldBe(fileName);
            doesFileExist.ShouldBeTrue();
        }

        [Test]
        public async Task Should_DoesFileExistInLog_For_No_Existing_Files_Returns_False()
        {
            const string fileName = "FtpsFile1";
            const string notExistingFileName = "FtpsFile2";
            bool doesFileExist = false;

            var localFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");
            var url = new Uri(localFilePath).AbsoluteUri;

            await fileHelper.LogFileAsync(fileName, _agentId, url, FileStatus.Uploaded, 10);

            using (var scope = Services.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetService<DataImportDbContext>();
                doesFileExist = await Helper.DoesFileExistInLog(dbContext, _agentId, notExistingFileName);
            }

            // Assert
            doesFileExist.ShouldBeFalse();
        }
    }
}
