# .NET 10 Upgrade Design

**Date:** 2026-03-25
**Branch:** `net10-upgrade`
**Approach:** Big Bang — single PR upgrading all projects at once

## Goal

Upgrade Ed-Fi DataImport from .NET 8 to .NET 10. The scope is strictly mechanical: get all projects compiling and all unit and integration tests passing on .NET 10. No language modernization, no new features.

E2E tests (Playwright/Cucumber) are excluded from the CI gate and will be verified manually after merge.

## Scope

- All 11 C# projects in `DataImport.sln`
- `global.json` SDK pin
- All NuGet package references
- `Dockerfile` and `dev.Dockerfile` base image digests
- GitHub Actions workflows where .NET version is explicitly pinned

## Out of Scope

- C# 13 language feature adoption
- Refactoring or code modernization
- E2E test execution (manual post-merge step)

---

## Phase 1: Package Compatibility Audit

Before making any changes, run a package audit to identify every package needing a version bump and flag any with no .NET 10 compatible release:

```bash
dotnet list package --outdated
```

This gates all subsequent phases. Known risks to resolve during this phase are documented in Phase 3 below.

---

## Phase 2: Framework & SDK Bump

**`global.json`** — Update the SDK version pin to .NET 10:
```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMajor"
  }
}
```

**All `*.csproj` files (11 projects)** — Change target framework:
```
net8.0 → net10.0
```

Projects affected:
- `DataImport.Common`
- `DataImport.Common.Tests`
- `DataImport.EdFi`
- `DataImport.EdFi.UnitTests`
- `DataImport.Models`
- `DataImport.Models.Tests`
- `DataImport.Server.TransformLoad`
- `DataImport.Server.TransformLoad.Tests`
- `DataImport.TestHelpers`
- `DataImport.Web`
- `DataImport.Web.Tests`

---

## Phase 3: NuGet Package Updates

### Straightforward Version Bumps

Update to `10.0.x` releases:

| Package | Current | Action |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.8 | → 10.0.x |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.8 | → 10.0.x |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.8 | → 10.0.x |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.8 | → 10.0.x |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.8 | → 10.0.x |
| `Microsoft.AspNetCore.Authentication.OpenIdConnect` | 8.0.8 | → 10.0.x |
| `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | 8.0.8 | → 10.0.x |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.8 | → 10.0.x (tracks EF Core versions) |
| `Microsoft.Extensions.Configuration` | 8.0.0 | → 10.0.x |
| `Microsoft.Extensions.Configuration.*` | 8.0.x | → 10.0.x |
| `Microsoft.Extensions.DependencyInjection` | 8.0.0 | → 10.0.x |
| `Microsoft.Extensions.Logging.Console` | 8.0.0 | → 10.0.x |
| `Serilog.AspNetCore` | 8.0.2 | → latest compatible |
| `System.Diagnostics.DiagnosticSource` | 8.0.1 | → 10.0.x |

### Packages Requiring Special Handling

**`Libuv` 1.10.0** — Present in 7 of 11 projects. This was the Kestrel transport layer in .NET Core 1.x/2.x and has been unnecessary since .NET Core 3.0. **Remove from all projects.**

**`Rijndael256` 3.2.0** — Used in `DataImport.Common` for credential encryption. **Replace** with `System.Security.Cryptography` AES (built into the BCL — no NuGet package required). Rijndael/AES are the same cipher; the migration is straightforward. Update all call sites in `DataImport.Common`.

**`Microsoft.Data.Services.Client` 5.8.5** — Legacy WCF OData client in `DataImport.Common`. During the audit phase, verify whether it is actually used. If unused, remove it. If used, assess whether a .NET 10 compatible version exists or whether an alternative is needed.

**`LigerShark.WebOptimizer.Core` 3.0.405** — Asset pipeline middleware in `DataImport.Web`. Verify .NET 10 support during audit; update to latest if available.

**`Microsoft.PowerShell.SDK` 7.4.5** — Used in 3 test projects. PowerShell 7.4 targets `net8.0`; verify whether 7.5+ (targeting `net10.0`) is required or if the existing version is compatible via `rollForward`.

**`Microsoft.CodeAnalysis` 4.8.0** — Bump to latest stable.

### Packages Expected to Be Compatible Without Version Changes

- `CsvHelper` 33.0.1
- `RestSharp` 112.1.0
- `FluentFTP` 51.1.0
- `SSH.NET` 2024.1.0
- `MediatR` 12.4.1
- `AutoMapper` 13.0.1
- `FluentValidation.AspNetCore` 11.3.0
- `Serilog` 4.0.2 and all non-ASP.NET Serilog sinks
- `FakeItEasy` 8.3.0
- `NUnit` 4.2.2 and related packages
- `Shouldly` 4.2.1
- `Humanizer` 2.14.1
- `Newtonsoft.Json` 13.0.3
- Azure SDK packages (`Azure.Core`, `Azure.Storage.*`, `Azure.Security.*`)

---

## Phase 4: Build Fix-Up

After the framework and package changes, run `dotnet build` and resolve any compile errors. Expected sources of breakage:

- **`Rijndael256` replacement** — Update encryption call sites in `DataImport.Common` to use `System.Security.Cryptography.Aes`.
- **Breaking API changes in .NET 10** — Consult the [.NET 10 breaking changes documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0) for any APIs used in the codebase.
- **EF Core 10 breaking changes** — Consult the [EF Core 10 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes) for migration code or query patterns affected.

Run tests locally to confirm green before opening the PR:
```powershell
.\build.ps1 unittest
.\build.ps1 integrationtest
```

---

## Phase 5: Docker & CI Updates

### Dockerfiles

Both `Dockerfile` and `dev.Dockerfile` use SHA-pinned base images. Update to .NET 10 equivalents:

- Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0` (with pinned SHA digest)
- SDK: `mcr.microsoft.com/dotnet/sdk:10.0` (with pinned SHA digest)

Obtain current SHA digests from the [Microsoft Artifact Registry](https://mcr.microsoft.com/en-us/artifact/mar/dotnet/aspnet/tags) at time of implementation.

### GitHub Actions Workflows

| File | Change |
|---|---|
| `on-prerelease.yml` | `dotnet-version: 8.0.x` → `10.0.x` |
| `on-pullrequest-or-push.yml` | No change (relies on `global.json`) |
| `on-merge-or-tag.yml` | No change (relies on `global.json`) |
| `on-release.yml` | No change (relies on `global.json`) |

The MSSQL service container (`mcr.microsoft.com/mssql/server:2019-latest`) is not .NET-version-dependent and does not need updating.

---

## Definition of Done

- [ ] `dotnet build` succeeds with zero errors and zero warnings on .NET 10
- [ ] `.\build.ps1 unittest` passes
- [ ] `.\build.ps1 integrationtest` passes
- [ ] CI pipeline (Build and Test job) is green on the `net10-upgrade` branch PR
- [ ] Docker images reference .NET 10 base images
- [ ] `on-prerelease.yml` references `dotnet-version: 10.0.x`
- [ ] E2E tests verified manually after merge (not a PR gate)
