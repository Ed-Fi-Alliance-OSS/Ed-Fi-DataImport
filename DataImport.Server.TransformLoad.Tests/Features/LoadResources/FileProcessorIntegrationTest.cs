// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.LoadResources;
using DataImport.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static DataImport.Server.TransformLoad.Tests.Testing;
using static DataImport.TestHelpers.TestHelpers;
using File = DataImport.Models.File;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    [TestFixture]
    [SetCulture("en-US")]
    public class FileProcessorIntegrationTest
    {
        private readonly string _bootstrapData = ReadTestFile("TestFiles/bootstrapData.json");
        private readonly string _datamapData = ReadTestFile("TestFiles/datamap.json");

        private readonly string _bootstrapMetadata = ReadTestFile("TestFiles/bootstrapMetadata.json");
        private readonly string _datamapMetadata = ReadTestFile("TestFiles/datamapMetadata.json");

        private readonly string _expectedRow1 = ReadTestFile("TestFiles/expectedRow1.json");
        private readonly string _expectedRow2 = ReadTestFile("TestFiles/expectedRow2.json");

        [Test]
        public async Task ShouldPostContent()
        {
            var referenceFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");

            var uploadedFilePath = SimulateFileUpload(referenceFilePath);

            PerformDataMapAndAgentSetup(uploadedFilePath, out string agentName, out string dataMapName, out int apiServerId);

            var testOdsApi = new TestOdsApi();
            testOdsApi.Config.ApiServerId = apiServerId;

            var bootstrapResponse = await Send(new PostBootstrapData.Command { OdsApi = testOdsApi, CheckMetadata = false });

            bootstrapResponse.Success.ShouldBe(true);

            System.IO.File.Exists(uploadedFilePath).ShouldBeTrue();
            await Send(new FileProcessor.Command { OdsApi = testOdsApi, CheckMetadata = false });
            System.IO.File.Exists(uploadedFilePath).ShouldBeFalse();

            testOdsApi.PostedBootstrapData
                .ShouldMatch(new TestOdsApi.SimulatedPost("http://test-ods-v2.5.0.1.example.com/api/v2.0/2019/assessments", _bootstrapData));

            testOdsApi.PostedContent
                .ShouldMatch(
                    new TestOdsApi.SimulatedPost("http://test-ods-v2.5.0.1.example.com/api/v2.0/2019/studentAssessments", NormalizeLineEndings(_expectedRow1)),
                    new TestOdsApi.SimulatedPost("http://test-ods-v2.5.0.1.example.com/api/v2.0/2019/studentAssessments", NormalizeLineEndings(_expectedRow2)));

            string NormalizeLineEndings(string content)
            {
                // The PostedContent expected data might only have \n depending on Git configuration.
                // Normalize to have \r\n, on the assumption that the actual results are being
                // generated on a Windows machine.
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? content.Replace("\n", "\r\n").Replace("\r\r", "\r")
                    : content;
            }

            var apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).Single(x => x.Id == apiServerId));
            var ingestionLogs = Query(d => d.IngestionLogs.Where(x => x.AgentName == agentName).ToList());
            ingestionLogs.ShouldNotBeEmpty();
            ingestionLogs.ShouldAllBe(x => x.ApiServerName == apiServer.Name);
            ingestionLogs.ShouldAllBe(x => x.ApiVersion == apiServer.ApiVersion.Version);
            ingestionLogs.ShouldAllBe(x => x.Level == LogLevel.Information);
        }

        [Test]
        public async Task ShouldProcessFilesInAgentOrder()
        {
            var referenceFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");

            var uploadedFilePath = SimulateFileUpload(referenceFilePath);

            var apiServer = GetDefaultApiServer();
            var studentAssessmentsMetadata = SwaggerMetadataParser.Parse("/studentAssessments", _datamapMetadata);

            var dataMapName = SampleString();
            var dataMap = new DataMap
            {
                Name = dataMapName,
                Map = _datamapData,
                CreateDate = DateTimeOffset.Now,
                Metadata = ResourceMetadata.Serialize(studentAssessmentsMetadata),
                ResourcePath = "/studentAssessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            var unorderedAgent = new Agent
            {
                Name = SampleString("UnorderedAgent"),
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id,
                RunOrder = null,
            };

            var unorderedFile = new File
            {
                Agent = unorderedAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            var firstAgent = new Agent
            {
                Name = SampleString("FirstAgent"),
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id,
                RunOrder = 1,
            };

            var firstFile = new File
            {
                Agent = firstAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            var secondAgent = new Agent
            {
                Name = SampleString("SecondAgent"),
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id,
                RunOrder = 2,
            };

            var secondFile = new File
            {
                Agent = secondAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            Transaction(database =>
            {
                database.Database.ExecuteSqlRaw("UPDATE Files SET Status =  " + (int) FileStatus.Loaded);
                database.DataMaps.Add(dataMap);
                database.Files.AddRange(secondFile, unorderedFile, firstFile);
                database.DataMapAgents.Add(new DataMapAgent { Agent = secondAgent, DataMap = dataMap });
                database.DataMapAgents.Add(new DataMapAgent { Agent = unorderedAgent, DataMap = dataMap });
                database.DataMapAgents.Add(new DataMapAgent { Agent = firstAgent, DataMap = dataMap });
                database.SaveChanges();
            });

            var testOdsApi = new TestOdsApi();
            testOdsApi.Config.ApiServerId = apiServer.Id;

            System.IO.File.Exists(uploadedFilePath).ShouldBeTrue();
            await Send(new FileProcessor.Command { OdsApi = testOdsApi, CheckMetadata = false });
            System.IO.File.Exists(uploadedFilePath).ShouldBeFalse();

            var firstAgentFiles = GetLoggedAgentFiles(firstAgent.Id).ToList();
            var secondAgentFiles = GetLoggedAgentFiles(secondAgent.Id).ToList();
            var unorderedAgentFiles = GetLoggedAgentFiles(unorderedAgent.Id).ToList();

            // Assert
            foreach (var secondAgentFile in secondAgentFiles)
            {
                //first before second
                firstAgentFiles.ShouldAllBe(firstAgentFile =>
                    firstAgentFile.UpdateDate.Value < secondAgentFile.UpdateDate.Value);
                //unordered after second
                unorderedAgentFiles.ShouldAllBe(unorderedAgentFile =>
                    unorderedAgentFile.UpdateDate.Value > secondAgentFile.UpdateDate.Value);
            }
        }

        [Test]
        public async Task ShouldProcessFilesOnlyForEnabledAgents()
        {
            var referenceFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");

            var uploadedFilePath = SimulateFileUpload(referenceFilePath);

            var apiServer = GetDefaultApiServer();

            var studentAssessmentsMetadata = SwaggerMetadataParser.Parse("/studentAssessments", _datamapMetadata);
            var assessmentsMetadata = SwaggerMetadataParser.Parse("/assessments", _bootstrapMetadata);

            var enabledAgentBootstrapData = new BootstrapData
            {
                Name = SampleString(),
                CreateDate = DateTimeOffset.Now,
                Data = _bootstrapData,
                Metadata = ResourceMetadata.Serialize(assessmentsMetadata),
                ResourcePath = "/assessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            var disabledAgentBootstrapData = new BootstrapData
            {
                Name = SampleString(),
                CreateDate = DateTimeOffset.Now,
                Data = _bootstrapData,
                Metadata = ResourceMetadata.Serialize(assessmentsMetadata),
                ResourcePath = "/assessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            var dataMapName = SampleString();
            var dataMap = new DataMap
            {
                Name = dataMapName,
                Map = _datamapData,
                CreateDate = DateTimeOffset.Now,
                Metadata = ResourceMetadata.Serialize(studentAssessmentsMetadata),
                ResourcePath = "/studentAssessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            var enabledAgent = new Agent
            {
                Name = SampleString("EnabledAgent"),
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id,
            };

            var enabledAgentFile = new File
            {
                Agent = enabledAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            var disabledAgent = new Agent
            {
                Name = SampleString("DisabledAgent"),
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = false,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id
            };

            var disabledAgentFile = new File
            {
                Agent = disabledAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };


            using (var scope = Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DataImportDbContext>();
                context.Database.ExecuteSqlRaw("UPDATE Files SET Status =  " + (int) FileStatus.Loaded);
                context.DataMaps.Add(dataMap);
                context.BootstrapDatas.AddRange(enabledAgentBootstrapData, disabledAgentBootstrapData);
                context.Files.AddRange(enabledAgentFile, disabledAgentFile);
                context.DataMapAgents.Add(new DataMapAgent { Agent = enabledAgent, DataMap = dataMap });
                context.DataMapAgents.Add(new DataMapAgent { Agent = disabledAgent, DataMap = dataMap });
                context.BootstrapDataAgents.Add(new BootstrapDataAgent { Agent = enabledAgent, BootstrapData = enabledAgentBootstrapData });
                context.BootstrapDataAgents.Add(new BootstrapDataAgent { Agent = disabledAgent, BootstrapData = disabledAgentBootstrapData });
                context.SaveChanges();
            }

            var testOdsApi = new TestOdsApi();
            testOdsApi.Config.ApiServerId = apiServer.Id;

            System.IO.File.Exists(uploadedFilePath).ShouldBeTrue();
            await Send(new FileProcessor.Command { OdsApi = testOdsApi, CheckMetadata = false });
            System.IO.File.Exists(uploadedFilePath).ShouldBeFalse();

            var bootstrapResponse = await Send(new PostBootstrapData.Command { OdsApi = testOdsApi, CheckMetadata = false });
            bootstrapResponse.Success.ShouldBe(true);

            apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).Single(x => x.Id == apiServer.Id));
            var ingestionLogsForEnabledAgent = Query(d => d.IngestionLogs.Where(x => x.AgentName == enabledAgent.Name).ToList());
            ingestionLogsForEnabledAgent.ShouldNotBeEmpty();
            ingestionLogsForEnabledAgent.ShouldAllBe(x => x.ApiServerName == apiServer.Name);
            ingestionLogsForEnabledAgent.ShouldAllBe(x => x.ApiVersion == apiServer.ApiVersion.Version);
            var bootstrapLogsForEnabledAgent = ingestionLogsForEnabledAgent.Where(i => i.Operation == "PostBootstrapData").ToList();
            bootstrapLogsForEnabledAgent.ShouldNotBeEmpty();

            var ingestionLogsForDisabledAgent = Query(d => d.IngestionLogs.Where(x => x.AgentName == disabledAgent.Name).ToList());
            ingestionLogsForDisabledAgent.ShouldBeEmpty();
            var bootstrapLogsForDisabledAgent = ingestionLogsForDisabledAgent.Where(i => i.Operation == "PostBootstrapData").ToList();
            bootstrapLogsForDisabledAgent.ShouldBeEmpty();
        }

        [Test]
        public async Task ShouldLogFailedPost()
        {
            var referenceFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");

            var uploadedFilePath = SimulateFileUpload(referenceFilePath);

            var newApiServer = SaveNewApiServer();

            PerformDataMapAndAgentSetup(uploadedFilePath, out string agentName, out string dataMapName, out int apiServerId, newApiServer);

            var testOdsApi = new TestFailingOdsApi { Config = { ApiServerId = apiServerId } };

            var bootstrapResponse = await Send(new PostBootstrapData.Command { OdsApi = testOdsApi, CheckMetadata = false });

            bootstrapResponse.Success.ShouldBe(false);

            var apiServer = Query(d => d.ApiServers.Include(x => x.ApiVersion).Single(x => x.Id == apiServerId));
            var ingestionLogs = Query(d => d.IngestionLogs.Where(x => x.AgentName == agentName).ToList());
            ingestionLogs.ShouldNotBeEmpty();
            ingestionLogs.ShouldAllBe(x => x.ApiServerName == apiServer.Name);
            ingestionLogs.ShouldAllBe(x => x.ApiVersion == apiServer.ApiVersion.Version);
            var bootstrapLogs = ingestionLogs.Where(i => i.Operation == "PostBootstrapData").ToList();
            bootstrapLogs.ShouldAllBe(b => b.Level == LogLevel.Error);
            bootstrapLogs.ShouldAllBe(b => !string.IsNullOrEmpty(b.Data));
        }

        private void PerformDataMapAndAgentSetup(string uploadedFilePath, out string agentName, out string dataMapName, out int apiServerId, ApiServer overrideApiServer = null)
        {
            var apiServer = overrideApiServer ?? GetDefaultApiServer();
            apiServerId = apiServer.Id;

            var assessmentsMetadata = SwaggerMetadataParser.Parse("/assessments", _bootstrapMetadata);

            var bootstrapData = new BootstrapData
            {
                Name = SampleString(),
                CreateDate = DateTimeOffset.Now,
                Data = _bootstrapData,
                Metadata = ResourceMetadata.Serialize(assessmentsMetadata),
                ResourcePath = "/assessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            var studentAssessmentsMetadata = SwaggerMetadataParser.Parse("/studentAssessments", _datamapMetadata);

            dataMapName = SampleString();
            var dataMap = new DataMap
            {
                Name = dataMapName,
                Map = _datamapData,
                CreateDate = DateTimeOffset.Now,
                Metadata = ResourceMetadata.Serialize(studentAssessmentsMetadata),
                ResourcePath = "/studentAssessments",
                ApiVersionId = apiServer.ApiVersionId
            };

            agentName = SampleString("Name");
            var agent = new Agent
            {
                Name = agentName,
                AgentTypeCode = AgentTypeCodeEnum.Manual,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServer.Id
            };

            var file = new File
            {
                Agent = agent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            Transaction(database =>
            {
                database.Database.ExecuteSqlRaw("UPDATE Files SET Status =  " + (int) FileStatus.Loaded);
                database.BootstrapDatas.Add(bootstrapData);
                database.DataMaps.Add(dataMap);
                database.Files.Add(file);
                database.DataMapAgents.Add(new DataMapAgent { Agent = agent, DataMap = dataMap });
                database.BootstrapDataAgents.Add(new BootstrapDataAgent { Agent = agent, BootstrapData = bootstrapData });
                database.SaveChanges();
            });
        }
        //This test fail if the test name is changed. Looking into it
        [Test]
        [TestCase(AgentActionsFile.DeleteOnSuccessful, AgentTypeCodeEnum.Ftps, false)]
        [TestCase(AgentActionsFile.NeverDelete, AgentTypeCodeEnum.Sftp, true)]
        [TestCase(AgentActionsFile.AlwaysDelete, AgentTypeCodeEnum.Ftps, false)]
        public async Task ShouldTempFilesBeDeleted_When_AgentFileActionIsSelected(AgentActionsFile fileAction, string agentTypeCode, bool expectedResult)
        {
            var referenceFilePath = Path.Combine(GetAssemblyPath(), "TestFiles/testing.csv");

            var uploadedFilePath = SimulateFileUpload(referenceFilePath);

            var apiServer = GetDefaultApiServer();

            PerformDataMapAndAgentFTPSetup(fileAction, agentTypeCode, apiServer.Id, uploadedFilePath);
            var testOdsApi = new TestOdsApi { Config = { ApiServerId = apiServer.Id } };
            await Send(new FileProcessor.Command { OdsApi = testOdsApi, CheckMetadata = false });
            System.IO.File.Exists(uploadedFilePath).ShouldBe(expectedResult);
        }
        private ApiServer SaveNewApiServer()
        {
            var version = Query(x => x.ApiVersions.First());

            var apiServer = new ApiServer
            {
                Name = SampleString("ApiServer"),
                ApiVersionId = version.Id,
                Url = TestFailingOdsApi.ConfigUrlDefault,
                TokenUrl = TestFailingOdsApi.ConfigUrlDefault,
                AuthUrl = TestFailingOdsApi.ConfigUrlDefault,
                Key = SampleString("testKey"),
                Secret = SampleString("testSecret")
            };

            Transaction(database =>
            {
                database.ApiServers.Add(apiServer);
            });

            return apiServer;
        }
        private void PerformDataMapAndAgentFTPSetup(AgentActionsFile fileAction, string agentTypeCode, int apiServerId, string uploadedFilePath)
        {
            var studentAssessmentsMetadata = SwaggerMetadataParser.Parse("/studentAssessments", _datamapMetadata);
            var assessmentsMetadata = SwaggerMetadataParser.Parse("/assessments", _bootstrapMetadata);

            var enabledAgentBootstrapData = new BootstrapData
            {
                Name = SampleString(),
                CreateDate = DateTimeOffset.Now,
                Data = _bootstrapData,
                Metadata = ResourceMetadata.Serialize(assessmentsMetadata),
                ResourcePath = "/assessments",
                ApiVersionId = apiServerId
            };

            var dataMapName = SampleString();
            var dataMap = new DataMap
            {
                Name = dataMapName,
                Map = _datamapData,
                CreateDate = DateTimeOffset.Now,
                Metadata = ResourceMetadata.Serialize(studentAssessmentsMetadata),
                ResourcePath = "/studentAssessments",
                ApiVersionId = apiServerId
            };

            var enabledAgent = new Agent
            {
                Name = SampleString("EnabledAgent"),
                AgentTypeCode = agentTypeCode,
                Enabled = true,
                Created = DateTimeOffset.Now,
                ApiServerId = apiServerId,
                ActionFileCode = fileAction.ToString(),
            };

            var enabledAgentFile = new File
            {
                Agent = enabledAgent,
                CreateDate = DateTimeOffset.Now,
                FileName = "testing.csv",
                Rows = 2,
                Status = FileStatus.Uploaded,
                Url = new Uri(uploadedFilePath).AbsoluteUri
            };

            using (var scope = Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DataImportDbContext>();
                context.Database.ExecuteSqlRaw($"UPDATE Files SET Status = {(int) FileStatus.Loaded}");
                context.DataMaps.Add(dataMap);
                context.BootstrapDatas.AddRange(enabledAgentBootstrapData);
                context.Files.AddRange(enabledAgentFile);
                context.DataMapAgents.Add(new DataMapAgent { Agent = enabledAgent, DataMap = dataMap });
                context.BootstrapDataAgents.Add(new BootstrapDataAgent { Agent = enabledAgent, BootstrapData = enabledAgentBootstrapData });
                context.SaveChanges();
            }
        }
        private static string SimulateFileUpload(string referenceFilePath)
        {
            var shareFolder = Path.Combine(GetAssemblyPath(), "Uploaded");
            Directory.CreateDirectory(shareFolder);

            var uploadedFilePath = Path.Combine(shareFolder, "testing.csv");
            System.IO.File.Copy(referenceFilePath, uploadedFilePath, overwrite: true);

            return uploadedFilePath;
        }
    }
}
