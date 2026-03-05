# Quickstart: Validate .NET 10 + C# 12 Migration

## Purpose

Validate that backend runtime/toolchain migration is complete and consistent across project files, local workflow, and CI expectations.

## Prerequisites

- .NET SDK matching repository `global.json` (`.NET 10.0.x` pinned version)
- Node.js/npm for frontend dev workflows (unchanged by this feature)
- Existing local services used by backend tests (if required by test profile)

## Validation Steps

### 1) Confirm SDK pinning

From repo root:

```powershell
Get-Content .\src\backend\global.json
```

Expected:
- file exists at `src/backend/global.json`
- `sdk.version` is a specific `.NET 10.0.x`

### 2) Restore and build backend

```powershell
cd src/backend

dotnet restore RecipeCollection.sln
dotnet build RecipeCollection.sln
```

Expected:
- restore/build complete successfully
- no target-framework mismatch errors

### 3) Run backend tests

```powershell
dotnet test RecipeCollection.sln
```

Expected:
- backend tests pass on `net10.0`

### 4) Verify project baselines

Check backend project files:
- `src/backend/src/RecipeApi/RecipeApi.csproj`
- `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`

Expected:
- `TargetFramework` is `net10.0` in both
- C# 12 baseline enforced (project or central configuration)
- no dual-targeting with legacy frameworks

### 5) Verify VS Code workflow alignment

Check:
- `.vscode/tasks.json`
- `.vscode/launch.json`

Expected:
- paths reflect current repo layout (`src/backend`, `src/frontend`)
- backend debug output path references `net10.0`

### 6) Verify documentation alignment

Check:
- `README.md`
- relevant spec quickstarts

Expected:
- local build/test command paths are current
- platform baseline text references `.NET 10` and C# 12

## Completion Criteria

Migration validation is complete when all of the following are true:
- backend build/test passes under pinned `.NET 10.0.x`
- project files target `net10.0` only
- dependency blockers are zero
- workspace tooling and docs are consistent with migrated baseline
