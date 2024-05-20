// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddIsDeleteByNaturalKeyField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleteByNaturalKey",
                table: "DataMaps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleteByNaturalKey",
                table: "DataMaps");
        }
    }
}
