using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataImport.Models.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Agent_FilesAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionFileCode",
                table: "Agents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionFileCode",
                table: "Agents");
        }
    }
}
