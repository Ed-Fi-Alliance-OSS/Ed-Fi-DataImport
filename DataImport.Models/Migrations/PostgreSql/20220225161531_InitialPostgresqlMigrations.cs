// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataImport.Models.Migrations.PostgreSql
{
    public partial class InitialPostgresqlMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MachineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Logged = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Logger = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    ServerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ServerAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RemoteAddress = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateSharingApiUrl = table.Column<string>(type: "text", nullable: true),
                    TemplateSharingApiKey = table.Column<string>(type: "text", nullable: true),
                    TemplateSharingApiSecret = table.Column<string>(type: "text", nullable: true),
                    InstanceAllowUserRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EnableProductImprovement = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableCmdlets = table.Column<string>(type: "text", nullable: true),
                    ImportPSModules = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseVersion",
                columns: table => new
                {
                    VersionString = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "IngestionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Operation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AgentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Process = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowNumber = table.Column<string>(type: "text", nullable: true),
                    EndPointUrl = table.Column<string>(type: "text", nullable: true),
                    HttpStatusCode = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    OdsResponse = table.Column<string>(type: "text", nullable: true),
                    ApiVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApiServerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Started = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Completed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceTable = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ScriptType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScriptContent = table.Column<string>(type: "text", nullable: true),
                    RequireOdsApiAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasAttribute = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutablePath = table.Column<string>(type: "text", nullable: true),
                    ExecutableArguments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TokenUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AuthUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ApiVersionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiServers_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ResourcePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApiVersionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BootstrapDatas_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    ApiSection = table.Column<int>(type: "integer", nullable: false),
                    ApiVersionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ResourcePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    Map = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ColumnHeaders = table.Column<string>(type: "text", nullable: true),
                    ApiVersionId = table.Column<int>(type: "integer", nullable: false),
                    FileProcessorScriptId = table.Column<int>(type: "integer", nullable: true),
                    Attribute = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataMaps_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataMaps_Scripts_FileProcessorScriptId",
                        column: x => x.FileProcessorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AgentTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AgentAction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    Directory = table.Column<string>(type: "text", nullable: true),
                    FilePattern = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastExecuted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowProcessorScriptId = table.Column<int>(type: "integer", nullable: true),
                    FileGeneratorScriptId = table.Column<int>(type: "integer", nullable: true),
                    ApiServerId = table.Column<int>(type: "integer", nullable: true),
                    RunOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_ApiServers_ApiServerId",
                        column: x => x.ApiServerId,
                        principalTable: "ApiServers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Scripts_FileGeneratorScriptId",
                        column: x => x.FileGeneratorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Scripts_RowProcessorScriptId",
                        column: x => x.RowProcessorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDataApiServers",
                columns: table => new
                {
                    BootstrapDataId = table.Column<int>(type: "integer", nullable: false),
                    ApiServerId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "AgentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentSchedules_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDataAgents",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    BootstrapDataId = table.Column<int>(type: "integer", nullable: false),
                    ProcessingOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDataAgents", x => new { x.BootstrapDataId, x.AgentId });
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_BootstrapDatas_BootstrapDataId",
                        column: x => x.BootstrapDataId,
                        principalTable: "BootstrapDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataMapAgents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataMapId = table.Column<int>(type: "integer", nullable: false),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    ProcessingOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMapAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_DataMaps_DataMapId",
                        column: x => x.DataMapId,
                        principalTable: "DataMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Rows = table.Column<int>(type: "integer", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ApiServerId",
                table: "Agents",
                column: "ApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_FileGeneratorScriptId",
                table: "Agents",
                column: "FileGeneratorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_RowProcessorScriptId",
                table: "Agents",
                column: "RowProcessorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedules_AgentId",
                table: "AgentSchedules",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_ApiVersionId",
                table: "ApiServers",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_Name",
                table: "ApiServers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiVersions_ApiVersion",
                table: "ApiVersions",
                column: "ApiVersion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Logged",
                table: "ApplicationLogs",
                column: "Logged");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataAgents_AgentId",
                table: "BootstrapDataAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataApiServers_ApiServerId",
                table: "BootstrapDataApiServers",
                column: "ApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDatas_ApiVersionId",
                table: "BootstrapDatas",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_AgentId",
                table: "DataMapAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_DataMapId",
                table: "DataMapAgents",
                column: "DataMapId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMaps_ApiVersionId",
                table: "DataMaps",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMaps_FileProcessorScriptId",
                table: "DataMaps",
                column: "FileProcessorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AgentId",
                table: "Files",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreateDate",
                table: "Files",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Date",
                table: "IngestionLogs",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Result",
                table: "IngestionLogs",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_SourceTable_Key",
                table: "Lookups",
                columns: new[] { "SourceTable", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ApiVersionId",
                table: "Resources",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path_ApiVersionId",
                table: "Resources",
                columns: new[] { "Path", "ApiVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_Name_ScriptType",
                table: "Scripts",
                columns: new[] { "Name", "ScriptType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSchedules");

            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BootstrapDataAgents");

            migrationBuilder.DropTable(
                name: "BootstrapDataApiServers");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DatabaseVersion");

            migrationBuilder.DropTable(
                name: "DataMapAgents");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "IngestionLogs");

            migrationBuilder.DropTable(
                name: "JobStatus");

            migrationBuilder.DropTable(
                name: "Lookups");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BootstrapDatas");

            migrationBuilder.DropTable(
                name: "DataMaps");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ApiServers");

            migrationBuilder.DropTable(
                name: "Scripts");

            migrationBuilder.DropTable(
                name: "ApiVersions");
        }
    }
}
