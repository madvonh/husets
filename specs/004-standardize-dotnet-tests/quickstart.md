# Quickstart: Standardize .NET Test Tooling

## Purpose

Use this guide to verify the backend test-stack migration to `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`.

## Prerequisites

- .NET SDK `10.0.103` installed
- Repository checked out on branch `004-standardize-dotnet-tests`
- Working directory at repository root

## Verify the migrated backend test stack

```powershell
cd src/backend/tests/RecipeApi.Tests
Get-Content RecipeApi.Tests.csproj
```

Confirm the project references:
- `Microsoft.NET.Test.Sdk`
- `NUnit`
- `NUnit3TestAdapter`
- `NSubstitute`

Confirm it does **not** reference:
- `xunit`
- `xunit.runner.visualstudio`

## Run the backend test project

```powershell
cd src/backend
dotnet test .\tests\RecipeApi.Tests\RecipeApi.Tests.csproj
```

Expected result:
- 73 tests discovered
- 73 tests passed
- 0 failed

## Re-run for stability

```powershell
cd src/backend
dotnet test .\tests\RecipeApi.Tests\RecipeApi.Tests.csproj
```

Expected result:
- same passing totals as the first run
- no framework discovery issues

## Verify CI-equivalent execution

```powershell
cd src/backend
dotnet test .\RecipeCollection.sln --no-restore --verbosity normal
```

Expected result:
- solution test run passes
- backend CI command path remains valid

## Verify current contributor guidance

Inspect these files:
- `README.md`
- `specs/001-recipe-collection/quickstart.md`
- `.specify/memory/constitution.md`

Confirm they reference the approved backend test standard:
- `NUnit`
- `NSubstitute`
- `Microsoft.NET.Test.Sdk`

## Regression protection

The platform-validation tests in `src/backend/tests/RecipeApi.Tests/PlatformValidation/` should fail if a non-approved primary .NET test framework is reintroduced into the active backend test project.
