// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddApiVersionIdToDataMaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiVersionId",
                table: "DataMaps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE dm
SET ApiVersionId = q.ApiVersionId
FROM 
	dbo.DataMaps dm
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
                name: "IX_DataMaps_ApiVersionId",
                table: "DataMaps",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiVersions_ApiVersion",
                table: "ApiVersions",
                column: "ApiVersion",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DataMaps_ApiVersions_ApiVersionId",
                table: "DataMaps",
                column: "ApiVersionId",
                principalTable: "ApiVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataMaps_ApiVersions_ApiVersionId",
                table: "DataMaps");

            migrationBuilder.DropIndex(
                name: "IX_DataMaps_ApiVersionId",
                table: "DataMaps");

            migrationBuilder.DropIndex(
                name: "IX_ApiVersions_ApiVersion",
                table: "ApiVersions");

            migrationBuilder.DropColumn(
                name: "ApiVersionId",
                table: "DataMaps");
        }
    }
}
