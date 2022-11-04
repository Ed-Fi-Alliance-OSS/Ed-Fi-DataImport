// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class IndexLookupSourceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GroupSet",
                table: "Lookups",
                newName: "SourceTable");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_SourceTable_Key",
                table: "Lookups",
                columns: new[] { "SourceTable", "Key" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lookups_SourceTable_Key",
                table: "Lookups");

            migrationBuilder.RenameColumn(
                name: "SourceTable",
                table: "Lookups",
                newName: "GroupSet");
        }
    }
}
