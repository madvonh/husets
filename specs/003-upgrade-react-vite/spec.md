# Feature Specification: Upgrade Frontend to React 19.2, Vite 7, and TypeScript 5.9

**Feature Branch**: `003-upgrade-react-vite`  
**Created**: 2026-03-09  
**Status**: Draft  
**Input**: User description: "Upgrade react to 19.2 + and React DOM to 19.2 + use Vite 7+ and typescript 5.9+. Dont create tests for versions. Existing tests should work after upgrade."

## Clarifications

### Session 2026-03-09

- Q: Which Node runtime should the upgraded frontend support in local and CI workflows? → A: Require Node 22 LTS for local and CI.
- Q: Which package manager should remain authoritative for the upgraded frontend workflow? → A: Standardize on npm with a single authoritative package-lock.json.
- Q: What browser compatibility target should this upgrade preserve? → A: Preserve current evergreen browser support with no new legacy-browser requirement.
- Q: Which user journeys should be included in post-upgrade smoke verification? → A: Smoke-check every existing frontend screen and interaction.
- Q: What code-quality configuration should the upgraded frontend use? → A: Use ESLint 9+ with flat config format for frontend code quality.
- Q: What development-tooling behavior should the upgraded frontend preserve? → A: Use the Vite dev server with HMR, TypeScript 5.9+, and a configured tsBuildInfoFile for incremental builds.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Keep the Frontend Working After Dependency Upgrade (Priority: P1)

As a developer, I want the frontend application to install, start, and build successfully on the upgraded dependency baseline so ongoing feature work is not blocked.

**Why this priority**: If the frontend cannot run after the upgrade, the migration delivers no usable value and halts development.

**Independent Test**: Can be fully tested by performing a clean dependency install, running type-check validation, starting the local frontend workflow, and producing a successful production build on the upgraded baseline.

**Acceptance Scenarios**:

1. **Given** a clean checkout of the repository, **When** a developer installs frontend dependencies on a supported environment, **Then** dependency resolution completes without unresolved upgrade-related conflicts.
2. **Given** the upgraded frontend baseline is in place, **When** a developer starts the local frontend workflow, **Then** the Vite development server runs with HMR and the application loads successfully without upgrade-related runtime failures.
3. **Given** the upgraded frontend baseline is in place, **When** a developer runs frontend validation before building, **Then** type-check completes successfully without upgrade-related errors.
4. **Given** the upgraded frontend baseline is in place, **When** a developer runs the production build workflow, **Then** the build completes successfully without upgrade-related compilation errors.

---

### User Story 2 - Preserve Existing Validation Confidence (Priority: P2)

As a maintainer, I want existing validation and test workflows to remain green after the upgrade so the team can trust that the migration did not introduce regressions.

**Why this priority**: Passing validation is the main evidence that the dependency upgrade is safe to merge.

**Independent Test**: Can be tested by running the existing automated checks and confirming that any failures introduced by the dependency upgrade are resolved without adding tests that only assert version numbers.

**Acceptance Scenarios**:

1. **Given** the upgrade branch, **When** the existing frontend-related validation workflows are executed, **Then** they complete successfully after any compatibility fixes required by the upgrade.
2. **Given** the upgrade branch, **When** maintainers review the test changes, **Then** no new tests exist whose sole purpose is to assert dependency version values.
3. **Given** the upgraded toolchain is configured, **When** maintainers run code-quality validation, **Then** ESLint uses the flat config format and completes successfully on the frontend codebase.

---

### User Story 3 - Keep Shared Tooling and Guidance Accurate (Priority: P3)

As a contributor, I want workspace commands and setup guidance to match the upgraded frontend baseline so I can work with the project without trial-and-error.

**Why this priority**: Dependency upgrades often fail in practice when scripts, docs, or shared tooling still assume older versions.

**Independent Test**: Can be tested by following the documented frontend workflow and verifying that shared commands still work on the upgraded baseline.

**Acceptance Scenarios**:

1. **Given** updated workspace configuration and documentation, **When** a contributor follows the documented frontend workflow, **Then** they can start and build the frontend without relying on outdated version assumptions.
2. **Given** shared frontend commands are used in local development, **When** contributors invoke them after the upgrade, **Then** the commands run against the upgraded baseline rather than legacy settings.

---

### Edge Cases

- A supporting package required by the frontend runtime or build pipeline is not compatible with the upgraded baseline.
- The upgraded toolchain changes default behavior and exposes existing code that depends on deprecated patterns.
- Local development works, but production build or type-check validation fails because configuration files still reflect older assumptions.
- Existing automated checks pass partially, but one workflow still references older dependency behavior or older generated artifacts.
- The upgrade can be completed only by adding version-assertion tests, which is explicitly disallowed for this feature.
- The main flows work after the upgrade, but an infrequently used existing screen or interaction regresses.
- The application builds once, but incremental TypeScript rebuilds are misconfigured because the tsBuildInfoFile path is missing or unstable.
- The development server starts, but HMR no longer updates changed frontend modules reliably.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The frontend application dependency baseline MUST use React version 19.2 or higher.
- **FR-002**: The frontend DOM rendering dependency baseline MUST use React DOM version 19.2 or higher.
- **FR-003**: The frontend build tooling baseline MUST use Vite version 7 or higher, TypeScript version 5.9 or higher, and ESLint version 9 or higher.
- **FR-004**: Supporting frontend dependencies and configuration MUST be updated as needed to keep dependency installation, local start, type-check, and production build workflows operational on the upgraded baseline.
- **FR-005**: Existing frontend behavior visible to users MUST remain functionally equivalent after the upgrade unless a change is required to preserve compatibility and does not degrade the current user experience.
- **FR-006**: Existing automated tests and validation workflows relevant to the frontend MUST pass after the upgrade is complete, including npm install, lint, type-check, build, and frontend CI validation.
- **FR-007**: The feature MUST NOT add new tests whose sole purpose is to verify package version numbers, dependency metadata, or lockfile values.
- **FR-008**: If the upgrade reveals deprecated or incompatible frontend code paths, those code paths MUST be updated within this feature to preserve current application behavior.
- **FR-009**: Shared frontend scripts, workspace commands, and contributor guidance MUST reflect the upgraded baseline in the same change set.
- **FR-010**: Dependency incompatibilities introduced by the upgrade MUST be resolved or documented as release blockers before the feature is considered complete.
- **FR-011**: The upgrade scope MUST remain limited to the frontend runtime, frontend toolchain, and compatibility work required to keep existing behavior and validation intact.
- **FR-012**: Local development and CI workflows for the upgraded frontend baseline MUST use Node 22 LTS.
- **FR-013**: The upgraded frontend workflow MUST standardize on npm as the package manager and maintain a single authoritative package-lock.json.
- **FR-014**: The upgrade MUST preserve the application's current evergreen browser compatibility and MUST NOT expand scope to add new legacy-browser support requirements.
- **FR-015**: Post-upgrade smoke verification MUST cover every existing frontend screen and interaction currently available in the application, including app load, search, clear search, navigation between screens, OCR/upload, recipe save, recipe detail load, add tag, remove tag, and retry/error states.
- **FR-016**: Frontend code-quality configuration MUST use the ESLint 9+ flat config format.
- **FR-017**: Local frontend development MUST use the Vite dev server with HMR enabled.
- **FR-018**: TypeScript incremental builds MUST use a configured tsBuildInfoFile so repeated validation runs remain stable and reproducible.

### Key Entities *(include if feature involves data)*

- **Frontend Dependency Baseline**: The approved set of runtime and build-tool versions required for frontend development and delivery.
- **Frontend Validation Workflow**: Existing commands and automated checks used to prove the upgraded frontend remains installable, buildable, and behaviorally stable.
- **Contributor Workflow Guidance**: Repository instructions and shared commands that tell contributors how to run and validate the frontend locally.
- **Compatibility Fix**: Any code or configuration adjustment required to keep current frontend behavior working on the upgraded baseline.
- **Code Quality Configuration**: The linting rules and format used to validate frontend code consistently across local and CI workflows.
- **Incremental Build Metadata**: The TypeScript build-info output used to support repeatable incremental compilation during frontend development and validation.

## Assumptions

- Existing user-facing frontend functionality remains in scope only to the extent needed to preserve current behavior during the upgrade.
- The repository's existing validation workflows provide sufficient coverage to detect upgrade-related regressions without adding tests that only check version values.
- Local and CI environments can be aligned to a runtime supported by the upgraded frontend toolchain.
- Backend APIs and contracts do not need to change unless a frontend compatibility issue directly requires it.
- Node 22 LTS is available for both contributor machines and CI environments used by this repository.
- npm remains the repository package manager for frontend dependency installation and script execution.
- Existing user-facing browser support is limited to current evergreen browsers.
- The repository accepts ESLint flat config as the canonical frontend lint configuration format.

## Dependencies

- Availability of ecosystem packages compatible with React 19.2+, React DOM 19.2+, Vite 7+, ESLint 9+, and TypeScript 5.9+.
- Availability of local and CI environments that provide Node 22 LTS for the upgraded frontend toolchain.
- Availability of npm-based dependency resolution compatible with the upgraded frontend package set.
- Access to existing frontend validation workflows and contributor documentation that must be kept in sync with the upgrade.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A clean frontend dependency install succeeds on the upgraded baseline in two consecutive validation runs.
- **SC-002**: The standard local frontend start workflow and production build workflow both complete successfully in two consecutive validation runs after the upgrade.
- **SC-003**: 100% of existing automated checks relevant to the frontend pass with zero unresolved failures attributable to the dependency upgrade.
- **SC-004**: A contributor following repository guidance can complete frontend setup and first successful local start/build workflow in under 20 minutes.
- **SC-005**: Manual smoke verification of core frontend flows — app load, home search, add recipe, and recipe detail load — completes with zero blocking regressions after the upgrade.
- **SC-006**: The upgrade change set introduces zero new tests whose only acceptance target is dependency version verification.
- **SC-007**: Local and CI validation for the frontend complete successfully using Node 22 LTS in two consecutive runs.
- **SC-008**: Frontend dependency installation succeeds using npm and the committed package-lock.json in two consecutive validation runs.
- **SC-009**: Manual smoke verification on supported evergreen browsers shows zero new blocking compatibility regressions after the upgrade.
- **SC-010**: Manual smoke verification covering every existing frontend screen and all required interactions from `FR-015` completes with zero blocking regressions after the upgrade.
- **SC-011**: Frontend lint validation completes successfully using ESLint flat config in two consecutive runs after the upgrade.
- **SC-012**: Two consecutive incremental TypeScript validation runs reuse the configured tsBuildInfoFile without producing blocking configuration errors.
