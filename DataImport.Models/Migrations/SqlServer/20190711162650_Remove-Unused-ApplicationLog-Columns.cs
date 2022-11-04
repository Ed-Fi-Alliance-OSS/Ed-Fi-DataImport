// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class RemoveUnusedApplicationLogColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Callsite",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "Https",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "SiteName",
                table: "ApplicationLogs");

            //these stored procedure sql commands were added manually
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[ApplicationLog_AddEntry]");

            migrationBuilder.Sql(
                @"CREATE PROCEDURE [dbo].[ApplicationLog_AddEntry]
    @machineName [nvarchar](200),
    @logged [datetimeoffset](7),
    @level [varchar](5),
    @userName [nvarchar](200),
    @message [nvarchar](max),
    @logger [nvarchar](300),
    @properties [nvarchar](max),
    @serverName [nvarchar](200),
    @port [nvarchar](100),
    @url [nvarchar](2000),
    @serverAddress [nvarchar](100),
    @remoteAddress [nvarchar](100),
    @exception [nvarchar](max)
AS
BEGIN
    INSERT INTO [dbo].[ApplicationLogs] (
    [MachineName],
    [Logged],
    [Level],
    [UserName],
    [Message],
    [Logger],
    [Properties],
    [ServerName],
    [Port],
    [Url],
    [ServerAddress],
    [RemoteAddress],
    [Exception]
    ) VALUES (
    @machineName,
    @logged,
    @level,
    @userName,
    @message,
    @logger,
    @properties,
    @serverName,
    @port,
    @url,
    @serverAddress,
    @remoteAddress,
    @exception
    );
END"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Callsite",
                table: "ApplicationLogs",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Https",
                table: "ApplicationLogs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteName",
                table: "ApplicationLogs",
                maxLength: 200,
                nullable: true);
        }
    }
}
