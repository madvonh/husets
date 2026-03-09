# Implementation Plan: Standardize .NET Test Tooling

**Branch**: `004-standardize-dotnet-tests` | **Date**: 2026-03-09 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-standardize-dotnet-tests/spec.md`

## Summary

Standardize the active backend test project on `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk` while preserving current backend validation confidence and aligning local and CI execution. The work is primarily a test-stack migration across the active .NET test project, test-source conversion, regression enforcement, CI verification, and current contributor-facing documentation. Per the clarified spec, the full active backend test project is in scope, all migrated tests must pass, future .NET test projects must use the same approved stack, non-mocking helpers may remain, and automated validation must reject non-approved primary test frameworks.

## Technical Context

**Language/Version**: C# 12 on `net10.0` for backend application and test projects  
**Primary Dependencies**: ASP.NET Core test host, `Microsoft.NET.Test.Sdk`, `NUnit`, `NSubstitute`, coverage collection package if compatible  
**Storage**: N/A for the test migration itself; tests exercise the existing backend API and service behavior  
**Testing**: Test-first migration workflow, repeated `dotnet test` validation, platform-validation checks for regression enforcement  
**Target Platform**: Local .NET 10 development and CI execution on the repository’s backend workflow  
**Project Type**: Backend API with one active .NET test project under `src/backend/tests`  
**Performance Goals**: No regression in backend test discovery/execution reliability; stable repeated local and CI test runs  
**Constraints**: Use `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`; migrate the full active backend test project; all tests must pass after migration; preserve `net10.0` and C# 12; update current guidance only; add automated enforcement against non-approved frameworks  
**Scale/Scope**: One active backend test project, multiple integration-heavy test files, project configuration, current CI workflow, and current contributor-facing docs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Pre-Phase 0 gate evaluation:

- ✅ Backend and test projects remain on `net10.0`.
- ✅ C# 12 baseline remains unchanged.
- ✅ Test-first intent is preserved: fail-first validation/enforcement is planned before migration work.
- ✅ Planned work aligns with the constitution's approved backend .NET test stack requirement: `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`.
- ✅ CI/backend validation remains required and in scope.
- ✅ No API contract or frontend security boundary changes are introduced.
- ✅ Simplicity is preserved: work is limited to the active backend test project, its supporting validation, and current guidance.

Post-Phase 1 design re-check:

- ✅ Planned work stays within constitution constraints and requires no governance exception.
- ✅ Validation, CI parity, and guidance alignment are explicitly included.
- ✅ Regression prevention for future .NET test projects is covered through documentation plus automated validation.
- ✅ The implementation scope matches the constitution's approved backend test-stack and review requirements.

## Project Structure

### Documentation (this feature)

```text
specs/004-standardize-dotnet-tests/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── backend/
    ├── Directory.Build.props
    ├── global.json
    ├── RecipeCollection.sln
    ├── src/
    │   └── RecipeApi/
    └── tests/
        └── RecipeApi.Tests/
            ├── RecipeApi.Tests.csproj
            ├── FoundationalTests.cs
            ├── SearchNormalizationTests.cs
            ├── SearchTests.cs
            ├── TagManagementTests.cs
            ├── UnitTest1.cs
            ├── ValidationTests.cs
            └── PlatformValidation/
                ├── DocumentationBaselineTests.cs
                └── ToolingParityTests.cs

.github/
└── workflows/
    └── backend-ci.yml

README.md
```

**Structure Decision**: Keep the existing backend API plus single active test-project structure. Concentrate changes in the active backend test project, platform-validation tests, backend CI verification, and current contributor-facing guidance.

## Phase Plan

### Phase 0 — Baseline & Discovery

- Confirm the active backend test project, current package references, current framework usage, and the authoritative current test count.
- Confirm where current contributor-facing guidance references backend test tooling.
- Confirm the exact migration hotspots, especially lifecycle-heavy integration tests and DI override patterns.
- Confirm an enforcement approach for rejecting non-approved frameworks using an existing repository validation pattern.

Output: planning-ready migration baseline and source-of-truth validation target.

### Phase 1 — Design & Enforcement Strategy

- Define the approved package/configuration set for the active test project on `net10.0`.
- Define the fail-first enforcement strategy for package/framework regression prevention.
- Define the NUnit lifecycle pattern for integration-heavy tests using `WebApplicationFactory<Program>`.
- Define the doc update surface limited to current contributor-facing guidance.

Output: implementation-ready migration and verification design captured in this plan.

### Phase 2 — Task Planning Hand-off

- Break the work into fail-first validation tasks, configuration migration tasks, file-by-file test migration tasks, CI/guidance updates, and final verification tasks.
- Sequence tasks so enforcement and failing validation happen before the project/package conversion, followed by test migration, then CI/doc alignment, then final repeated validation.

Output: ready for `/speckit.tasks`.

## Implementation Outline

1. Capture fail-first evidence for the current mixed-framework state in the active backend test project and current guidance.
2. Add or extend automated validation in the existing platform-validation area so non-approved primary .NET test frameworks are rejected in active test projects.
3. Update [src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj](./../../src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj) to remove `xUnit` references and adopt the approved NUnit-based stack while preserving `net10.0` compatibility.
4. Migrate lower-risk test files first by converting attributes, assertions, and setup semantics in foundational and unit-style test files.
5. Migrate integration-heavy files next, especially [src/backend/tests/RecipeApi.Tests/ValidationTests.cs](./../../src/backend/tests/RecipeApi.Tests/ValidationTests.cs), using a consistent NUnit lifecycle approach around `WebApplicationFactory<Program>` and DI overrides.
6. Apply `NSubstitute` only where collaborator replacements are actually needed, while retaining non-mocking helpers such as fixtures, builders, and test data factories.
7. Remove obsolete placeholders and any lingering `xUnit` usings, package references, or framework-specific assumptions from the active backend test project.
8. Update current contributor-facing guidance and confirm backend CI still runs the standardized suite through the existing `dotnet test` path.
9. Finish with repeated full-suite validation and enforcement verification so the active backend test project is fully passing and future regressions are blocked.

## Verification Strategy

- Run repeated `dotnet test` validation against [src/backend/RecipeCollection.sln](./../../src/backend/RecipeCollection.sln).
- Confirm all tests in [src/backend/tests/RecipeApi.Tests](./../../src/backend/tests/RecipeApi.Tests) are discovered and pass in two consecutive runs after migration.
- Confirm automated validation fails when a non-approved primary .NET test framework is introduced into an active .NET test project.
- Confirm current contributor-facing guidance points contributors to `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`.
- Confirm CI parity with [ .github/workflows/backend-ci.yml](./../../.github/workflows/backend-ci.yml).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
