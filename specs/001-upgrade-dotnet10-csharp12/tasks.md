# Tasks: Upgrade to .NET 10 with C# 12

**Input**: Design documents from `/specs/001-upgrade-dotnet10-csharp12/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Tests are REQUIRED for this feature (constitution + spec test-first constraints).
**Organization**: Tasks are grouped by user story so each story is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish migration scaffolding and deterministic SDK baseline.

- [x] T001 Create repository SDK pinning file in `global.json`
- [x] T002 Create backend baseline properties file enforcing C# 12 in `src/backend/Directory.Build.props`
- [x] T003 [P] Create migration evidence template in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared migration infrastructure required before user-story implementation.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [x] T004 Create dependency compatibility matrix in `specs/001-upgrade-dotnet10-csharp12/dependency-compatibility.md`
- [x] T005 Create platform assertion helpers in `src/backend/tests/RecipeApi.Tests/PlatformValidation/FileAssertionHelpers.cs`
- [x] T006 [P] Create project baseline test suite skeleton in `src/backend/tests/RecipeApi.Tests/PlatformValidation/PlatformBaselineTests.cs`
- [x] T007 [P] Create tooling/CI parity test suite skeleton in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`
- [x] T008 Create documentation baseline test suite skeleton in `src/backend/tests/RecipeApi.Tests/PlatformValidation/DocumentationBaselineTests.cs`
- [x] T009 Create SDK parity check script in `scripts/powershell/assert-sdk-parity.ps1`

**Checkpoint**: Foundation ready — user story implementation can proceed.

---

## Phase 3: User Story 1 - Development Workflow Operational (Priority: P1) 🎯 MVP

**Goal**: Backend app + tests build and run on `net10.0` with C# 12 using pinned SDK.

**Independent Test**: From clean checkout, run restore/build/test and API startup successfully under pinned `.NET 10.0.x`.

### Tests for User Story 1 (write first, fail first)

- [x] T010 [US1] Add failing test for API project target framework (`net10.0`) in `src/backend/tests/RecipeApi.Tests/PlatformValidation/PlatformBaselineTests.cs`
- [x] T011 [US1] Add failing test for test project target framework and language baseline in `src/backend/tests/RecipeApi.Tests/PlatformValidation/PlatformBaselineTests.cs`
- [x] T012 [US1] Record pre-implementation failing test evidence in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`

### Implementation for User Story 1

- [x] T013 [US1] Update API project TFM/lang settings in `src/backend/src/RecipeApi/RecipeApi.csproj`
- [x] T014 [US1] Update test project TFM/lang settings in `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`
- [x] T015 [US1] Upgrade backend package references for `net10.0` compatibility in `src/backend/src/RecipeApi/RecipeApi.csproj`
- [x] T016 [US1] Upgrade test package references for `net10.0` compatibility in `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`
- [x] T017 [US1] Run backend restore/build/test and capture pass evidence in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - CI and Team Tooling Compatible (Priority: P2)

**Goal**: CI and workspace tooling execute the same pinned SDK + backend workflow.

**Independent Test**: Tooling/CI parity tests pass and CI workflow uses same SDK/version and backend paths.

### Tests for User Story 2 (write first, fail first)

- [x] T018 [US2] Add failing test for VS Code backend task path/cmd alignment in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`
- [x] T019 [US2] Add failing test for VS Code backend launch output path (`net10.0`) in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`
- [x] T020 [US2] Add failing test for CI workflow SDK/version parity with `global.json` in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`

### Implementation for User Story 2

- [x] T021 [US2] Update backend build task paths in `.vscode/tasks.json`
- [x] T022 [US2] Update backend launch configuration output path in `.vscode/launch.json`
- [x] T023 [US2] Create backend CI workflow using pinned SDK and backend restore/build/test in `.github/workflows/backend-ci.yml`
- [x] T024 [US2] Wire CI workflow to run parity script in `.github/workflows/backend-ci.yml`
- [x] T025 [US2] Finalize SDK parity checks in `scripts/powershell/assert-sdk-parity.ps1`
- [x] T026 [US2] Record tooling/CI parity validation evidence in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Documentation Accurate for Onboarding (Priority: P3)

**Goal**: Contributor docs clearly and consistently reflect the new baseline and commands.

**Independent Test**: A contributor can follow docs and complete setup + build/test run without hidden assumptions.

### Tests for User Story 3 (write first, fail first)

- [x] T027 [US3] Add failing test for README platform/version and backend path guidance in `src/backend/tests/RecipeApi.Tests/PlatformValidation/DocumentationBaselineTests.cs`
- [x] T028 [US3] Add failing test for quickstart baseline guidance and command consistency in `src/backend/tests/RecipeApi.Tests/PlatformValidation/DocumentationBaselineTests.cs`

### Implementation for User Story 3

- [x] T029 [US3] Update platform and command guidance in `README.md`
- [x] T030 [US3] Update migration quickstart with pinned SDK and validation flow in `specs/001-upgrade-dotnet10-csharp12/quickstart.md`
- [x] T031 [US3] Update feature quickstart prerequisites/commands in `specs/001-recipe-collection/quickstart.md`
- [x] T032 [US3] Update dependency blocker tracking notes in `specs/001-upgrade-dotnet10-csharp12/research.md`
- [x] T033 [US3] Record documentation walkthrough evidence in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across all stories.

- [x] T034 Run full backend test suite and append final report in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`
- [x] T035 Verify no `net9.0` references remain in backend/tooling/docs and log findings in `specs/001-upgrade-dotnet10-csharp12/migration-validation.md`
- [x] T036 Validate no API surface drift against contract in `specs/001-upgrade-dotnet10-csharp12/contracts/no-api-surface-change.yaml`
- [x] T037 Update completion notes/checks in `specs/001-upgrade-dotnet10-csharp12/checklists/requirements.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately.
- **Phase 2 (Foundational)**: depends on Phase 1; blocks all user stories.
- **Phases 3–5 (User Stories)**: depend on Phase 2 completion.
- **Phase 6 (Polish)**: depends on all targeted user stories being complete.

### User Story Dependencies

- **US1 (P1)**: starts after Foundational; no dependency on other stories.
- **US2 (P2)**: starts after Foundational; should validate parity against US1 baseline outputs.
- **US3 (P3)**: starts after Foundational; depends on stabilized baseline/tooling decisions from US1/US2.

### Within Each User Story

- Test tasks MUST be authored first and fail before implementation tasks begin.
- Configuration changes before validation evidence capture.
- Story checkpoint must be met before moving to next priority.

---

## Parallel Execution Examples

### User Story 1

- Run together: T013 in `src/backend/src/RecipeApi/RecipeApi.csproj` and T014 in `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`
- Then run: T015 + T016 package compatibility updates

### User Story 2

- Run together: T021 in `.vscode/tasks.json` and T022 in `.vscode/launch.json`
- Run together: T023 in `.github/workflows/backend-ci.yml` and T025 in `scripts/powershell/assert-sdk-parity.ps1`

### User Story 3

- Run together: T029 in `README.md` and T031 in `specs/001-recipe-collection/quickstart.md`
- Then run: T030 in `specs/001-upgrade-dotnet10-csharp12/quickstart.md`

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 + Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Stop and validate restore/build/test/API startup under pinned SDK.

### Incremental Delivery

1. Baseline migration (US1) → validate.
2. Tooling/CI parity (US2) → validate.
3. Documentation onboarding (US3) → validate.
4. Cross-cutting polish and final evidence report.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Then parallelize by story owner:
   - Engineer A: US1 baseline/runtime migration.
   - Engineer B: US2 CI/tooling parity.
   - Engineer C: US3 documentation and onboarding validation.
