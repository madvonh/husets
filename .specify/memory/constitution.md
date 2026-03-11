# Husets Constitution

## Core Principles

### I. Static-First Web App
The frontend is a static web app: prebuilt assets (HTML/CSS/JS) served as static files.
- No server-side rendering requirements by default.
- All backend data access goes through the API (no direct browser access to Cosmos DB).
- No secrets in the frontend bundle; public configuration only.

### II. API-First Backend
All non-trivial behavior that requires data, secrets, or privileged access lives behind a backend HTTP API.
- Stable API surface is defined by an explicit contract (OpenAPI or equivalent) before implementation.
- API is deployable to Azure App Service and runs behind HTTPS only.

### III. Test-First (NON-NEGOTIABLE)
For any behavior change (frontend or backend):
1) Add/adjust tests first (or contract tests for API changes).
2) Verify tests fail for the right reason.
3) Implement the smallest change to make tests pass.
4) Refactor with tests staying green.

For backend .NET test projects:
- The approved test stack is `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`.
- Collaborator replacements in new or migrated tests must use `NSubstitute`.
- Non-mocking helpers such as builders, fixtures, and test data factories may be used when they are not acting as substitute-library replacements.

### IV. Cloud-Ready Data Access
Cosmos DB is the system-of-record database and is accessed only by the API.
- Use a clear partitioning strategy (partition key required for any new container/collection).
- Prefer managed identity (or equivalent non-secret auth) where supported; otherwise secrets are stored in Key Vault, never in source control.
- Data model changes must be backwards compatible or include a migration plan.

### V. Security, Observability, and Simplicity
- Security baseline: least privilege, HTTPS only, validate all inputs, and no secrets in client code.
- Observability baseline: structured logs, correlation IDs, and App Insights-compatible telemetry for the API.
- Simplicity baseline: implement the smallest thing that satisfies the spec; avoid premature abstractions.

## Minimum Platform Requirements

### Static Web App (Frontend)
- Must build to static assets suitable for CDN/static hosting.
- Must have a single configurable API base URL (per environment).
- Must not embed secrets, connection strings, keys, or tokens in the bundle.
- Must handle client-side routing without server rewrites being required by app logic (if routing is needed, it must be compatible with static hosting).
- Frontend code quality must use ESLint flat config on ESLint 9 or newer.
- Frontend development must use the Vite dev server with HMR.
- Frontend TypeScript must target version 5.9 or newer and configure `tsBuildInfoFile` for stable incremental builds.

### Backend API (Azure App Service)
- Must target `net10.0` for all backend application and test projects (full cutover; no dual-targeting with older frameworks).
- Must compile with C# 12 language baseline.
- Must pin SDK selection via `global.json` to a specific `.NET 10.0.x` version, and CI must use the same pinned SDK.
- Active and future backend .NET test projects must use `NUnit` as the primary test framework and `Microsoft.NET.Test.Sdk` for test execution.
- Active and future backend .NET test projects must use `NSubstitute` for collaborator replacements when a substitute library is needed.
- Must expose a health endpoint (e.g., `GET /health`) returning 200 when dependencies are healthy.
- Must return consistent JSON error shapes (at minimum: `message` and `code`).
- Must validate inputs and return appropriate 4xx responses; never rely on Cosmos errors for validation.
- Must support configuration via environment variables/App Service app settings.
- Must log structured events (JSON) and include a request correlation ID in logs.
- Dependency incompatibilities with the required platform baseline are release blockers.

### Cosmos DB
- Must be accessed only by the backend API (never directly from the browser).
- Must define a partition key for each container/collection and use it in queries.
- Must not store secrets in documents.
- Must handle transient failures and rate limiting (429) with bounded retries.

### Secrets and Config
- Secrets belong in Key Vault (or equivalent secret store), not in repo or frontend.
- Prefer managed identity from App Service to access Azure resources.

## Development Workflow and Quality Gates

### Definition of Done (Minimum)
- Tests exist for new/changed behavior and pass locally/CI.
- Backend .NET test projects use the approved stack and do not introduce non-approved primary test frameworks.
- API contract is updated when the API changes.
- Lint/build succeeds for affected projects.
- No secrets committed.

### CI/CD (Minimum)
- Build frontend assets and publish artifacts.
- Run backend tests and produce deployable artifact.
- Use automated validation to reject non-approved primary test frameworks in active backend .NET test projects.
- Deploy API to App Service and frontend to static hosting per environment.

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

This constitution supersedes local conventions when there is a conflict.
- Changes to these rules require updating this file and bumping the version.
- Breaking changes to public API contracts require a migration plan.
- Reviews must explicitly check: security baseline, test-first compliance, approved backend test stack compliance, and “no secrets in frontend/repo”.

**Version**: 1.3.0 | **Ratified**: 2026-02-10 | **Last Amended**: 2026-03-09
