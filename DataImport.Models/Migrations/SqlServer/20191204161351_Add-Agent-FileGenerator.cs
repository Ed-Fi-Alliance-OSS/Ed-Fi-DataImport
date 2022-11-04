// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddAgentFileGenerator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Preprocessor",
                table: "Agents",
                newName: "RowProcessor");

            migrationBuilder.AddColumn<string>(
                name: "FileGenerator",
                table: "Agents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileGenerator",
                table: "Agents");

            migrationBuilder.RenameColumn(
                name: "RowProcessor",
                table: "Agents",
                newName: "Preprocessor");
        }
    }
}
