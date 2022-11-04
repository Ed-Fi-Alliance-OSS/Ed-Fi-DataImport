// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddFileProcessorScriptToDataMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileProcessorScriptId",
                table: "DataMaps",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataMaps_FileProcessorScriptId",
                table: "DataMaps",
                column: "FileProcessorScriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataMaps_Scripts_FileProcessorScriptId",
                table: "DataMaps",
                column: "FileProcessorScriptId",
                principalTable: "Scripts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataMaps_Scripts_FileProcessorScriptId",
                table: "DataMaps");

            migrationBuilder.DropIndex(
                name: "IX_DataMaps_FileProcessorScriptId",
                table: "DataMaps");

            migrationBuilder.DropColumn(
                name: "FileProcessorScriptId",
                table: "DataMaps");
        }
    }
}
