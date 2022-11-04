// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddResourcePathApiSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Manual removal of existing Resources records, as this migration's
            //new Resources columns invalidate the Resources cache.
            migrationBuilder.Sql("DELETE FROM Resources");

            //Manual removal of existing Data Maps and Bootstraps, as a format
            //change inside Metadata columns invalidates existing records prior
            //to initial public release.
            migrationBuilder.Sql("DELETE FROM DataMapAgents");
            migrationBuilder.Sql("DELETE FROM BootstrapDatas");
            migrationBuilder.Sql("DELETE FROM DataMaps");

            //Generated Migration
            migrationBuilder.AddColumn<int>(
                name: "ApiSection",
                table: "Resources",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Resources",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiSection",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Resources");
        }
    }
}
