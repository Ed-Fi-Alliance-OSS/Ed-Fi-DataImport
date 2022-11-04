// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class ChangeIngestionLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "IngestionLogs",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiServerName",
                table: "IngestionLogs",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiVersion",
                table: "IngestionLogs",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE il
SET 
	ApiVersion = IIF([av].ApiVersion IS NOT NULL, [av].ApiVersion, IIF(il.AgentId IS NOT NULL, d.DefaultApiVersion, NULL)),
	AgentName = a.Name,
	ApiServerName = IIF([as].Name IS NOT NULL, [as].Name, IIF(il.AgentId IS NOT NULL, d.DefaultApiServerName, NULL))
FROM 
	dbo.IngestionLogs il
INNER JOIN 
	dbo.Agents a ON il.AgentId = a.Id
LEFT JOIN 
	dbo.ApiServers [as] ON a.ApiServerId = [as].Id
LEFT JOIN 
	dbo.ApiVersions av ON [as].ApiVersionId = av.id
CROSS JOIN 
(
	SELECT TOP 1 
		   as2.Name AS DefaultApiServerName, 
		   av2.ApiVersion AS DefaultApiVersion
	FROM 
		 dbo.ApiServers as2
	INNER JOIN
		dbo.ApiVersions av2 ON as2.ApiVersionId = av2.Id
	ORDER BY 
			 as2.Id
) as d
");


            migrationBuilder.DropForeignKey(
                name: "FK_IngestionLogs_Agents_AgentId",
                table: "IngestionLogs");

            migrationBuilder.DropIndex(
                name: "IX_IngestionLogs_AgentId",
                table: "IngestionLogs");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "IngestionLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agents",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "IngestionLogs");

            migrationBuilder.DropColumn(
                name: "ApiServerName",
                table: "IngestionLogs");

            migrationBuilder.DropColumn(
                name: "ApiVersion",
                table: "IngestionLogs");

            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "IngestionLogs",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agents",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_AgentId",
                table: "IngestionLogs",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IngestionLogs_Agents_AgentId",
                table: "IngestionLogs",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
