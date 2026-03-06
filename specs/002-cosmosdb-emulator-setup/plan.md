# Implementation Plan: Cosmos DB Emulator for Local Development

**Branch**: `002-cosmosdb-emulator-setup` | **Date**: 2026-03-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-cosmosdb-emulator-setup/spec.md`

## Summary

Configure the backend API to connect to the Cosmos DB Emulator (Windows desktop application) for local development and Azure Cosmos DB for production. The implementation adds SSL bypass for localhost connections, auto-creation of database/container on startup, resilient startup when the database is unreachable, and developer documentation. The existing in-memory fallback is preserved for automated tests. No API surface changes.

## Technical Context

**Language/Version**: C# 12 / .NET 10.0 (SDK 10.0.103 pinned via `src/backend/global.json`)
**Primary Dependencies**: Microsoft.Azure.Cosmos 3.57.1, ASP.NET Core Minimal API, FluentValidation 11.3.1
**Storage**: Azure Cosmos DB (production) + Cosmos DB Emulator Windows app (local dev) + InMemoryCosmosDbService (tests)
**Testing**: xUnit 2.9.3 (73 existing tests, all must continue passing; emulator integration tests explicitly out of scope)
**Target Platform**: Windows (local dev with emulator), Azure App Service (production)
**Project Type**: Web application (backend + frontend)
**Performance Goals**: N/A — configuration and documentation feature; no performance-sensitive code paths introduced
**Constraints**: SSL validation disabled only for localhost; resilient startup (never crash on DB unavailability); zero code changes between local and production
**Scale/Scope**: Backend API only; ~6 files modified, ~2 files created, documentation artifacts

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Constitution Rule | Status | Notes |
|---|---|---|---|
| I | Static-First Web App | PASS | No frontend changes |
| II | API-First Backend | PASS | No API surface changes; existing OpenAPI contract unchanged |
| III | Test-First (NON-NEGOTIABLE) | PASS with caveat | No new runtime behavior requiring new unit tests — this is configuration + documentation. Existing 73 tests must pass unchanged (FR-005). Health check behavior should be validated by existing health check test. If `Program.cs` wiring changes, existing foundational tests cover startup. |
| IV | Cloud-Ready Data Access | PASS | Cosmos DB remains system-of-record. Partition key (`Pk`) already defined. Auto-create uses same partition key config. No data model changes. |
| V | Security, Observability, Simplicity | PASS | SSL bypass scoped to localhost only (FR-011). Structured logging preserved. No secrets in source — emulator uses well-known key. Connection strings via config/environment variables. |
| — | Platform Requirements (net10.0 / C# 12) | PASS | Already on net10.0 / C# 12 per feature 001 |
| — | Health endpoint required | PASS | Existing `/health` endpoint; enhanced to correctly report emulator connectivity (FR-006, FR-012) |
| — | Cosmos DB: transient failure handling | PASS | Existing `CosmosDbService` already implements retry with exponential backoff for 429/503/408/500 |
| — | Config via environment variables | PASS | Same keys used (`CosmosDb:ConnectionString`, `CosmosDb:DatabaseName`, `CosmosDb:ContainerName`) |

**Pre-Phase 0 Gate: PASS** — No violations. Proceed to research.

## Project Structure

### Documentation (this feature)

```text
specs/002-cosmosdb-emulator-setup/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (minimal — no schema changes)
├── quickstart.md        # Phase 1 output (emulator setup guide)
├── contracts/           # Phase 1 output (no API changes — health contract note only)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (files affected by this feature)

```text
src/backend/src/RecipeApi/
├── Program.cs                              # MODIFY: CosmosClient options (SSL bypass, auto-create DB/container, resilient startup)
├── appsettings.Development.json            # MODIFY: Add emulator connection string + DB/container names
├── appsettings.json                        # MODIFY: Add CosmosDb section placeholder for production
├── Services/
│   ├── CosmosDbService.cs                  # MODIFY: Accept CosmosClient via DI (already does), add startup initialization
│   ├── CosmosDbHealthCheck.cs              # MODIFY: Use injected CosmosClient, handle SSL for emulator
│   └── CosmosDbInitializer.cs              # CREATE: Database/container auto-creation logic (IHostedService)
└── ...

src/backend/tests/RecipeApi.Tests/
└── (no changes — existing 73 tests must pass as-is)
```

**Structure Decision**: Existing web application structure under `src/backend/` and `src/frontend/`. This feature modifies backend only. A new `CosmosDbInitializer.cs` service handles database/container creation on startup as an `IHostedService`.

## Complexity Tracking

No constitution violations to justify.

## Post-Design Constitution Check

| # | Constitution Rule | Status | Notes |
|---|---|---|---|
| I | Static-First Web App | PASS | No frontend runtime or hosting changes introduced |
| II | API-First Backend | PASS | Contract artifact added for health behavior; no endpoint additions |
| III | Test-First (NON-NEGOTIABLE) | PASS | Design preserves existing in-memory tests; implementation must keep test-first ordering when code changes begin |
| IV | Cloud-Ready Data Access | PASS | Uses existing Cosmos partition key path `/pk`; no schema or migration changes |
| V | Security, Observability, Simplicity | PASS | SSL bypass remains localhost-only; startup failures are logged and surfaced through health checks |

**Post-Phase 1 Gate: PASS** — Research and design artifacts align with the constitution. Ready for Phase 2 task planning.
