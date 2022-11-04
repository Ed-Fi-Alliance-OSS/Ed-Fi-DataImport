// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddApiVersionIdToBootstrapData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiVersionId",
                table: "BootstrapDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE b
SET ApiVersionId = q.ApiVersionId
FROM 
	dbo.BootstrapDatas b
CROSS JOIN 
(
	SELECT 
		TOP 1 [as].ApiVersionId 
	FROM 
		dbo.ApiServers [as]
	ORDER BY 
		[as].Id
) q;
");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDatas_ApiVersionId",
                table: "BootstrapDatas",
                column: "ApiVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BootstrapDatas_ApiVersions_ApiVersionId",
                table: "BootstrapDatas",
                column: "ApiVersionId",
                principalTable: "ApiVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BootstrapDatas_ApiVersions_ApiVersionId",
                table: "BootstrapDatas");

            migrationBuilder.DropIndex(
                name: "IX_BootstrapDatas_ApiVersionId",
                table: "BootstrapDatas");

            migrationBuilder.DropColumn(
                name: "ApiVersionId",
                table: "BootstrapDatas");
        }
    }
}
