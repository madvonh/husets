# Research: Cosmos DB Emulator for Local Development

**Feature**: 002-cosmosdb-emulator-setup
**Date**: 2026-03-06

## R1: SSL Certificate Bypass for Cosmos DB Emulator

**Question**: How to disable SSL validation for the emulator's self-signed certificate using Microsoft.Azure.Cosmos SDK v3.x?

**Decision**: Use `CosmosClientOptions.HttpClientFactory` with `HttpClientHandler.DangerousAcceptAnyServerCertificateValidator`, conditional on localhost connection string.

**Rationale**: The Cosmos SDK v3.x has no built-in boolean to skip SSL validation. The `HttpClientFactory` approach is the Microsoft-recommended pattern for emulator connectivity. When `HttpClientFactory` is set, `ConnectionMode.Gateway` is required (Direct mode ignores the factory and uses its own TCP transport). The emulator works correctly in Gateway mode.

**Alternatives considered**:
1. Install emulator's self-signed certificate into OS trust store — works but requires per-developer manual steps, contradicts FR-011 ("developers MUST NOT be required to manually install or trust the certificate").
2. Set `AZURE_COSMOS_EMULATOR_ALLOW_NETWORK_ACCESS` environment variable — not applicable to SSL; only controls network binding.

**Code pattern**:
```csharp
var options = new CosmosClientOptions();
if (connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase))
{
    options.ConnectionMode = ConnectionMode.Gateway;
    options.HttpClientFactory = () =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return new HttpClient(handler);
    };
}
```

**Constraint**: Must also check for `127.0.0.1` in connection strings.

---

## R2: Auto-Create Database and Container on Startup

**Question**: How to ensure the Cosmos DB database and container exist before the first request?

**Decision**: Use an `IHostedService` (not `BackgroundService`) that calls `CreateDatabaseIfNotExistsAsync` and `CreateContainerIfNotExistsAsync` during `StartAsync`.

**Rationale**:
- `IHostedService.StartAsync` runs before Kestrel starts accepting requests (default in .NET 8+), guaranteeing the container exists before first request (if initialization succeeds).
- Both `CreateDatabaseIfNotExistsAsync` and `CreateContainerIfNotExistsAsync` are idempotent — safe to call on every startup (200 OK if existing, 201 Created if new).
- Clean separation from `Program.cs`.
- Testable in isolation.

**Alternatives considered**:
1. Inline in `Program.cs` after `builder.Build()` — blocks startup pipeline, harder to test, messy error handling.
2. `BackgroundService` — wrong tool; designed for continuous loops, not one-shot initialization.
3. Lazy initialization on first request — would delay first request and complicate error handling.

**Parameters**:
- Database name: from `IConfiguration["CosmosDb:DatabaseName"]`
- Container name: from `IConfiguration["CosmosDb:ContainerName"]`
- Partition key path: `/pk` (matches `Recipe.Pk` property serialized as lowercase)
- Throughput: omit (use emulator/serverless defaults)

---

## R3: Resilient Startup Pattern

**Question**: How to ensure the API starts even when Cosmos DB is unreachable?

**Decision**: Wrap `CosmosDbInitializer.StartAsync` in try/catch for `CosmosException`, `HttpRequestException`, and `InvalidOperationException`. Set an `IsInitialized` flag for health check integration. Do not rethrow.

**Rationale**: FR-012 requires the API to start normally even if the database is unavailable. The health endpoint reports unhealthy status (FR-006). Catastrophic exceptions (e.g., `OutOfMemoryException`) should still crash the app.

**Alternatives considered**:
1. Retry in a background loop — over-engineered for this scenario; the developer can restart the app after starting the emulator.
2. Throw and crash — violates FR-012.

---

## R4: Well-Known Emulator Connection String

**Question**: What is the default emulator connection string and endpoint?

**Decision**: Use the publicly documented well-known emulator connection string in `appsettings.Development.json`.

**Details**:
- **Endpoint**: `https://localhost:8081`
- **Account Key**: `C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==`
- **Connection string**: `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==`

**Rationale**: This key is publicly documented by Microsoft and identical across all emulator installations. It is not a secret. Storing it in `appsettings.Development.json` is appropriate — it should NOT be in `appsettings.json` (production config).

---

## R5: Health Check Architecture

**Question**: How should the existing `CosmosDbHealthCheck` integrate with the initializer and SSL bypass?

**Decision**: Refactor `CosmosDbHealthCheck` to use the DI-injected `CosmosClient` singleton instead of creating a new one. Optionally accept `CosmosDbInitializer` to check initialization status.

**Rationale**: The current `CosmosDbHealthCheck` creates a new `CosmosClient` on every health check call, which:
1. Does not inherit the SSL bypass configuration (would fail against emulator).
2. Creates unnecessary client instances.
3. Is inconsistent with the DI-registered client.

By injecting the singleton `CosmosClient`, the health check inherits all configuration (SSL bypass, Gateway mode) automatically.

**Pattern**: Register `CosmosDbInitializer` as both `AddSingleton<CosmosDbInitializer>()` and `AddHostedService(sp => sp.GetRequiredService<CosmosDbInitializer>())` so the same instance is injectable into the health check and runs as a hosted service.

---

## R6: CosmosClient Registration Pattern

**Question**: How to conditionally register `CosmosClient` with SSL bypass and preserve in-memory fallback?

**Decision**: Three-tier registration in `Program.cs`:

1. **No connection string**: Register `InMemoryCosmosDbService` (existing behavior, for tests).
2. **Localhost connection string**: Register `CosmosClient` with SSL bypass + Gateway mode, register `CosmosDbService`, register `CosmosDbInitializer`.
3. **Production connection string**: Register `CosmosClient` with default options, register `CosmosDbService`, register `CosmosDbInitializer`.

**Rationale**: Clean separation. The localhost check (`Contains("localhost")` or `Contains("127.0.0.1")`) is the only branching point. All three paths use the same `ICosmosDbService` interface.
