﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataImport.Models.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class Datamap_Add_SelectedEdOrgIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedIngestionLogEdOrgIdColumn",
                table: "DataMaps",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedIngestionLogEdOrgIdColumn",
                table: "DataMaps");
        }
    }
}