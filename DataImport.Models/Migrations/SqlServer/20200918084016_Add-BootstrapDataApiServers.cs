// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class AddBootstrapDataApiServers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedDate",
                table: "BootstrapDatas");

            migrationBuilder.CreateTable(
                name: "BootstrapDataApiServers",
                columns: table => new
                {
                    BootstrapDataId = table.Column<int>(nullable: false),
                    ApiServerId = table.Column<int>(nullable: false),
                    ProcessedDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDataApiServers", x => new { x.BootstrapDataId, x.ApiServerId });
                    table.ForeignKey(
                        name: "FK_BootstrapDataApiServers_ApiServers_ApiServerId",
                        column: x => x.ApiServerId,
                        principalTable: "ApiServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BootstrapDataApiServers_BootstrapDatas_BootstrapDataId",
                        column: x => x.BootstrapDataId,
                        principalTable: "BootstrapDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataApiServers_ApiServerId",
                table: "BootstrapDataApiServers",
                column: "ApiServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BootstrapDataApiServers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessedDate",
                table: "BootstrapDatas",
                nullable: true);
        }
    }
}
