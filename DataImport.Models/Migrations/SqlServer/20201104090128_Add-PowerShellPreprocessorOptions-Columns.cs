// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddPowerShellPreprocessorOptionsColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableCmdlets",
                table: "Configurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportPSModules",
                table: "Configurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableCmdlets",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "ImportPSModules",
                table: "Configurations");
        }
    }
}
