# Migration Validation Evidence

**Feature**: Upgrade to .NET 10 with C# 12
**Date**: 2026-03-05

## Pre-Implementation Evidence

### Failing Tests (Red Phase)

_Captured before implementation changes. Tests should fail because projects still target net9.0._

| Test | Expected Failure | Actual Result | Date |
|------|------------------|---------------|------|
| PlatformBaselineTests.ApiProject_TargetFramework_IsNet10 | Fail (net9.0) | FAIL: Expected net10.0, Actual net9.0 | 2026-03-05 |
| PlatformBaselineTests.TestProject_TargetFramework_IsNet10 | Fail (net9.0) | FAIL: Expected net10.0, Actual net9.0 | 2026-03-05 |
| PlatformBaselineTests.Backend_LangVersion_IsCSharp12 | Fail (no LangVersion) | PASS (Directory.Build.props already set) | 2026-03-05 |
| PlatformBaselineTests.GlobalJson_PinsSdk_To10_0_x | Pass (already created) | PASS (global.json 10.0.103) | 2026-03-05 |
| PlatformBaselineTests.ApiProject_NoLegacyFrameworkReference | Fail (net9) | FAIL: Found "net9" in TFM | 2026-03-05 |
| PlatformBaselineTests.TestProject_NoLegacyFrameworkReference | Fail (net9) | FAIL: Found "net9" in TFM | 2026-03-05 |

---

## Post-Implementation Evidence

### US1: Development Workflow Operational

| Validation | Expected | Actual | Date |
|------------|----------|--------|------|
| `dotnet restore RecipeCollection.sln` | Success | Success | 2026-03-05 |
| `dotnet build RecipeCollection.sln` | Success | Success (0 warnings, 0 errors) | 2026-03-05 |
| `dotnet test RecipeCollection.sln` | All pass | 60 passed, 0 failed | 2026-03-05 |
| API project TFM | net10.0 | net10.0 | 2026-03-05 |
| Test project TFM | net10.0 | net10.0 | 2026-03-05 |
| C# language version | 12.0 | 12.0 (Directory.Build.props) | 2026-03-05 |
| global.json SDK | 10.0.103 | 10.0.103 | 2026-03-05 |

### US2: CI and Tooling Parity

| Validation | Expected | Actual | Date |
|------------|----------|--------|------|
| tasks.json backend build path | src/backend/... | ${workspaceFolder}/src/backend/src/RecipeApi/RecipeApi.csproj | 2026-03-05 |
| launch.json output path | net10.0 | net10.0/RecipeApi.dll | 2026-03-05 |
| CI workflow SDK version | matches global.json | 10.0.103 in backend-ci.yml | 2026-03-05 |
| SDK parity script | Pass | Pass (all checks green) | 2026-03-05 |

### US3: Documentation Baseline

| Validation | Expected | Actual | Date |
|------------|----------|--------|------|
| README references .NET 10 | Yes | Yes (.NET 10 (C# 12)) | 2026-03-05 |
| README references C# 12 | Yes | Yes (.NET 10 (C# 12)) | 2026-03-05 |
| Quickstart commands match paths | Yes | Yes (src/backend, src/frontend) | 2026-03-05 |

---

## Final Report

| Phase | Status | Notes |
|-------|--------|-------|
| Phase 1: Setup | ✅ Complete | global.json (10.0.103), Directory.Build.props (C# 12), migration template |
| Phase 2: Foundational | ✅ Complete | Dependency matrix, test helpers, test skeletons, SDK parity script |
| Phase 3: US1 MVP | ✅ Complete | 60 tests pass on net10.0, all packages upgraded |
| Phase 4: US2 Tooling | ✅ Complete | tasks.json, launch.json, backend-ci.yml all aligned |
| Phase 5: US3 Docs | ✅ Complete | README, quickstart updated with .NET 10 + C# 12 + correct paths |
| Phase 6: Polish | ✅ Complete | 73/73 tests pass, no stale net9.0 refs, API surface unchanged |

### net9.0 Reference Sweep

_No `net9.0` references should remain in backend/tooling/docs after migration._

| File | Reference Found | Fixed | Date |
|------|----------------|-------|------|
| RecipeApi.csproj | No | N/A | 2026-03-05 |
| RecipeApi.Tests.csproj | No | N/A | 2026-03-05 |
| .vscode/tasks.json | No | N/A | 2026-03-05 |
| .vscode/launch.json | No | N/A | 2026-03-05 |
| .github/workflows/backend-ci.yml | No | N/A | 2026-03-05 |
| README.md | No | N/A | 2026-03-05 |
| quickstart.md (recipe) | No | N/A | 2026-03-05 |
| _Spec/migration artifacts_ | Historical refs only (expected) | N/A | 2026-03-05 |
