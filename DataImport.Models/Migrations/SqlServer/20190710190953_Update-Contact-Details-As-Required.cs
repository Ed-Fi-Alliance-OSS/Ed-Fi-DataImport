// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class UpdateContactDetailsAsRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Manual removal of configuration in case now-required fields were previously null.
            migrationBuilder.Sql("DELETE FROM Configurations");

            //Generated Migration
            migrationBuilder.AlterColumn<string>(
                name: "ContactOrganization",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactName",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContactOrganization",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ContactName",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
