// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddTemplateSharingApiConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateSharingApiKey",
                table: "Configurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateSharingApiSecret",
                table: "Configurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateSharingApiUrl",
                table: "Configurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateSharingApiKey",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "TemplateSharingApiSecret",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "TemplateSharingApiUrl",
                table: "Configurations");
        }
    }
}
