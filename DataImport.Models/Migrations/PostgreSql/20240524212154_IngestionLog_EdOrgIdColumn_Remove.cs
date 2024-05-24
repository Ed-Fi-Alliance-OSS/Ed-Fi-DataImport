using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataImport.Models.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class IngestionLog_EdOrgIdColumn_Remove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationOrganizationId",
                table: "IngestionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EducationOrganizationId",
                table: "IngestionLogs",
                type: "uuid",
                nullable: true);
        }
    }
}
