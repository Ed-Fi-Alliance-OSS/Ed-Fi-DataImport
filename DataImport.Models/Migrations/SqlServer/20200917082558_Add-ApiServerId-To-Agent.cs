// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddApiServerIdToAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiServerId",
                table: "Agents",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE dm
SET ApiServerId = q.Id
FROM 
	dbo.Agents dm
CROSS JOIN 
(
	SELECT 
		TOP 1 [as].Id 
	FROM 
		dbo.ApiServers [as]
	ORDER BY 
		[as].Id
) q;
");

            migrationBuilder.CreateTable(
                name: "BootstrapDataAgents",
                columns: table => new
                {
                    AgentId = table.Column<int>(nullable: false),
                    BootstrapDataId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDataAgents", x => new { x.BootstrapDataId, x.AgentId });
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_BootstrapDatas_BootstrapDataId",
                        column: x => x.BootstrapDataId,
                        principalTable: "BootstrapDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
INSERT INTO dbo.BootstrapDataAgents(AgentId, BootstrapDataId)
SELECT 
        a.Id AS AgentId, 
        bd.Id AS BootstrapDataId
FROM
    dbo.BootstrapDatas bd, 
    dbo.Agents a;
");


            migrationBuilder.CreateIndex(
                name: "IX_Agents_ApiServerId",
                table: "Agents",
                column: "ApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataAgents_AgentId",
                table: "BootstrapDataAgents",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_ApiServers_ApiServerId",
                table: "Agents",
                column: "ApiServerId",
                principalTable: "ApiServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_ApiServers_ApiServerId",
                table: "Agents");

            migrationBuilder.DropTable(
                name: "BootstrapDataAgents");

            migrationBuilder.DropIndex(
                name: "IX_Agents_ApiServerId",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "ApiServerId",
                table: "Agents");
        }
    }
}
