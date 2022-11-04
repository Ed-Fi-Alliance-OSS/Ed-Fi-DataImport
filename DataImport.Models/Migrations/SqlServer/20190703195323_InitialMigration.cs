// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    AgentTypeCode = table.Column<string>(maxLength: 50, nullable: true),
                    AgentAction = table.Column<string>(maxLength: 50, nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    Directory = table.Column<string>(nullable: true),
                    FilePattern = table.Column<string>(nullable: true),
                    Queue = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    LastExecuted = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiServers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Secret = table.Column<string>(nullable: false),
                    ApiVersion = table.Column<string>(maxLength: 20, nullable: false),
                    TokenUrl = table.Column<string>(maxLength: 255, nullable: false),
                    AuthUrl = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDatas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ResourceName = table.Column<string>(maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: false),
                    ProcessingOrder = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(nullable: true),
                    ProcessedDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InstanceOrganizationUrl = table.Column<string>(nullable: true),
                    InstanceOrganizationLogo = table.Column<string>(nullable: true),
                    InstanceEduUseText = table.Column<string>(nullable: true),
                    InstanceAllowUserRegistration = table.Column<bool>(nullable: false, defaultValue: false),
                    JanitorReportLast = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataMaps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    ResourceName = table.Column<string>(maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(nullable: false),
                    Map = table.Column<string>(nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(nullable: true),
                    CsvColumnHeaders = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GroupSet = table.Column<string>(maxLength: 1024, nullable: false),
                    Key = table.Column<string>(maxLength: 1024, nullable: false),
                    Value = table.Column<string>(maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MachineName = table.Column<string>(maxLength: 200, nullable: true),
                    SiteName = table.Column<string>(maxLength: 200, nullable: true),
                    Logged = table.Column<DateTimeOffset>(nullable: false),
                    Level = table.Column<string>(maxLength: 5, nullable: false),
                    UserName = table.Column<string>(maxLength: 200, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Logger = table.Column<string>(maxLength: 300, nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    ServerName = table.Column<string>(maxLength: 200, nullable: true),
                    Port = table.Column<string>(maxLength: 100, nullable: true),
                    Url = table.Column<string>(maxLength: 2000, nullable: true),
                    Https = table.Column<bool>(nullable: true),
                    ServerAddress = table.Column<string>(maxLength: 100, nullable: true),
                    RemoteAddress = table.Column<string>(nullable: true),
                    Callsite = table.Column<string>(maxLength: 300, nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false),
                    Hour = table.Column<int>(nullable: false),
                    Minute = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentSchedules_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    AgentId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Rows = table.Column<int>(nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogIngestions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EducationOrganizationId = table.Column<Guid>(nullable: true),
                    Level = table.Column<string>(maxLength: 255, nullable: true),
                    Operation = table.Column<string>(maxLength: 255, nullable: true),
                    AgentId = table.Column<int>(nullable: true),
                    Process = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Result = table.Column<int>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    RowNumber = table.Column<string>(nullable: true),
                    EndPointUrl = table.Column<string>(nullable: true),
                    HttpStatusCode = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    OdsResponse = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogIngestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogIngestions_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataMapAgents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DataMapId = table.Column<int>(nullable: false),
                    AgentId = table.Column<int>(nullable: false),
                    ProcessingOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMapAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_DataMaps_DataMapId",
                        column: x => x.DataMapId,
                        principalTable: "DataMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedules_AgentId",
                table: "AgentSchedules",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_AgentId",
                table: "DataMapAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_DataMapId",
                table: "DataMapAgents",
                column: "DataMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AgentId",
                table: "Files",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LogIngestions_AgentId",
                table: "LogIngestions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LogIngestions_Result",
                table: "LogIngestions",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name",
                unique: true);

            //this stored procedure sql command was added manually
            migrationBuilder.Sql(
                @"CREATE PROCEDURE [dbo].[NLog_AddEntry]
    @machineName [nvarchar](200),
    @siteName [nvarchar](200),
    @logged [datetimeoffset](7),
    @level [varchar](5),
    @userName [nvarchar](200),
    @message [nvarchar](max),
    @logger [nvarchar](300),
    @properties [nvarchar](max),
    @serverName [nvarchar](200),
    @port [nvarchar](100),
    @url [nvarchar](2000),
    @https [bit],
    @serverAddress [nvarchar](100),
    @remoteAddress [nvarchar](100),
    @callSite [nvarchar](300),
    @exception [nvarchar](max)
AS
BEGIN
    INSERT INTO [dbo].[NLog] (
    [MachineName],
    [SiteName],
    [Logged],
    [Level],
    [UserName],
    [Message],
    [Logger],
    [Properties],
    [ServerName],
    [Port],
    [Url],
    [Https],
    [ServerAddress],
    [RemoteAddress],
    [CallSite],
    [Exception]
    ) VALUES (
    @machineName,
    @siteName,
    @logged,
    @level,
    @userName,
    @message,
    @logger,
    @properties,
    @serverName,
    @port,
    @url,
    @https,
    @serverAddress,
    @remoteAddress,
    @callSite,
    @exception
    );
END"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSchedules");

            migrationBuilder.DropTable(
                name: "ApiServers");

            migrationBuilder.DropTable(
                name: "BootstrapDatas");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DataMapAgents");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "LogIngestions");

            migrationBuilder.DropTable(
                name: "Lookups");

            migrationBuilder.DropTable(
                name: "NLog");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "DataMaps");

            migrationBuilder.DropTable(
                name: "Agents");
        }
    }
}
