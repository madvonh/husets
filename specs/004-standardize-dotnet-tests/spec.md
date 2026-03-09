# Feature Specification: Standardize .NET Test Tooling

**Feature Branch**: `004-standardize-dotnet-tests`  
**Created**: 2026-03-09  
**Status**: Draft  
**Input**: User description: "Standardize .NET testing on NUnit, NSubstitute, and Microsoft.NET.Test.Sdk"

## Clarifications

### Session 2026-03-09

- Q: How strictly should the repository standardize test doubles when moving to the approved stack? → A: Require NSubstitute for collaborator replacements in migrated or newly added tests, but allow non-mocking helpers such as builders, fixtures, and test data factories.
- Q: How much of the active backend test project must be migrated in this feature? → A: Migrate all existing tests in the active backend test project to the approved stack in this feature.
- Q: How should this standard apply beyond the current backend test project? → A: Migrate the current backend test project now, and require any future .NET test project to use the same approved stack.
- Q: What passing threshold defines completion for the migrated active backend test project? → A: This feature is only complete when all tests in the active backend test project pass after migration.
- Q: How should the repository prevent regressions to non-approved .NET test frameworks after migration? → A: Use both repository guidance and automated validation that fails if non-approved test frameworks are introduced into active .NET test projects.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Use One Approved Test Stack for Backend Changes (Priority: P1)

As a backend maintainer, I want all .NET tests in the repository to use the same approved testing stack so I can add or update tests without switching frameworks or conventions.

**Why this priority**: A single testing standard removes day-to-day confusion, reduces maintenance cost, and directly affects every future backend behavior change.

**Independent Test**: Can be fully tested by reviewing the active .NET test project configuration, adding or updating representative tests, and confirming the standard test workflow runs successfully without relying on mixed frameworks.

**Acceptance Scenarios**:

1. **Given** a maintainer adds a new backend test, **When** the test is created in the repository's active or future .NET test project, **Then** it uses the approved test framework, approved substitute library, and approved .NET test runner integration.
2. **Given** existing backend tests use older conventions, **When** this feature is completed, **Then** all existing tests in the active backend test project follow the approved testing standard instead of a mixed-framework approach.
3. **Given** a maintainer updates a backend behavior, **When** they add or revise test coverage for that behavior, **Then** the test can be written using the approved stack without needing compatibility workarounds for a second framework.

---

### User Story 2 - Keep Test Execution Reliable for Local and CI Validation (Priority: P2)

As a maintainer, I want local and CI test execution to use the same standardized .NET test setup so validation results stay predictable and trustworthy.

**Why this priority**: Standardization only creates value if the same tests can be discovered, executed, and reported consistently across contributor machines and automation.

**Independent Test**: Can be tested by running the standard backend test workflow locally and in CI-equivalent conditions and confirming that test discovery and execution succeed without framework-specific failures.

**Acceptance Scenarios**:

1. **Given** the standardized .NET test setup is in place, **When** a maintainer runs the backend test workflow locally, **Then** all tests in the active backend test project are discovered and pass successfully using the approved stack.
2. **Given** the standardized .NET test setup is in place, **When** CI runs backend validation, **Then** the same test project configuration is used and automated validation fails if non-approved test frameworks are introduced into active .NET test projects.
3. **Given** the repository contains test support packages, **When** maintainers review the final configuration, **Then** only packages required by the approved testing standard remain for the active .NET test project.

---

### User Story 3 - Make the Testing Standard Clear to Contributors (Priority: P3)

As a contributor, I want the repository's testing guidance to clearly state the approved .NET testing approach so I can write compliant tests on my first attempt.

**Why this priority**: Clear guidance prevents reintroducing mixed test frameworks after the migration is complete.

**Independent Test**: Can be tested by following the documented guidance for adding or updating a backend test and confirming that the instructions lead to a compliant result without extra clarification.

**Acceptance Scenarios**:

1. **Given** a contributor needs to add a backend test, **When** they follow repository guidance, **Then** they are directed to use NUnit, NSubstitute, and Microsoft.NET.Test.Sdk for the current backend test project and any future .NET test project.
2. **Given** a contributor reviews existing backend tests after this feature, **When** they compare project setup and examples, **Then** the approved testing pattern is consistent and easy to follow.

---

### Edge Cases

- An existing test depends on assertions or lifecycle behavior that does not map directly to the approved standard and must be rewritten rather than mechanically converted.
- A backend test project references packages from more than one test framework, causing ambiguous discovery or duplicate runner behavior.
- A contributor adds a new test file using an outdated framework after the migration unless repository guidance and validation clearly reject it.
- A test relies on mocking behavior that should be expressed with substitutes under the approved stack but currently uses handwritten mocks or another mocking library, while still allowing non-mocking helpers such as builders, fixtures, and test data factories to remain.
- Local execution succeeds, but CI fails because runner integration or package references are not aligned with the approved standard.
- A future change reintroduces a non-approved test framework package or usage pattern unless automated validation blocks it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The active backend test project and any future .NET test project in the repository MUST standardize on NUnit as the unit and integration test framework.
- **FR-002**: The active backend test project and any future .NET test project in the repository MUST use Microsoft.NET.Test.Sdk as the .NET test runner integration package.
- **FR-003**: Tests that require collaborator replacements in migrated or newly added tests MUST use NSubstitute as the approved substitute library, while non-mocking helpers such as builders, fixtures, and test data factories MAY remain in use.
- **FR-004**: The repository MUST migrate all existing tests in the active backend test project to the approved testing standard and remove mixed-framework usage so contributors are not required to choose between competing test frameworks, and future .NET test projects MUST start on that same standard.
- **FR-005**: Existing backend test coverage relevant to current application behavior MUST remain executable and passing after the standardization change.
- **FR-006**: The standardized test setup MUST support local test discovery and execution through the repository's normal .NET test workflow.
- **FR-007**: The standardized test setup MUST support CI validation without requiring framework-specific exceptions or alternate execution paths.
- **FR-008**: Repository guidance for backend testing MUST explicitly identify NUnit, NSubstitute, and Microsoft.NET.Test.Sdk as the required stack for new and updated .NET tests.
- **FR-008a**: Automated validation MUST fail when a non-approved primary .NET test framework is introduced into an active .NET test project.
- **FR-009**: New or updated backend tests introduced as part of future behavior changes MUST follow the approved testing stack.
- **FR-010**: Standardization scope MUST include the full active backend test project configuration, all existing test source files in that project, and contributor guidance required to prevent regression to mixed frameworks.
- **FR-011**: The feature MUST NOT reduce current validation confidence for backend behavior covered by the existing automated tests, and the migrated active backend test project MUST be fully passing before the feature is complete.
- **FR-012**: Any legacy test package or framework usage left outside the approved standard MUST be explicitly documented as out of scope or removed from the active .NET test project.

### Key Entities *(include if feature involves data)*

- **.NET Test Project Standard**: The approved repository-wide combination of test framework, substitute library, and .NET test runner integration that all active backend tests must follow.
- **Backend Test Suite**: The current collection of automated .NET tests that verifies backend behavior and must remain executable after standardization.
- **Test Double**: A collaborator replacement used in tests to isolate behavior, which must use the approved substitute approach when such replacement is needed.
- **Contributor Testing Guidance**: Repository instructions and examples that tell contributors how to create compliant backend tests.

## Assumptions

- The standardization work applies to the full active backend test project under `src/backend/tests`, and all existing tests in that project are in scope for migration in this feature.
- Any future .NET test project added to the repository is expected to adopt the same approved testing standard from its initial creation.
- The current backend test suite provides behavior coverage worth preserving and migrating rather than replacing wholesale.
- The repository's constitution requirement for test-first behavior changes remains in effect after the testing stack is standardized.
- Coverlet or equivalent coverage collection may remain if it is compatible with the approved testing stack and does not introduce a second test framework.
- The approved testing standard for this repository is fixed by stakeholder direction and does not require further framework selection analysis.
- Non-mocking test-support patterns such as builders, fixtures, and test data factories remain acceptable where they do not act as substitute-library replacements.

## Dependencies

- Access to the active .NET test project configuration and existing backend test sources that require migration.
- Availability of NUnit, NSubstitute, and Microsoft.NET.Test.Sdk versions compatible with the repository's required .NET 10 backend baseline.
- Availability of local and CI .NET test execution workflows that can validate the standardized setup.
- Availability of repository guidance locations where the approved testing standard can be documented for contributors.
- Availability of a repository validation mechanism that can detect non-approved .NET test framework usage in active test projects.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of the active backend test project uses NUnit, Microsoft.NET.Test.Sdk, and no competing primary test framework after the feature is complete, and the repository guidance defines the same standard for any future .NET test project.
- **SC-002**: 100% of existing tests in the active backend test project are discovered successfully in two consecutive local test runs after standardization.
- **SC-003**: 100% of tests in the active backend test project pass in two consecutive local validation runs after migration.
- **SC-004**: 100% of CI backend test runs use the same standardized .NET test setup with zero failures caused by mixed-framework configuration.
- **SC-004a**: Automated validation rejects 100% of attempted introductions of non-approved primary .NET test frameworks into active .NET test projects.
- **SC-005**: A contributor can identify the approved backend testing stack and add a compliant new test in under 10 minutes using repository guidance alone.
- **SC-006**: Zero active backend test files added or modified after this feature use a non-approved .NET test framework.
- **SC-007**: Zero active test-project package references remain that are only required for a superseded primary test framework.
