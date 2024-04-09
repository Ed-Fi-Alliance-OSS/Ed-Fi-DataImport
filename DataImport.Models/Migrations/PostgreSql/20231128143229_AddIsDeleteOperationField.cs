using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataImport.Models.Migrations.PostgreSql
{
    public partial class AddIsDeleteOperationField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleteOperation",
                table: "DataMaps",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleteOperation",
                table: "DataMaps");
        }
    }
}
