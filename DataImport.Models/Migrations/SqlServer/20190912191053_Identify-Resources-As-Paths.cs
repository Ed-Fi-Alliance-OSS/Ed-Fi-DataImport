// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class IdentifyResourcesAsPaths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Resources",
                newName: "Path");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                newName: "IX_Resources_Path");

            migrationBuilder.RenameColumn(
                name: "ResourceName",
                table: "DataMaps",
                newName: "ResourcePath");

            migrationBuilder.RenameColumn(
                name: "ResourceName",
                table: "BootstrapDatas",
                newName: "ResourcePath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Resources",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_Path",
                table: "Resources",
                newName: "IX_Resources_Name");

            migrationBuilder.RenameColumn(
                name: "ResourcePath",
                table: "DataMaps",
                newName: "ResourceName");

            migrationBuilder.RenameColumn(
                name: "ResourcePath",
                table: "BootstrapDatas",
                newName: "ResourceName");
        }
    }
}
