// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class ApplySchemaUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogIngestions");

            migrationBuilder.DropTable(
                name: "NLog");

            migrationBuilder.RenameColumn(
                name: "CsvColumnHeaders",
                table: "DataMaps",
                newName: "ColumnHeaders");

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
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
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngestionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
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
                    table.PrimaryKey("PK_IngestionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngestionLogs_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreateDate",
                table: "Files",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Logged",
                table: "ApplicationLogs",
                column: "Logged");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_AgentId",
                table: "IngestionLogs",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Date",
                table: "IngestionLogs",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Result",
                table: "IngestionLogs",
                column: "Result");

            //these stored procedure sql commands were added manually
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[NLog_AddEntry]");

            migrationBuilder.Sql(
                @"CREATE PROCEDURE [dbo].[ApplicationLog_AddEntry]
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
    INSERT INTO [dbo].[ApplicationLogs] (
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
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "IngestionLogs");

            migrationBuilder.DropIndex(
                name: "IX_Files_CreateDate",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "ColumnHeaders",
                table: "DataMaps",
                newName: "CsvColumnHeaders");

            migrationBuilder.CreateTable(
                name: "LogIngestions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    EducationOrganizationId = table.Column<Guid>(nullable: true),
                    EndPointUrl = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    HttpStatusCode = table.Column<string>(nullable: true),
                    Level = table.Column<string>(maxLength: 255, nullable: true),
                    OdsResponse = table.Column<string>(nullable: true),
                    Operation = table.Column<string>(maxLength: 255, nullable: true),
                    Process = table.Column<string>(nullable: true),
                    Result = table.Column<int>(nullable: false),
                    RowNumber = table.Column<string>(nullable: true)
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
                name: "NLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Callsite = table.Column<string>(maxLength: 300, nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Https = table.Column<bool>(nullable: true),
                    Level = table.Column<string>(maxLength: 5, nullable: false),
                    Logged = table.Column<DateTimeOffset>(nullable: false),
                    Logger = table.Column<string>(maxLength: 300, nullable: true),
                    MachineName = table.Column<string>(maxLength: 200, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Port = table.Column<string>(maxLength: 100, nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    RemoteAddress = table.Column<string>(nullable: true),
                    ServerAddress = table.Column<string>(maxLength: 100, nullable: true),
                    ServerName = table.Column<string>(maxLength: 200, nullable: true),
                    SiteName = table.Column<string>(maxLength: 200, nullable: true),
                    Url = table.Column<string>(maxLength: 2000, nullable: true),
                    UserName = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogIngestions_AgentId",
                table: "LogIngestions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LogIngestions_Result",
                table: "LogIngestions",
                column: "Result");
        }
    }
}
