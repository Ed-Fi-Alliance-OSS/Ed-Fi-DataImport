// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class UpdateConfigurationWithContactDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InstanceOrganizationUrl",
                table: "Configurations",
                newName: "ContactOrganization");

            migrationBuilder.RenameColumn(
                name: "InstanceOrganizationLogo",
                table: "Configurations",
                newName: "ContactName");

            migrationBuilder.RenameColumn(
                name: "InstanceEduUseText",
                table: "Configurations",
                newName: "ContactEmail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContactOrganization",
                table: "Configurations",
                newName: "InstanceOrganizationUrl");

            migrationBuilder.RenameColumn(
                name: "ContactName",
                table: "Configurations",
                newName: "InstanceOrganizationLogo");

            migrationBuilder.RenameColumn(
                name: "ContactEmail",
                table: "Configurations",
                newName: "InstanceEduUseText");
        }
    }
}
