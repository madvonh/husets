# Implementation Plan: Upgrade to .NET 10 with C# 12

**Branch**: `001-upgrade-dotnet10-csharp12` | **Date**: 2026-03-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-upgrade-dotnet10-csharp12/spec.md`

## Summary

Upgrade backend runtime and toolchain from `net9.0` to `net10.0` with C# 12 as a hard baseline, enforce pinned SDK selection through `global.json`, and align local tooling/docs in the same migration pass. The feature includes project configuration updates, workspace task/launch path corrections, dependency compatibility verification, and validation guidance to ensure local and CI build/test parity.

## Technical Context

**Language/Version**: C# 12 (`net10.0`) backend, TypeScript 5.x frontend  
**Primary Dependencies**: ASP.NET Core Minimal API, Microsoft.NET.Test.Sdk, xUnit, Azure SDK libraries (Cosmos/Blob/Vision), FluentValidation, Vite/React toolchain  
**Storage**: Azure Cosmos DB (API-managed access only), Azure Blob Storage  
**Testing**: `dotnet test` (xUnit integration/unit tests in `src/backend/tests/RecipeApi.Tests`)  
**Target Platform**: Windows local development; Azure App Service for API; static hosting for frontend  
**Project Type**: Web application (`src/backend` + `src/frontend`)  
**Performance Goals**: No regression from existing behavior; maintain current build/test turnaround and startup profile  
**Constraints**: Full cutover to `net10.0` only; no dual-targeting; pinned `.NET 10.0.x` SDK in `global.json`; dependency incompatibilities are release blockers; update CI/workspace/docs together  
**Scale/Scope**: Configuration and tooling migration affecting backend app/test projects, VS Code tasks/launch settings, and developer documentation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Pre-Phase 0 gate evaluation:

- ✅ Static-first frontend preserved (no frontend secret or SSR changes required).
- ✅ API-first constraints preserved (no direct DB access from browser introduced).
- ✅ Test-first process maintained (plan requires validation-first tasks for build/test before refactors).
- ✅ Cloud-ready data access unchanged (Cosmos access pattern unaffected by runtime migration).
- ✅ Security/observability baseline unaffected by scope.
- ✅ Platform requirements explicitly targeted:
  - Backend app + tests move to `net10.0` only.
  - C# 12 baseline enforced.
  - `global.json` pins specific `.NET 10.0.x` SDK, mirrored in CI.

Post-Phase 1 design re-check:

- ✅ Research decisions and artifacts keep all gates satisfied.
- ✅ No constitution violations introduced by design.

## Project Structure

### Documentation (this feature)

```text
specs/001-upgrade-dotnet10-csharp12/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── no-api-surface-change.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── backend/
│   ├── RecipeCollection.sln
│   ├── src/RecipeApi/
│   │   ├── RecipeApi.csproj
│   │   └── Program.cs
│   └── tests/RecipeApi.Tests/
│       └── RecipeApi.Tests.csproj
└── frontend/
    ├── package.json
    └── src/

.vscode/
├── tasks.json
└── launch.json

.github/
└── workflows/            # Created/updated if CI workflow is present/required

specs/001-upgrade-dotnet10-csharp12/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/no-api-surface-change.yaml
```

**Structure Decision**: Use existing web app split under `src/backend` and `src/frontend`; migration changes are concentrated in backend project/test configuration, workspace tooling, and documentation artifacts.

## Phase Plan

### Phase 0 — Research

- Confirm .NET 10 migration strategy for app/test projects.
- Confirm SDK pinning approach using `global.json` for local and CI parity.
- Confirm compatibility policy and fallback options for dependency blockers.
- Confirm scope synchronization (project files + workspace tooling + docs + CI).

Output: `research.md` with explicit Decision / Rationale / Alternatives for each topic.

### Phase 1 — Design & Contracts

- Define configuration entities and validation relationships in `data-model.md`.
- Produce contract artifact documenting that API surface remains unchanged (`contracts/no-api-surface-change.yaml`).
- Produce `quickstart.md` for validating migration end-to-end locally.
- Update agent context for Copilot via `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`.

Output: `data-model.md`, `contracts/no-api-surface-change.yaml`, `quickstart.md`, updated agent context file.

### Phase 2 — Task Planning Hand-off

- Prepare for `/speckit.tasks` with implementation-ready requirements and artifacts.
- Ensure constitution gates are explicitly re-checked and passed.

Output: planning package ready for task generation.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
