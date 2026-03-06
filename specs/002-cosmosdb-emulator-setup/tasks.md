# Tasks: Cosmos DB Emulator for Local Development

**Feature**: 002-cosmosdb-emulator-setup
**Date**: 2026-03-06
**Plan**: [plan.md](plan.md)

## Phase 1: Configuration (Setup)

- [x] T01: Add CosmosDb section to `appsettings.Development.json` and emulator connection string to `launchSettings.json` [FR-001, FR-003]
- [x] T02: Add CosmosDb placeholder section to `appsettings.json` for production [FR-002, FR-003]

## Phase 2: Core Services

- [x] T03: Create `CosmosDbInitializer` IHostedService for database/container auto-creation with resilient startup [FR-004, FR-012]
- [x] T04: Update `Program.cs` Cosmos registration with three-tier logic: in-memory fallback, localhost SSL bypass, production defaults [FR-001, FR-002, FR-005, FR-011]
- [x] T05: Register `CosmosDbInitializer` as hosted service in `Program.cs` [FR-004]

## Phase 3: Health Check

- [x] T06: Refactor `CosmosDbHealthCheck` to use DI-injected CosmosClient and CosmosDbInitializer [FR-006, FR-012]

## Phase 4: Validation

- [x] T07: Run existing tests — all 67 pass without modification [FR-005, SC-005]

## Phase 5: Documentation

- [x] T08: Update quickstart.md with correct local ports from launchSettings.json [FR-009, FR-010]
