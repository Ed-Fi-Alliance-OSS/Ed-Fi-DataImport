# Remove AutoMapper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove AutoMapper from Ed-Fi DataImport and replace with hand-written static C# mapper classes, eliminating the AutoMapper NuGet dependency.

**Architecture:** Each AutoMapper `MappingProfile` becomes a static `Mapper` class in the same feature folder, exposing `ToViewModel()` / `ToModel()` extension or static methods. Handlers drop `IMapper` injection and call the static methods directly.

**Tech Stack:** .NET 10, ASP.NET Core, MediatR, Entity Framework Core, C#

---

## File Structure

### Files to Create (new static mapper classes)

| New File | Replaces |
|----------|----------|
| `DataImport.Web/Features/Activity/Mapper.cs` | `Activity/MappingProfile.cs` |
| `DataImport.Web/Features/ApiServers/Mapper.cs` | `ApiServers/MappingProfile.cs` |
| `DataImport.Web/Features/BootstrapData/Mapper.cs` | `BootstrapData/MappingProfile.cs` |
| `DataImport.Web/Features/DataMaps/Mapper.cs` | `DataMaps/MappingProfile.cs` |
| `DataImport.Web/Features/Lookup/Mapper.cs` | `Lookup/MappingProfile.cs` |
| `DataImport.Web/Features/Log/Mapper.cs` | `Log/MappingProfile.cs` |
| `DataImport.Web/Features/Preprocessor/Mapper.cs` | `Preprocessor/MappingProfile.cs` |
| `DataImport.Web/Features/Agent/Mapper.cs` | `Agent/MappingProfile.cs` |
| `DataImport.Web/Features/Assessment/Mapper.cs` | `Assessment/MappingProfile.cs` |
| `DataImport.Web/Features/School/Mapper.cs` | `School/MappingProfile.cs` |

### Files to Modify

- All handler `.cs` files that inject `IMapper` (one per feature folder)
- `DataImport.Web/Infrastructure/AutoMapperInstaller.cs` — remove or delete
- `DataImport.Web/DataImport.Web.csproj` — remove AutoMapper package refs
- `DataImport.Web/Startup.cs` — remove AutoMapper DI registration
- `DataImport.Web.Tests/AutoMapperConfigurationTest.cs` — delete entire file
- `DataImport.EdFi/Api/EnrollmentComposite/EnrollmentApi.cs` — replace mapper calls
- `DataImport.EdFi/Api/Resources/AssessmentsApi.cs` — replace mapper calls
- `DataImport.EdFi/DataImport.EdFi.csproj` — remove AutoMapper package ref if present

---

## Task 1: Activity Mapper

**Files:**
- Create: `DataImport.Web/Features/Activity/Mapper.cs`
- Modify: `DataImport.Web/Features/Activity/GetActivity.cs`
- Delete: `DataImport.Web/Features/Activity/MappingProfile.cs`

**Profile logic:**
- `File` → `GetActivity.FileModel`
  - `FileName = src.FileName`
  - `Status = src.Status.ToString()`
  - `Created = src.CreateDate.ToString("M/d/yyyy h:mm tt")`
  - `ApiConnection = src.Agent?.ApiServer?.Name ?? "N/A"`
  - `Rows = src.Rows`
  - `ReasonDetails = src.ReasonDetails`

- [ ] **Step 1: Create `DataImport.Web/Features/Activity/Mapper.cs`**

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Activity;

public static class Mapper
{
    public static GetActivity.FileModel ToFileModel(this File src) =>
        new()
        {
            FileName = src.FileName,
            Status = src.Status.ToString(),
            Created = src.CreateDate.ToString("M/d/yyyy h:mm tt"),
            ApiConnection = src.Agent?.ApiServer?.Name ?? "N/A",
            Rows = src.Rows,
            ReasonDetails = src.ReasonDetails
        };
}
```

- [ ] **Step 2: Update `GetActivity.cs` handler**

Find the `_mapper.Map<FileModel>` call inside the `.Select()` and replace:
```csharp
// Before:
.Select(x => _mapper.Map<FileModel>(x))
// After:
.Select(x => x.ToFileModel())
```

Remove `IMapper _mapper` field and constructor parameter injection.

- [ ] **Step 3: Delete `Activity/MappingProfile.cs`**

- [ ] **Step 4: Build to verify**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
```
Expected: Build succeeds (errors only for remaining AutoMapper usages in other files).

- [ ] **Step 5: Commit**
```bash
git add DataImport.Web/Features/Activity/
git commit -m "refactor: replace AutoMapper with static mapper in Activity feature"
```

---

## Task 2: ApiServers Mapper

**Files:**
- Create: `DataImport.Web/Features/ApiServers/Mapper.cs`
- Modify: `DataImport.Web/Features/ApiServers/ApiServerIndex.cs`
- Delete: `DataImport.Web/Features/ApiServers/MappingProfile.cs`

**Profile logic:**
- `ApiServer` → `ApiServerIndex.ApiServerModel`
  - `Id = src.Id`
  - `Name = src.Name`
  - `Url = src.Url`
  - `ApiVersion = src.ApiVersion?.Version`

- [ ] **Step 1: Create `ApiServers/Mapper.cs`**

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.ApiServers;

public static class Mapper
{
    public static ApiServerIndex.ApiServerModel ToApiServerModel(this ApiServer src) =>
        new()
        {
            Id = src.Id,
            Name = src.Name,
            Url = src.Url,
            ApiVersion = src.ApiVersion?.Version
        };
}
```

- [ ] **Step 2: Update `ApiServerIndex.cs`**

Replace `_mapper.Map<ApiServerModel>(x)` with `x.ToApiServerModel()`. Remove IMapper.

- [ ] **Step 3: Delete `ApiServers/MappingProfile.cs`**

- [ ] **Step 4: Build**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
```

- [ ] **Step 5: Commit**
```bash
git add DataImport.Web/Features/ApiServers/
git commit -m "refactor: replace AutoMapper with static mapper in ApiServers feature"
```

---

## Task 3: BootstrapData Mapper

**Files:**
- Create: `DataImport.Web/Features/BootstrapData/Mapper.cs`
- Modify: `DataImport.Web/Features/BootstrapData/BootstrapDataIndex.cs`
- Delete: `DataImport.Web/Features/BootstrapData/MappingProfile.cs`

**Profile logic:**
- `BootstrapData` → `BootstrapDataIndex.ViewModel`
  - Map all direct properties
  - `ResourceName = src.ToResourceName()` (extension method on the entity)

- [ ] **Step 1: Create `BootstrapData/Mapper.cs`**

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.BootstrapData;

public static class Mapper
{
    public static BootstrapDataIndex.ViewModel ToViewModel(this Models.BootstrapData src) =>
        new()
        {
            Id = src.Id,
            Name = src.Name,
            ResourceName = src.ToResourceName(),
            ApiVersionId = src.ApiVersionId,
            CreateDate = src.CreateDate,
            UpdateDate = src.UpdateDate
        };
}
```

NOTE: Read the actual `BootstrapDataIndex.ViewModel` properties before writing to ensure all fields are covered.

- [ ] **Step 2: Update `BootstrapDataIndex.cs`** — replace mapper call, remove IMapper.

- [ ] **Step 3: Delete `BootstrapData/MappingProfile.cs`**

- [ ] **Step 4: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/BootstrapData/
git commit -m "refactor: replace AutoMapper with static mapper in BootstrapData feature"
```

---

## Task 4: DataMaps Mapper

**Files:**
- Create: `DataImport.Web/Features/DataMaps/Mapper.cs`
- Modify: `DataImport.Web/Features/DataMaps/DataMapIndex.cs`
- Delete: `DataImport.Web/Features/DataMaps/MappingProfile.cs`

**Profile logic (same pattern as BootstrapData):**
- `DataMap` → `DataMapIndex.ViewModel`
  - `ResourceName = src.ToResourceName()`

- [ ] **Step 1: Create `DataMaps/Mapper.cs`** (same pattern as BootstrapData)
- [ ] **Step 2: Update `DataMapIndex.cs`** — replace mapper call, remove IMapper.
- [ ] **Step 3: Delete `DataMaps/MappingProfile.cs`**
- [ ] **Step 4: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/DataMaps/
git commit -m "refactor: replace AutoMapper with static mapper in DataMaps feature"
```

---

## Task 5: Lookup Mapper

**Files:**
- Create: `DataImport.Web/Features/Lookup/Mapper.cs`
- Modify: `DataImport.Web/Features/Lookup/LookupIndex.cs`, `DataImport.Web/Features/Lookup/EditLookup.cs`
- Delete: `DataImport.Web/Features/Lookup/MappingProfile.cs`

**Profile logic:**
- `Lookup` → `LookupIndex.LookupItem` — direct property copy
- `Lookup` → `EditLookup.Command` — direct property copy

- [ ] **Step 1: Read `LookupIndex.LookupItem` and `EditLookup.Command` property lists** from the source files to confirm exact field names.

- [ ] **Step 2: Create `Lookup/Mapper.cs`**

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Lookup;

public static class Mapper
{
    public static LookupIndex.LookupItem ToLookupItem(this Models.Lookup src) =>
        new()
        {
            Id = src.Id,
            SourceTable = src.SourceTable,
            Key = src.Key,
            Value = src.Value
        };

    public static EditLookup.Command ToEditCommand(this Models.Lookup src) =>
        new()
        {
            Id = src.Id,
            SourceTable = src.SourceTable,
            Key = src.Key,
            Value = src.Value
        };
}
```

- [ ] **Step 3: Update `LookupIndex.cs` and `EditLookup.cs`** — replace mapper calls, remove IMapper.
- [ ] **Step 4: Delete `Lookup/MappingProfile.cs`**
- [ ] **Step 5: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/Lookup/
git commit -m "refactor: replace AutoMapper with static mapper in Lookup feature"
```

---

## Task 6: Log Mapper

**Files:**
- Create: `DataImport.Web/Features/Log/Mapper.cs`
- Modify: `DataImport.Web/Features/Log/FilesLog.cs`, `DataImport.Web/Features/Log/IngestionLog.cs`, `DataImport.Web/Features/Log/ApplicationLog.cs`
- Delete: `DataImport.Web/Features/Log/MappingProfile.cs`

**Profile logic:**
- `File` → `LogViewModel.File`
- `IngestionLog` → `LogViewModel.Ingestion`
- `ApplicationLog` → `LogViewModel.ApplicationLog`

- [ ] **Step 1: Read `LogViewModel.cs`** to confirm all destination property names.

- [ ] **Step 2: Create `Log/Mapper.cs`** with three mapping methods:

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Log;

public static class Mapper
{
    public static LogViewModel.File ToLogFile(this Models.File src) => new() { /* fields */ };
    public static LogViewModel.Ingestion ToLogIngestion(this Models.IngestionLog src) => new() { /* fields */ };
    public static LogViewModel.ApplicationLog ToLogApplicationLog(this Models.ApplicationLog src) => new() { /* fields */ };
}
```

Fill in actual field assignments by reading the MappingProfile before deleting it.

- [ ] **Step 3: Update handler files** — replace mapper calls, remove IMapper from each.
- [ ] **Step 4: Delete `Log/MappingProfile.cs`**
- [ ] **Step 5: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/Log/
git commit -m "refactor: replace AutoMapper with static mapper in Log feature"
```

---

## Task 7: Preprocessor Mapper

**Files:**
- Create: `DataImport.Web/Features/Preprocessor/Mapper.cs`
- Modify: `DataImport.Web/Features/Preprocessor/AddPreprocessor.cs`, `DataImport.Web/Features/Preprocessor/EditPreprocessor.cs`, `DataImport.Web/Features/Preprocessor/PreprocessorIndex.cs`
- Delete: `DataImport.Web/Features/Preprocessor/MappingProfile.cs`

**Profile logic (bidirectional):**
- `Script` → `AddEditPreprocessorViewModel` (for Edit)
- `AddEditPreprocessorViewModel` → `Script` (for Add/Edit save)
- `Script` → `PreprocessorIndex.PreprocessorIndexModel`

- [ ] **Step 1: Read `AddEditPreprocessorViewModel` and `PreprocessorIndex.PreprocessorIndexModel`** property lists.

- [ ] **Step 2: Create `Preprocessor/Mapper.cs`** with three methods:

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Preprocessor;

public static class Mapper
{
    public static AddEditPreprocessorViewModel ToViewModel(this Script src) => new() { /* fields */ };
    public static void ApplyToScript(this AddEditPreprocessorViewModel src, Script dest) { /* fields */ }
    public static PreprocessorIndex.PreprocessorIndexModel ToIndexModel(this Script src) => new() { /* fields */ };
}
```

Note: For the `AddEditPreprocessorViewModel → Script` direction (used in save handlers), use `ApplyToScript` to update an existing tracked EF entity rather than creating a new object. This avoids EF tracking issues.

- [ ] **Step 3: Update handler files** — replace mapper calls, remove IMapper from each.
- [ ] **Step 4: Delete `Preprocessor/MappingProfile.cs`**
- [ ] **Step 5: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/Preprocessor/
git commit -m "refactor: replace AutoMapper with static mapper in Preprocessor feature"
```

---

## Task 8: Agent Mapper

**Files:**
- Create: `DataImport.Web/Features/Agent/Mapper.cs`
- Modify: `DataImport.Web/Features/Agent/EditAgent.cs` (and related handlers)
- Delete: `DataImport.Web/Features/Agent/MappingProfile.cs`

**Profile logic (most complex):**
- `Agent` → `AddEditAgentViewModel` — many ignored members (filled by other queries)
- `DataMapAgent` → `MappedAgent`
- `BootstrapDataAgent` → `AgentBootstrapData`
- `AgentSchedule` → `Schedule`

- [ ] **Step 1: Read the full `Agent/MappingProfile.cs`** to capture all `.ForMember(dest => dest.X, opt => opt.Ignore())` lines — these become fields that stay at default (0/null/"") in the static mapper.

- [ ] **Step 2: Read `AddEditAgentViewModel`, `MappedAgent`, `AgentBootstrapData`, `Schedule`** property definitions.

- [ ] **Step 3: Create `Agent/Mapper.cs`**

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Agent;

public static class Mapper
{
    public static AddEditAgentViewModel ToViewModel(this Models.Agent src) =>
        new()
        {
            Id = src.Id,
            Name = src.Name,
            AgentTypeCode = src.AgentTypeCode,
            Url = src.Url,
            Username = src.Username,
            // Password intentionally omitted (ignored in profile)
            Directory = src.Directory,
            FilePattern = src.FilePattern,
            Enabled = src.Enabled,
            ApiServerId = src.ApiServerId,
            RunOrder = src.RunOrder,
            // All .Ignore()d members stay default — they are populated
            // separately in the handler after calling this method
        };

    public static MappedAgent ToMappedAgent(this DataMapAgent src) =>
        new()
        {
            DataMapId = src.DataMapId,
            ProcessingOrder = src.ProcessingOrder
        };

    public static AgentBootstrapData ToAgentBootstrapData(this BootstrapDataAgent src) =>
        new()
        {
            BootstrapDataId = src.BootstrapDataId,
            ProcessingOrder = src.ProcessingOrder
        };

    public static Schedule ToSchedule(this AgentSchedule src) =>
        new()
        {
            Id = src.Id,
            Day = src.Day,
            Hour = src.Hour,
            Minute = src.Minute
        };
}
```

- [ ] **Step 4: Update `EditAgent.cs`** — replace `_mapper.Map<AddEditAgentViewModel>(agent)` with `agent.ToViewModel()`, and replace nested `.Select()` mapper calls. Remove IMapper.

- [ ] **Step 5: Delete `Agent/MappingProfile.cs`**
- [ ] **Step 6: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
git add DataImport.Web/Features/Agent/
git commit -m "refactor: replace AutoMapper with static mapper in Agent feature"
```

---

## Task 9: Assessment Mapper

**Files:**
- Create: `DataImport.Web/Features/Assessment/Mapper.cs`
- Modify: `DataImport.Web/Features/Assessment/AssessmentIndex.cs`, `DataImport.Web/Features/Assessment/AssessmentDetails.cs`
- Modify: `DataImport.EdFi/Api/Resources/AssessmentsApi.cs`
- Delete: `DataImport.Web/Features/Assessment/MappingProfile.cs`

**Profile logic (complex — V2.5 API conversion):**
The `Assessment/MappingProfile.cs` has two distinct mapping concerns:
1. Converting from the Ed-Fi V2.5 `Assessment` API model to the current internal `Assessment` type
2. Mapping descriptors: converting `{namespace}#{shortname}` format to just `{shortname}`

The V2 path in `AssessmentsApi.cs` calls `_mapper.Map<Assessment>(v2Assessment)` where it converts field-by-field between two different model shapes.

- [ ] **Step 1: Read `Assessment/MappingProfile.cs` in full** to capture all field mappings and the descriptor name conversion logic.

- [ ] **Step 2: Read the V2 and current `Assessment` model classes** to understand source/destination shapes.

- [ ] **Step 3: Create `Assessment/Mapper.cs`** with methods for:
  - V2.5 Assessment → current Assessment
  - Assessment → AssessmentIndex.ViewModel
  - Assessment → AssessmentDetails.ViewModel (if applicable)

```csharp
// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Features.Assessment;

public static class Mapper
{
    private static string ToShortName(string descriptorValue) =>
        descriptorValue?.Contains('#') == true
            ? descriptorValue.Split('#')[1]
            : descriptorValue;

    public static DataImport.EdFi.Models.Resources.Assessment ToCurrentAssessment(
        this DataImport.EdFi.Models.Resources.v2.Assessment src) =>
        new()
        {
            // Field-by-field from the profile, using ToShortName() for descriptors
        };
}
```

- [ ] **Step 4: Update `AssessmentsApi.cs`** — replace `_mapper.Map<Assessment>(v2Assessment)` with `v2Assessment.ToCurrentAssessment()`. Remove IMapper from that class.

Note: The non-V2 path in `AssessmentsApi.cs` maps `Assessment → Assessment` (identity) — simply remove that mapper call entirely; the object is already the right type.

- [ ] **Step 5: Update `AssessmentIndex.cs` and `AssessmentDetails.cs`** — replace mapper calls, remove IMapper.

- [ ] **Step 6: Delete `Assessment/MappingProfile.cs`**
- [ ] **Step 7: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
dotnet build DataImport.EdFi/DataImport.EdFi.csproj
git add DataImport.Web/Features/Assessment/ DataImport.EdFi/
git commit -m "refactor: replace AutoMapper with static mapper in Assessment feature"
```

---

## Task 10: School Mapper

**Files:**
- Create: `DataImport.Web/Features/School/Mapper.cs` (or in `DataImport.EdFi`)
- Modify: `DataImport.EdFi/Api/EnrollmentComposite/EnrollmentApi.cs`
- Delete: `DataImport.Web/Features/School/MappingProfile.cs`

**Profile logic (V2.5 enrollment model conversion):**
- V2.5 `School` → current `School`
- V2.5 `Section` → current `Section`
- Similar descriptor name formatting as Assessment

- [ ] **Step 1: Read `School/MappingProfile.cs` in full** and the V2 School/Section model types.

- [ ] **Step 2: Create mapper** — place in `DataImport.EdFi` project since that's where `EnrollmentApi.cs` lives, alongside the EdFi model types.

- [ ] **Step 3: Update `EnrollmentApi.cs`** — replace mapper calls, remove IMapper.

- [ ] **Step 4: Delete `School/MappingProfile.cs`**
- [ ] **Step 5: Build and commit**
```bash
dotnet build DataImport.Web/DataImport.Web.csproj
dotnet build DataImport.EdFi/DataImport.EdFi.csproj
git add DataImport.Web/Features/School/ DataImport.EdFi/
git commit -m "refactor: replace AutoMapper with static mapper in School/Enrollment feature"
```

---

## Task 11: Remove AutoMapper Infrastructure

**Files:**
- Delete: `DataImport.Web/Infrastructure/AutoMapperInstaller.cs`
- Modify: `DataImport.Web/Startup.cs` — remove `services.AddAutoMapper(...)` call
- Modify: `DataImport.Web/DataImport.Web.csproj` — remove AutoMapper package references
- Modify: `DataImport.EdFi/DataImport.EdFi.csproj` — remove AutoMapper package references (if any)
- Delete: `DataImport.Web.Tests/AutoMapperConfigurationTest.cs`

- [ ] **Step 1: Confirm no remaining IMapper usages**
```bash
grep -r "IMapper\|AutoMapper\|MapperConfiguration\|MappingProfile\|\.Map<" \
  DataImport.Web DataImport.EdFi --include="*.cs" -l
```
Expected: Zero results (or only test helpers that will be deleted).

- [ ] **Step 2: Delete `AutoMapperConfigurationTest.cs`**

This test existed to validate AutoMapper profiles were configured without errors. It is no longer needed because static mappers cannot have configuration errors — they either compile or they don't.

Why removal is safe: The test verified runtime configuration of AutoMapper profile mappings. Since we've replaced all profiles with compile-time-checked static methods, there is no runtime configuration to validate.

- [ ] **Step 3: Remove AutoMapper DI registration from `Startup.cs`**

Find and remove:
```csharp
services.AddAutoMapper(typeof(Startup).Assembly);
// or similar
```

- [ ] **Step 4: Delete `AutoMapperInstaller.cs`** (if it exists as a separate installer)

- [ ] **Step 5: Remove AutoMapper packages from `.csproj` files**

In `DataImport.Web.csproj`, remove:
```xml
<PackageReference Include="AutoMapper" Version="*" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="*" />
```

In `DataImport.EdFi.csproj`, remove AutoMapper references if present.

- [ ] **Step 6: Restore and build entire solution**
```bash
dotnet restore
dotnet build
```
Expected: Zero errors. If AutoMapper-related `NU1xxx` warnings appear about unused packages, they confirm successful removal.

- [ ] **Step 7: Run unit tests**
```bash
dotnet test DataImport.Web.Tests
dotnet test DataImport.EdFi.UnitTests
```
Expected: All tests pass (231 + 10 from baseline).

- [ ] **Step 8: Commit**
```bash
git add -A
git commit -m "refactor: remove AutoMapper package and infrastructure

AutoMapper profiles have been replaced with static C# mapper classes
in each feature folder. The static mappers are checked at compile time,
making the AutoMapperConfigurationTest redundant.

Removed:
- AutoMapper and AutoMapper.Extensions.Microsoft.DependencyInjection packages
- AutoMapperInstaller and AddAutoMapper DI registration
- All MappingProfile.cs files
- AutoMapperConfigurationTest.cs (compile-time safety replaces runtime config validation)"
```

---

## Task 12: Open Pull Request

- [ ] **Step 1: Push the branch**
```bash
git push -u origin remove-automapper
```

- [ ] **Step 2: Open PR targeting `net10-upgrade`**
```bash
gh pr create \
  --base net10-upgrade \
  --title "Remove AutoMapper: replace with static C# mappers" \
  --body "$(cat <<'EOF'
## Summary

- Replaces all AutoMapper `MappingProfile` classes with static `Mapper` helper classes per feature folder
- Removes `IMapper` injection from all MediatR handlers
- Removes `AutoMapper` and `AutoMapper.Extensions.Microsoft.DependencyInjection` NuGet packages
- Deletes `AutoMapperConfigurationTest.cs` (compile-time safety replaces runtime config validation)

## Motivation

AutoMapper adds runtime configuration complexity and obscures mapping logic. Static mappers are explicit, compile-time-checked, and easier to read and debug.

## Test plan

- [ ] `dotnet build` — zero errors
- [ ] `dotnet test DataImport.Web.Tests` — all tests pass
- [ ] `dotnet test DataImport.EdFi.UnitTests` — all tests pass
- [ ] Manually verify Activity, Log, and Agent pages render correctly

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```
