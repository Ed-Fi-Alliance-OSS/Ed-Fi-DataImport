// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class MigrateAgentsToScriptProcessors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileGeneratorScriptId",
                table: "Agents",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowProcessorScriptId",
                table: "Agents",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_FileGeneratorScriptId",
                table: "Agents",
                column: "FileGeneratorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_RowProcessorScriptId",
                table: "Agents",
                column: "RowProcessorScriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_Scripts_FileGeneratorScriptId",
                table: "Agents",
                column: "FileGeneratorScriptId",
                principalTable: "Scripts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_Scripts_RowProcessorScriptId",
                table: "Agents",
                column: "RowProcessorScriptId",
                principalTable: "Scripts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
BEGIN TRAN

INSERT INTO dbo.Scripts
SELECT 
	a.RowProcessor AS Name, 'CustomRowProcessor' AS ScriptType, NULL AS ScriptContent, 0 AS RequireOdsApiAccess
FROM 
	dbo.Agents a
WHERE 
	a.RowProcessor IS NOT NULL
UNION
SELECT 
	a.FileGenerator AS Name, 'CustomFileGenerator' AS ScriptType, NULL AS ScriptContent, 0 AS RequireOdsApiAccess
FROM 
	dbo.Agents a
WHERE 
	a.FileGenerator IS NOT NULL;

UPDATE a
SET 
	a.FileGeneratorScriptId = f.Id, a.RowProcessorScriptId = r.Id
FROM 
	dbo.Agents a
LEFT JOIN dbo.Scripts r ON a.RowProcessor = r.Name AND r.ScriptType = 'CustomRowProcessor'
LEFT JOIN dbo.Scripts f ON a.FileGenerator = f.Name AND f.ScriptType = 'CustomFileGenerator'
WHERE 
	r.id IS NOT NULL OR f.Id IS NOT NULL

COMMIT;
");

            migrationBuilder.DropColumn(
                name: "FileGenerator",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "RowProcessor",
                table: "Agents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileGenerator",
                table: "Agents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowProcessor",
                table: "Agents",
                nullable: true);

            migrationBuilder.Sql(@"
BEGIN TRAN

UPDATE a 
SET 
	a.RowProcessor = r.Name, a.FileGenerator = f.Name, a.FileGeneratorScriptId = NULL, a.RowProcessorScriptId = NULL
FROM 
	dbo.Agents a
LEFT JOIN 
	dbo.Scripts f ON a.FileGeneratorScriptId = f.Id
LEFT JOIN 
	dbo.Scripts r ON a.RowProcessorScriptId = r.Id
WHERE 
	a.FileGeneratorScriptId IS NOT NULL OR a.RowProcessorScriptId IS NOT NULL;

DELETE s
FROM dbo.Scripts s
WHERE 
	(s.ScriptType = 'CustomRowProcessor' AND EXISTS (SELECT 1 FROM dbo.Agents a WHERE a.RowProcessor = s.Name))
OR
	(s.ScriptType = 'CustomFileGenerator' AND EXISTS (SELECT 1 FROM dbo.Agents a WHERE a.FileGenerator = s.Name));

COMMIT
"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Agents_Scripts_FileGeneratorScriptId",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_Agents_Scripts_RowProcessorScriptId",
                table: "Agents");

            migrationBuilder.DropIndex(
                name: "IX_Agents_FileGeneratorScriptId",
                table: "Agents");

            migrationBuilder.DropIndex(
                name: "IX_Agents_RowProcessorScriptId",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "FileGeneratorScriptId",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "RowProcessorScriptId",
                table: "Agents");
        }
    }
}
