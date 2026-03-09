# Tasks: Frontend Tooling Upgrade

**Input**: Design documents from `/specs/003-upgrade-react-vite/`
**Prerequisites**: plan.md (required), spec.md (required), checklists/requirements.md

**Tests**: No new version-only automated tests are added for this feature. Constitution-compliant fail-first validation evidence is required before compatibility fixes, followed by existing frontend workflows (`npm` install, lint, type-check, build), CI execution, and manual smoke verification of current screens/interactions.
**Organization**: Tasks are grouped by user story so each story is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create shared tracking artifacts for the upgrade.

- [x] T001 Create upgrade validation log in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T002 Create dependency compatibility tracker in `specs/003-upgrade-react-vite/dependency-compatibility.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the shared frontend toolchain baseline required before any user-story work starts.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T003 Capture pre-upgrade workflow and configuration gaps against the new baseline in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T004 Capture the required screen-and-interaction smoke matrix from `FR-015` in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T005 Update frontend dependency baseline, Node 22 `engines`, and npm validation scripts in `src/frontend/package.json`
- [x] T006 Regenerate the authoritative npm lockfile for the upgraded toolchain in `src/frontend/package-lock.json`
- [x] T007 Create ESLint 9 flat config for the frontend in `src/frontend/eslint.config.mjs`
- [x] T008 Configure incremental TypeScript build metadata and `tsBuildInfoFile` in `src/frontend/tsconfig.json`
- [x] T009 [P] Configure incremental TypeScript metadata for the Vite config project in `src/frontend/tsconfig.node.json`
- [x] T010 [P] Ignore TypeScript incremental build artifacts in `src/frontend/.gitignore`
- [x] T011 Document the resolved dependency and tooling compatibility set in `specs/003-upgrade-react-vite/dependency-compatibility.md`

**Checkpoint**: Frontend dependency, lint, and incremental TypeScript baseline is ready for user-story implementation.

---

## Phase 3: User Story 1 - Keep the Frontend Working After Dependency Upgrade (Priority: P1) 🎯 MVP

**Goal**: Keep the frontend install, type-check, dev-server, and production build workflow operational on the upgraded baseline.

**Independent Test**: From a clean checkout, run npm install, run type-check, start the Vite dev server with HMR, and complete a production build successfully.

### Validation for User Story 1 (run first, fail first where applicable)

- [x] T012 [US1] Capture pre-fix recipe detail navigation failure evidence in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T013 [US1] Run upgraded type-check/build validation after foundational changes and record failing app-compatibility evidence in `specs/003-upgrade-react-vite/migration-validation.md`

### Implementation for User Story 1

- [x] T014 [P] [US1] Update the React 19 app entry compatibility in `src/frontend/src/main.tsx`
- [x] T015 [P] [US1] Align the recipe detail route definition with the upgraded navigation flow in `src/frontend/src/App.tsx`
- [x] T016 [P] [US1] Fix recipe detail navigation to match the routed URL structure in `src/frontend/src/pages/Home.tsx`
- [x] T017 [P] [US1] Apply React/Vite/TypeScript compatibility fixes for the add recipe flow in `src/frontend/src/pages/AddRecipe.tsx`
- [x] T018 [P] [US1] Apply React/Vite/TypeScript compatibility fixes for the recipe detail flow in `src/frontend/src/pages/RecipeDetail.tsx`
- [x] T019 [P] [US1] Apply React/Vite/TypeScript compatibility fixes for frontend API calls in `src/frontend/src/services/api.ts`
- [x] T020 [US1] Run npm install, `npm run type-check`, `npm run dev`, and `npm run build`, then record results in `specs/003-upgrade-react-vite/migration-validation.md`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - Preserve Existing Validation Confidence (Priority: P2)

**Goal**: Keep frontend validation workflows green after the upgrade without adding version-only tests.

**Independent Test**: Run lint, type-check, build, and CI-equivalent validation successfully on the upgraded baseline.

### Validation for User Story 2 (run first, fail first where applicable)

- [x] T021 [US2] Capture the pre-implementation frontend CI gap and validation workflow mismatch in `specs/003-upgrade-react-vite/migration-validation.md`

### Implementation for User Story 2

- [x] T022 [US2] Create Node 22 frontend CI workflow for npm install, lint, type-check, and build in `.github/workflows/frontend-ci.yml`
- [x] T023 [US2] Update the compatibility tracker with final lint/build/type-check decisions in `specs/003-upgrade-react-vite/dependency-compatibility.md`
- [x] T024 [US2] Run `npm run lint` and record ESLint 9 flat-config validation evidence in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T025 [US2] Run `npm run type-check` twice and record `tsBuildInfoFile` stability evidence in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T026 [US2] Run the CI-equivalent npm install, lint, type-check, and build flow, then record that no version-only tests were introduced in `specs/003-upgrade-react-vite/migration-validation.md`

**Checkpoint**: User Story 2 is independently functional and testable.

---

## Phase 5: User Story 3 - Keep Shared Tooling and Guidance Accurate (Priority: P3)

**Goal**: Align workspace tooling and contributor guidance with the upgraded frontend baseline.

**Independent Test**: Follow the updated documentation and shared commands to start and validate the frontend without outdated assumptions.

### Validation for User Story 3 (run first, fail first where applicable)

- [x] T027 [US3] Capture outdated Node/tooling/documentation references requiring alignment in `specs/003-upgrade-react-vite/migration-validation.md`

### Implementation for User Story 3

- [x] T028 [P] [US3] Update the frontend VS Code task for the upgraded Vite workflow in `.vscode/tasks.json`
- [x] T029 [P] [US3] Update frontend browser launch profiles for the upgraded dev server in `.vscode/launch.json`
- [x] T030 [US3] Update repository frontend status, Node 22 guidance, and npm workflow notes in `README.md`
- [x] T031 [US3] Update frontend local run instructions for Node 22, npm, and Vite in `specs/001-recipe-collection/quickstart.md`
- [x] T032 [US3] Update the frontend prerequisite from Node 18 to Node 22 in `specs/002-cosmosdb-emulator-setup/quickstart.md`
- [x] T033 [US3] Create feature-specific upgrade verification instructions in `specs/003-upgrade-react-vite/quickstart.md`

**Checkpoint**: User Story 3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and completion updates across all stories.

- [x] T034 Run manual smoke verification for app load, search, clear search, and retry/error states in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T035 Run manual smoke verification for add recipe navigation, OCR/upload, and recipe save in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T036 Run manual smoke verification for recipe detail navigation, detail load, add tag, and remove tag in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T037 Verify evergreen-browser smoke coverage and Vite HMR behavior notes in `specs/003-upgrade-react-vite/migration-validation.md`
- [x] T038 Reconcile final compatibility notes and release blockers in `specs/003-upgrade-react-vite/dependency-compatibility.md`
- [x] T039 Update completion notes in `specs/003-upgrade-react-vite/checklists/requirements.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2 and validates the upgraded baseline.
- **Phase 5 (US3)**: Depends on Phase 2 and aligns tooling/docs with the upgraded baseline.
- **Phase 6 (Polish)**: Depends on the completion of all targeted user stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other user stories.
- **US2 (P2)**: Starts after Foundational; validates the baseline established by Foundational and US1 work.
- **US3 (P3)**: Starts after Foundational; can proceed in parallel with US2 once shared tool decisions are stable.

### Within Each User Story

- Fail-first validation tasks must run before the relevant implementation tasks for that story.
- Shared configuration must be completed before app, CI, or documentation updates.
- Post-implementation validation tasks must run after the relevant implementation tasks for that story.
- Each story should meet its independent test criteria before moving to later phases.

---

## Parallel Execution Examples

### User Story 1

- Run together: T014 in `src/frontend/src/main.tsx`, T015 in `src/frontend/src/App.tsx`, and T016 in `src/frontend/src/pages/Home.tsx`
- Run together: T017 in `src/frontend/src/pages/AddRecipe.tsx`, T018 in `src/frontend/src/pages/RecipeDetail.tsx`, and T019 in `src/frontend/src/services/api.ts`

### User Story 2

- Run together: T022 in `.github/workflows/frontend-ci.yml` and T023 in `specs/003-upgrade-react-vite/dependency-compatibility.md`
- Then run: T024, T025, and T026 sequentially in `specs/003-upgrade-react-vite/migration-validation.md`

### User Story 3

- Run together: T028 in `.vscode/tasks.json` and T029 in `.vscode/launch.json`
- Run together: T030 in `README.md`, T031 in `specs/001-recipe-collection/quickstart.md`, and T032 in `specs/002-cosmosdb-emulator-setup/quickstart.md`

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. **STOP and VALIDATE**: Confirm npm install, type-check, Vite dev server with HMR, and production build succeed.

### Incremental Delivery

1. Complete Setup + Foundational to establish the upgraded frontend baseline.
2. Deliver US1 and validate install/dev/build workflow.
3. Deliver US2 and validate lint/type-check/build/CI confidence.
4. Deliver US3 and validate shared tooling/docs.
5. Finish with smoke verification and checklist completion.

### Parallel Team Strategy

1. Complete Setup + Foundational together.
2. After Foundational:
   - Engineer A: US1 runtime and app compatibility.
   - Engineer B: US2 validation workflow and CI.
   - Engineer C: US3 tooling and documentation alignment.
3. Merge for final Polish validation.

---

## Notes

- [P] tasks touch different files and have no dependency on incomplete work in the same phase.
- No new version-only tests should be introduced anywhere in this feature.
- Manual smoke verification must cover every existing frontend screen and interaction.
- Commit after each task or logical task group.
- Stop at each checkpoint to validate the story independently.
