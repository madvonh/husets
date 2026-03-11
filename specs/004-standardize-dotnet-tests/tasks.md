# Tasks: Standardize .NET Test Tooling

**Input**: Design documents from `/specs/004-standardize-dotnet-tests/`  
**Prerequisites**: plan.md (required), spec.md (required), checklists/requirements.md

**Tests**: Follow the constitution's test-first rule. Add fail-first validation for approved backend test stack requirements before migrating the test project and test files. Final validation must prove the full active backend test project passes twice consecutively and that automated enforcement rejects non-approved primary test frameworks.
**Organization**: Tasks are grouped by user story so each story is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create shared tracking artifacts and baseline evidence for the migration.

- [x] T001 Create migration validation log in `specs/004-standardize-dotnet-tests/migration-validation.md`
- [x] T002 Capture current backend test project baseline, package references, and passing/failing status in `specs/004-standardize-dotnet-tests/migration-validation.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish fail-first governance checks and the approved backend test stack baseline before any broad migration work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T003 Add fail-first test-stack governance checks for active backend test projects in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`
- [x] T004 Add fail-first current-guidance checks for backend testing docs in `src/backend/tests/RecipeApi.Tests/PlatformValidation/DocumentationBaselineTests.cs`
- [x] T005 Run `dotnet test` and record failing governance evidence in `specs/004-standardize-dotnet-tests/migration-validation.md`
- [x] T006 Update active backend test project packages/usings to the approved stack in `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`

**Checkpoint**: Governance checks exist, fail for the old stack, and the active test project is ready for file-by-file migration.

---

## Phase 3: User Story 1 - Use One Approved Test Stack for Backend Changes (Priority: P1) 🎯 MVP

**Goal**: Migrate the full active backend test project to the approved stack so maintainers can write and update backend tests consistently.

**Independent Test**: Convert the full active backend test project to the approved stack and prove all migrated tests pass locally.

### Validation for User Story 1 (run first, fail first where applicable)

- [x] T007 [US1] Record the active xUnit-based test syntax and fixture baseline in `specs/004-standardize-dotnet-tests/migration-validation.md`

### Implementation for User Story 1

- [x] T008 [P] [US1] Migrate foundational integration tests to the approved stack in `src/backend/tests/RecipeApi.Tests/FoundationalTests.cs`
- [x] T009 [P] [US1] Migrate unit-style normalization tests to the approved stack in `src/backend/tests/RecipeApi.Tests/SearchNormalizationTests.cs`
- [x] T010 [P] [US1] Migrate search integration tests to the approved stack in `src/backend/tests/RecipeApi.Tests/SearchTests.cs`
- [x] T011 [P] [US1] Migrate tag-management integration tests to the approved stack in `src/backend/tests/RecipeApi.Tests/TagManagementTests.cs`
- [x] T012 [US1] Migrate validation and test-host override tests to the approved stack in `src/backend/tests/RecipeApi.Tests/ValidationTests.cs`
- [x] T013 [US1] Remove or replace obsolete placeholder tests in `src/backend/tests/RecipeApi.Tests/UnitTest1.cs`
- [x] T014 [US1] Run `dotnet test` and record full-suite passing evidence in `specs/004-standardize-dotnet-tests/migration-validation.md`

**Checkpoint**: The full active backend test project uses the approved stack and passes locally.

---

## Phase 4: User Story 2 - Keep Test Execution Reliable for Local and CI Validation (Priority: P2)

**Goal**: Keep local and CI backend test execution aligned and enforce the approved stack automatically.

**Independent Test**: Run the standardized test suite locally and in CI-equivalent mode, and prove enforcement fails when non-approved frameworks are introduced.

### Validation for User Story 2 (run first, fail first where applicable)

- [x] T015 [US2] Capture current CI/backend-test execution assumptions in `specs/004-standardize-dotnet-tests/migration-validation.md`

### Implementation for User Story 2

- [x] T016 [US2] Finalize automated framework enforcement checks in `src/backend/tests/RecipeApi.Tests/PlatformValidation/ToolingParityTests.cs`
- [x] T017 [US2] Align documentation baseline checks with the approved test stack in `src/backend/tests/RecipeApi.Tests/PlatformValidation/DocumentationBaselineTests.cs`
- [x] T018 [US2] Verify backend CI continues to run the standardized suite in `.github/workflows/backend-ci.yml`
- [x] T019 [US2] Run `dotnet test` twice and record stable repeated-pass evidence in `specs/004-standardize-dotnet-tests/migration-validation.md`

**Checkpoint**: Local and CI validation are aligned and framework regression is automatically blocked.

---

## Phase 5: User Story 3 - Make the Testing Standard Clear to Contributors (Priority: P3)

**Goal**: Update active contributor-facing guidance so new backend tests follow the approved stack by default.

**Independent Test**: Follow current documentation and confirm it directs contributors to the approved backend test stack.

### Validation for User Story 3 (run first, fail first where applicable)

- [x] T020 [US3] Capture outdated backend testing guidance in `specs/004-standardize-dotnet-tests/migration-validation.md`

### Implementation for User Story 3

- [x] T021 [US3] Update active backend testing guidance in `README.md`
- [x] T022 [US3] Update current backend run/test guidance in `specs/001-recipe-collection/quickstart.md`
- [x] T023 [US3] Add feature-specific migration verification instructions in `specs/004-standardize-dotnet-tests/quickstart.md`
- [x] T024 [US3] Run documentation/governance validation and record results in `specs/004-standardize-dotnet-tests/migration-validation.md`

**Checkpoint**: Current contributor-facing guidance points to the approved backend testing standard.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and completion updates across all stories.

- [x] T025 Reconcile final migration notes and blockers in `specs/004-standardize-dotnet-tests/migration-validation.md`
- [x] T026 Update completion notes in `specs/004-standardize-dotnet-tests/checklists/requirements.md`
- [x] T027 Mark all completed tasks in `specs/004-standardize-dotnet-tests/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2 and validates the migrated suite.
- **Phase 5 (US3)**: Depends on Phase 2 and can proceed after current guidance scope is confirmed.
- **Phase 6 (Polish)**: Depends on all targeted user stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other user stories.
- **US2 (P2)**: Starts after Foundational and validates the standardized suite.
- **US3 (P3)**: Starts after Foundational and aligns current contributor-facing guidance.

### Within Each User Story

- Fail-first validation tasks must run before the relevant implementation tasks for that story.
- Project/package migration must happen before the full file-by-file test migration.
- Repeated validation runs must happen after the relevant migration steps are complete.

---

## Notes

- [P] tasks touch different files and have no dependency on incomplete work in the same phase.
- The active backend test project and all of its existing tests are in scope for migration.
- Future .NET test projects must use the same approved stack.
- Non-mocking helpers may remain; collaborator replacements should use NSubstitute.
- The feature is incomplete until all tests in the active backend test project pass.
