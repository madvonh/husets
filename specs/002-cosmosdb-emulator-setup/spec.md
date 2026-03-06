# Feature Specification: Cosmos DB Emulator for Local Development

**Feature Branch**: `002-cosmosdb-emulator-setup`  
**Created**: 2026-03-05  
**Status**: Draft  
**Input**: User description: "Use Cosmos DB Emulator - Windows application for local development and Azure Cosmos DB for production"

## Clarifications

### Session 2026-03-06

- Q: How should the emulator's self-signed SSL certificate be handled? → A: Disable SSL validation for emulator (localhost) only — no manual cert trust needed.
- Q: What should happen if Cosmos DB is unreachable at API startup? → A: Start normally, report unhealthy via health endpoint (resilient startup).
- Q: Should emulator integration tests be included in this feature? → A: Out of scope — keep existing in-memory tests only; emulator integration tests are a future feature.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Local Development with Cosmos DB Emulator (Priority: P1)

A developer clones the repository, installs the Cosmos DB Emulator (Windows desktop application), starts it, and runs the backend API locally. The API connects to the local emulator for all data operations (recipes, ingredients, tags) without needing an Azure subscription or internet access. The developer can create, read, search, and delete recipes using the emulator as the data store.

**Why this priority**: This is the core enabler — without a working local Cosmos DB connection, developers cannot iterate on data-dependent features. It replaces the current in-memory fallback with a real Cosmos-compatible store, making local development behavior match production.

**Independent Test**: Start the Cosmos DB Emulator, configure the backend with the emulator connection string, run the API, and exercise all CRUD endpoints against actual persisted data. Restart the API and confirm data survives across restarts.

**Acceptance Scenarios**:

1. **Given** the Cosmos DB Emulator is installed and running on the developer's Windows machine, **When** the developer starts the backend API with the emulator connection string configured, **Then** the API starts successfully, the health endpoint returns 200, and the database and container are created automatically if they do not exist.
2. **Given** the API is running against the emulator, **When** the developer creates a recipe via the API, **Then** the recipe is persisted in the emulator and can be retrieved by ID.
3. **Given** the API is running against the emulator, **When** the developer searches for recipes, **Then** results are returned from the emulator's data store (not from an in-memory store).
4. **Given** the API is running and data has been created, **When** the developer restarts the API, **Then** previously created data is still available (data persists across API restarts).
5. **Given** the Cosmos DB Emulator is not running, **When** the developer starts the backend API with the emulator connection string configured, **Then** the API starts successfully, the health endpoint returns an unhealthy status, and the API logs a clear error indicating the emulator is unreachable.

---

### User Story 2 — Production Azure Cosmos DB Connection (Priority: P2)

When deployed to Azure (or configured with a real Azure Cosmos DB connection string), the API connects to the Azure Cosmos DB account and operates identically to the local emulator experience. Configuration is environment-driven so no code changes are needed between local and production.

**Why this priority**: Production readiness is essential but depends on the local development story being solid first. The existing `CosmosDbService` already works with Azure Cosmos DB; this story ensures the configuration flow and documentation clearly support both environments.

**Independent Test**: Configure the backend with an Azure Cosmos DB connection string, run the API, and verify all CRUD operations work against the cloud database. Confirm the same configuration keys are used for both emulator and production.

**Acceptance Scenarios**:

1. **Given** the API is configured with an Azure Cosmos DB connection string, **When** the API starts, **Then** it connects to the Azure Cosmos DB account successfully and the health endpoint returns 200.
2. **Given** the same API code is used for both local and production, **When** only the connection string configuration value changes between environments, **Then** the API works correctly against both the emulator and Azure Cosmos DB without code changes.
3. **Given** the API is deployed to Azure App Service, **When** the connection string is provided via App Service configuration (environment variable), **Then** the API uses the Azure Cosmos DB account for all data operations.

---

### User Story 3 — Developer Onboarding Documentation (Priority: P3)

A new contributor reads the quickstart guide and can set up the Cosmos DB Emulator, configure the backend, and run the full local development flow within 15 minutes. The documentation covers installation, startup, configuration, and troubleshooting.

**Why this priority**: Good documentation reduces onboarding friction and support burden. It depends on US1 being stable so the instructions are accurate.

**Independent Test**: A new contributor follows the quickstart guide from scratch and successfully runs the API against the emulator without additional help.

**Acceptance Scenarios**:

1. **Given** a new contributor reads the quickstart documentation, **When** they follow the setup steps, **Then** they can install and start the Cosmos DB Emulator, configure the backend, and run the API successfully.
2. **Given** a developer encounters a common issue (emulator not starting, SSL certificate errors, port conflicts), **When** they consult the troubleshooting section, **Then** they find actionable guidance to resolve the issue.

---

### Edge Cases

- What happens when the emulator's SSL certificate is not trusted by the .NET SDK? The backend MUST automatically disable SSL validation when connecting to localhost (emulator). No manual certificate trust steps required from developers.
- What happens when the emulator's default port (8081) is already in use? The documentation should explain how to configure an alternative port.
- What happens when the database or container does not exist on first run? The API should create them automatically.
- What happens when the connection string is missing entirely? The current in-memory fallback behavior should be preserved so tests continue to work without any Cosmos DB instance.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The backend API MUST connect to the Cosmos DB Emulator (Windows desktop application) when running locally, using the emulator's well-known connection string.
- **FR-002**: The backend API MUST connect to Azure Cosmos DB when deployed to production, using a connection string provided via environment configuration.
- **FR-003**: The same configuration keys (`CosmosDb:ConnectionString`, `CosmosDb:DatabaseName`, `CosmosDb:ContainerName`) MUST be used for both emulator and production connections — only the connection string value changes.
- **FR-004**: The backend API MUST automatically create the database and container on startup if they do not already exist (for both emulator and production).
- **FR-005**: The in-memory Cosmos DB fallback MUST be preserved for automated tests and environments where no Cosmos DB connection string is configured.
- **FR-006**: The health endpoint MUST accurately report Cosmos DB connectivity status — healthy when reachable, unhealthy when the emulator or Azure Cosmos DB is unreachable.
- **FR-007**: Data created via the API against the emulator MUST persist across API restarts (i.e., the emulator stores data on disk, not in-memory).
- **FR-008**: Local development MUST NOT require an Azure subscription or internet access (the emulator runs entirely offline).
- **FR-009**: Developer documentation MUST provide step-by-step setup instructions for the Cosmos DB Emulator (Windows desktop application), including installation, startup, and backend configuration.
- **FR-010**: The documentation MUST include troubleshooting guidance for common emulator issues (SSL certificates, port conflicts, startup failures).
- **FR-011**: The backend MUST automatically disable SSL certificate validation when connecting to the Cosmos DB Emulator on localhost. Developers MUST NOT be required to manually install or trust the emulator's self-signed certificate. SSL validation MUST remain enabled for all non-localhost (production) connections.
- **FR-012**: The backend API MUST start normally even if the configured Cosmos DB instance (emulator or production) is unreachable. It MUST NOT fail or crash on startup due to database unavailability. The health endpoint MUST report unhealthy status when the database is unreachable.

### Key Entities

- **Cosmos DB Emulator**: Windows desktop application providing a local Cosmos DB-compatible endpoint for development. Runs on `https://localhost:8081` by default.
- **Azure Cosmos DB**: Cloud-hosted production database service. Connection string provided per-environment.
- **Backend Configuration**: Environment-driven settings (`CosmosDb:ConnectionString`, `CosmosDb:DatabaseName`, `CosmosDb:ContainerName`) that determine which Cosmos DB instance the API connects to.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can start the backend API against the Cosmos DB Emulator and successfully create, read, search, and delete recipes within 5 minutes of starting the emulator.
- **SC-002**: The same backend code runs against both the local emulator and Azure Cosmos DB with only a connection string change — zero code modifications required.
- **SC-003**: Data persists across API restarts when using the emulator — a recipe created before restart is retrievable after restart.
- **SC-004**: A new contributor can complete the full local setup (emulator install, backend configuration, first successful API call) within 15 minutes by following the quickstart documentation.
- **SC-005**: All existing automated tests continue to pass without modification (in-memory fallback preserved).
- **SC-006**: The health endpoint correctly reports unhealthy status within 10 seconds when the configured Cosmos DB instance is unreachable.

## Assumptions

- Developers are on Windows (the Cosmos DB Emulator is a Windows-only desktop application).
- The Cosmos DB Emulator is installed from the official Microsoft installer (not the Docker/Linux variant).
- The existing `CosmosDbService` and `ICosmosDbService` interface are already compatible with both the emulator and Azure Cosmos DB (same SDK, same wire protocol).
- The existing `InMemoryCosmosDbService` fallback is preserved and remains the default when no connection string is configured.
- No changes to the API surface or endpoint behavior are required — this feature is purely a data layer configuration and documentation change.
- Automated integration tests that require the emulator to be running are explicitly out of scope for this feature. All existing automated tests continue to use the in-memory fallback.

## Dependencies

- Cosmos DB Emulator (Windows desktop application) must be installed by each developer individually.
- The `Microsoft.Azure.Cosmos` NuGet package (already included) supports both the emulator and Azure Cosmos DB.
