// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class MoveProcessingOrderToBootstrapDataAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessingOrder",
                table: "BootstrapDataAgents",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE bda 
SET 
	bda.ProcessingOrder = bd.ProcessingOrder
FROM 
	dbo.BootstrapDataAgents bda
INNER JOIN 
	dbo.BootstrapDatas bd ON bda.BootstrapDataId = bd.Id;");

            migrationBuilder.DropColumn(
                name: "ProcessingOrder",
                table: "BootstrapDatas");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingOrder",
                table: "BootstrapDataAgents");

            migrationBuilder.AddColumn<int>(
                name: "ProcessingOrder",
                table: "BootstrapDatas",
                nullable: false,
                defaultValue: 0);
        }
    }
}
