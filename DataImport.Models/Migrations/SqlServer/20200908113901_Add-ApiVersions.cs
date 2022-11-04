// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddApiVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resources_Path",
                table: "Resources");

            migrationBuilder.AddColumn<int>(
                name: "ApiVersionId",
                table: "Resources",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApiVersionId",
                table: "ApiServers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ApiServers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE dbo.ApiServers
SET    
    Name = 'Default API Connection';
");

            migrationBuilder.CreateTable(
                name: "ApiVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApiVersion = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiVersions", x => x.Id);
                });

            migrationBuilder.Sql(@"
INSERT INTO 
    ApiVersions
SELECT 
    ApiVersion
FROM 
    dbo.ApiServers;

UPDATE a
SET 
	a.ApiVersionId = v.Id
FROM
	dbo.ApiServers a
INNER JOIN 
	dbo.ApiVersions v ON a.ApiVersion = v.ApiVersion;

UPDATE a
SET 
	a.ApiVersionId = q.Id
FROM
	dbo.Resources a
CROSS JOIN
	(SELECT TOP 1 Id FROM dbo.ApiVersions) q;
");

            migrationBuilder.DropColumn(
                name: "ApiVersion",
                table: "ApiServers");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ApiVersionId",
                table: "Resources",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path_ApiVersionId",
                table: "Resources",
                columns: new[] { "Path", "ApiVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_ApiVersionId",
                table: "ApiServers",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_Name",
                table: "ApiServers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiServers_ApiVersions_ApiVersionId",
                table: "ApiServers",
                column: "ApiVersionId",
                principalTable: "ApiVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_ApiVersions_ApiVersionId",
                table: "Resources",
                column: "ApiVersionId",
                principalTable: "ApiVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiServers_ApiVersions_ApiVersionId",
                table: "ApiServers");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_ApiVersions_ApiVersionId",
                table: "Resources");

            migrationBuilder.DropTable(
                name: "ApiVersions");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ApiVersionId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Path_ApiVersionId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_ApiServers_ApiVersionId",
                table: "ApiServers");

            migrationBuilder.DropIndex(
                name: "IX_ApiServers_Name",
                table: "ApiServers");

            migrationBuilder.DropColumn(
                name: "ApiVersionId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ApiVersionId",
                table: "ApiServers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ApiServers");

            migrationBuilder.AddColumn<string>(
                name: "ApiVersion",
                table: "ApiServers",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path",
                table: "Resources",
                column: "Path",
                unique: true);
        }
    }
}
