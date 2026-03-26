# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ed-Fi DataImport is a .NET solution for loading CSV data into the Ed-Fi ODS/API. It has two main runtime components:

- **DataImport.Web** — ASP.NET Core admin UI for configuration and job monitoring
- **DataImport.Server.TransformLoad** — Console app that processes and loads data

## Build Commands

All builds use `build.ps1` (PowerShell):

```powershell
.\build.ps1 build                                  # Clean, restore, compile
.\build.ps1 unittest                               # Unit tests (no DB required)
.\build.ps1 integrationtest                        # Integration tests (DB required)
.\build.ps1 run -LaunchProfile mssql-shared -Trust # Run web app with SQL Server
.\build.ps1 run -LaunchProfile pg-shared -Trust    # Run web app with PostgreSQL
```

Individual test project (faster feedback):

```bash
dotnet test DataImport.Common.Tests --filter "FullyQualifiedName~.UnitTests"
dotnet test DataImport.Web.Tests
```

## E2E Tests

Do not run the E2E tests as they may be broken.

## Architecture

### CQRS Pattern

All features use MediatR — look for `IRequest<T>` handlers. Commands/queries live in feature folders within each project.

### Database

EF Core with dual-database support. Two DB contexts: `SqlDataImportDbContext` and `PostgreSqlDataImportDbContext`. Migrations are in `DataImport.Models/Migrations/[SqlServer|PostgreSql]/`.

### Adding DB Migrations (via Package Manager Console, targeting DataImport.Models)

```none
Add-Migration <Name> -Context SqlDataImportDbContext -OutputDir ./Migrations/SqlServer -Args "<ConnectionString> SqlServer"
Add-Migration <Name> -Context PostgreSqlDataImportDbContext -OutputDir ./Migrations/PostgreSql -Args "<ConnectionString> PostgreSql"
```

### Key Libraries

- **MediatR** — CQRS command/query dispatch
- **AutoMapper** — entity/DTO mapping
- **CsvHelper** — CSV parsing
- **FluentFTP / SSH.NET** — file ingestion via FTPS/SFTP
- **Serilog** — structured logging (configured via `logging.json`, `logging_Sql.json`, `logging_PgSql.json`)

### Web App Areas

The MVC app is organized into Areas: Activity, Agent, ApiServers, BootstrapData, DataMaps, Log, Preprocessor, Share, and others.

### SSL/TLS for Local Dev

Set `IgnoresCertificateErrors: true` in appsettings for local dev with self-signed certs (both Web and TransformLoad have this flag).

## Code Style

- C# uses Allman brace style, 4-space indent, max 110 chars per line (enforced via `.editorconfig`)
- `TreatWarningsAsErrors` is enabled — fix all warnings; never add <NoWarn> to suppress them.
- TypeScript E2E tests use ESLint + Prettier (run `npm run lint` to check)

## Testing Notes

- Unit test projects are suffixed `*.UnitTests` and require no database
- Integration test projects are suffixed `*.Tests` and require a live database connection via `ConnectionStrings__defaultConnection` environment variable
- Integration tests auto-deploy/reset the DB schema on run
- `PythonExecutableLocation` env var may be needed for preprocessor tests (default: `/usr/bin/python`)
