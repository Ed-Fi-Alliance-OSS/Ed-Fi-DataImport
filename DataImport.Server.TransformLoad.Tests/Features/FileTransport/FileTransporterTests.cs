// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.FileTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Server.TransformLoad.Tests.Testing;

namespace DataImport.Server.TransformLoad.Tests.Features.FileTransport
{
    [TestFixture]
    public class FileTransporterTests
    {
        private Agent _ftpsAgent;
        private Agent _sftpAgent;
        private List<AgentSchedule> _agentSchedules;
        private string _ftpsAgentName;
        private string _sftpAgentName;
        private AppSettings _appSettings;

        [SetUp]
        public void Init()
        {
            var apiServer = GetDefaultApiServer();

            Transaction(database =>
            {
                foreach (var record in database.Agents)
                    record.Enabled = false;
            });

            var schedule = DateTime.Now.AddMinutes(-15);
            _agentSchedules = new List<AgentSchedule>
            {
                new AgentSchedule
                {
                    Day = (int) schedule.DayOfWeek,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute
                }
            };
            _ftpsAgentName = SampleString("ftpsAgent");
            _ftpsAgent = new Agent
            {
                Name = _ftpsAgentName,
                AgentTypeCode = AgentTypeCodeEnum.Ftps,
                Url = "127.0.0.1",
                Username = "username",
                Password = "password",
                Directory = "test/",
                FilePattern = "*.csv",
                ApiServerId = apiServer.Id
            };

            _sftpAgentName = SampleString("sftpAgent");
            _sftpAgent = new Agent
            {
                Name = _sftpAgentName,
                AgentTypeCode = AgentTypeCodeEnum.Sftp,
                Url = "172.0.0.33",
                Username = "username",
                Password = "password",
                Directory = "test/",
                FilePattern = "*.csv",
                ApiServerId = apiServer.Id
            };

            _appSettings = Services.GetService<IOptions<AppSettings>>().Value;
        }

        [Test]
        public async Task Should_Log_Files_Successfully_For_Enabled_FTPS_Agent_With_Schedule()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = _agentSchedules;

            var agentId = AddAgent(_ftpsAgent);

            _ftpsAgent.ApiServerId.ShouldNotBeNull();
            await Send(new FileTransporter.Command { ApiServerId = _ftpsAgent.ApiServerId.Value });

            var result = GetLoggedAgentFiles(agentId).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result[0].FileName.ShouldBe("FtpsFile1");
            result[1].FileName.ShouldBe("FtpsFile2");
        }

        [Test]
        public async Task Should_Log_Files_Successfully_For_Enabled_SFTP_Agent_With_Schedule()
        {
            _sftpAgent.Enabled = true;
            _sftpAgent.AgentSchedules = _agentSchedules;

            var agentId = AddAgent(_sftpAgent);

            _sftpAgent.ApiServerId.ShouldNotBeNull();
            await Send(new FileTransporter.Command { ApiServerId = _sftpAgent.ApiServerId.Value });

            var result = GetLoggedAgentFiles(agentId).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result[0].FileName.ShouldBe("SftpFile1");
            result[1].FileName.ShouldBe("SftpFile2");
        }

        [Test]
        public async Task Should_Store_Files_For_Enabled_FTPS_Agent_On_Specified_FileMode()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = _agentSchedules;

            var agentId = AddAgent(_ftpsAgent);

            _sftpAgent.ApiServerId.ShouldNotBeNull();
            await Send(new FileTransporter.Command { ApiServerId = _sftpAgent.ApiServerId.Value });
            var result = GetLoggedAgentFiles(agentId).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result[0].FileName.ShouldBe("FtpsFile1");
            result[0].Url.ShouldContainWithoutWhitespace(agentId);
            result[0].Url.ShouldContainWithoutWhitespace(_appSettings.FileMode);
            result[1].FileName.ShouldBe("FtpsFile2");
            result[1].Url.ShouldContainWithoutWhitespace(agentId);
            result[1].Url.ShouldContainWithoutWhitespace(_appSettings.FileMode);
        }

        [Test]
        public async Task Should_Not_Be_Any_Logged_Files_For_Enabled_FTPS_Agent_With_No_Schedule()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = null;

            var agentId = AddAgent(_ftpsAgent);
            await Send(new FileTransporter.Command());

            var result = GetLoggedAgentFiles(agentId).ToList();

            // Assert
            result.ShouldBeEmpty();
        }

        [Test]
        public async Task Should_Transport_Files_In_Defined_Agent_Order()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = _agentSchedules;
            _ftpsAgent.RunOrder = 2;
            var secondAgentId = AddAgent(_ftpsAgent);

            var schedule = DateTime.Now.AddMinutes(-15);
            _sftpAgent.Enabled = true;
            _sftpAgent.AgentSchedules = new List<AgentSchedule> //need a wholly separate schedule
            {
                new AgentSchedule
                {
                    Day = (int) schedule.DayOfWeek,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute
                }
            };
            _sftpAgent.RunOrder = 1;
            var firstAgentId = AddAgent(_sftpAgent);

            _sftpAgent.ApiServerId.ShouldNotBeNull();
            await Send(new FileTransporter.Command { ApiServerId = _sftpAgent.ApiServerId.Value });

            var firstAgentFiles = GetLoggedAgentFiles(firstAgentId).ToList();
            var secondAgentFiles = GetLoggedAgentFiles(secondAgentId).ToList();

            // Assert
            foreach (var secondAgentFile in secondAgentFiles)
            {
                firstAgentFiles.ShouldAllBe(firstAgentFile =>
                    firstAgentFile.CreateDate.Value < secondAgentFile.CreateDate.Value);
            }
        }

        [Test]
        public async Task Should_Transport_Files_In_For_Ordered_Agent_Before_Unordered()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = _agentSchedules;
            _ftpsAgent.RunOrder = null;
            var unorderedAgentId = AddAgent(_ftpsAgent);

            var schedule = DateTime.Now.AddMinutes(-15);
            _sftpAgent.Enabled = true;
            _sftpAgent.AgentSchedules = new List<AgentSchedule> //need a wholly separate schedule
            {
                new AgentSchedule
                {
                    Day = (int) schedule.DayOfWeek,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute
                }
            };
            _sftpAgent.RunOrder = 10;
            var orderedAgentId = AddAgent(_sftpAgent);

            Debug.Assert(_sftpAgent.ApiServerId != null, "_sftpAgent.ApiServerId != null");
            await Send(new FileTransporter.Command { ApiServerId = _sftpAgent.ApiServerId.Value });

            var orderedAgentFiles = GetLoggedAgentFiles(orderedAgentId).ToList();
            var unorderedAgentFiles = GetLoggedAgentFiles(unorderedAgentId).ToList();

            // Assert
            foreach (var unorderedAgentFile in unorderedAgentFiles)
            {
                orderedAgentFiles.ShouldAllBe(firstAgentFile =>
                    firstAgentFile.CreateDate.Value < unorderedAgentFile.CreateDate.Value);
            }
        }

        [Test]
        public async Task Should_Transport_Files_Only_For_Enabled_Agents()
        {
            _ftpsAgent.Enabled = true;
            _ftpsAgent.AgentSchedules = _agentSchedules;
            var enabledAgentId = AddAgent(_ftpsAgent);

            var schedule = DateTime.Now.AddMinutes(-15);
            _sftpAgent.Enabled = false;
            _sftpAgent.AgentSchedules = new List<AgentSchedule> //need a wholly separate schedule
            {
                new AgentSchedule
                {
                    Day = (int) schedule.DayOfWeek,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute
                }
            };
            var disabledAgentId = AddAgent(_sftpAgent);

            Debug.Assert(_sftpAgent.ApiServerId != null, "_sftpAgent.ApiServerId != null");
            await Send(new FileTransporter.Command { ApiServerId = _sftpAgent.ApiServerId.Value });

            var enabledAgentFiles = GetLoggedAgentFiles(enabledAgentId).ToList();
            var disabledAgentFiles = GetLoggedAgentFiles(disabledAgentId).ToList();

            enabledAgentFiles.ShouldNotBeEmpty();
            disabledAgentFiles.ShouldBeEmpty();
        }
    }
}
