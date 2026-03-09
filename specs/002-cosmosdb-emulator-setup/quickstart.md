# Quickstart: Cosmos DB Emulator for Local Development

**Feature**: `002-cosmosdb-emulator-setup`
**Date**: 2026-03-06

This guide explains how to run the backend API locally against the Windows Cosmos DB Emulator while keeping Azure Cosmos DB for production.

## Prerequisites

- Windows machine
- .NET SDK 10.0.103 or compatible with `src/backend/global.json`
- Cosmos DB Emulator (Windows desktop application)
- Git

Optional:
- Node.js 22 LTS if running the frontend

## Step 1: Install and Start the Cosmos DB Emulator

1. Install the official Windows Cosmos DB Emulator from Microsoft.
2. Start the emulator from the Start Menu.
3. Wait until the emulator reports that it is running.
4. Verify the local endpoint opens in a browser:
   - `https://localhost:8081/_explorer/index.html`

Default emulator connection string:

```text
AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
```

## Step 2: Configure Backend Settings

The emulator connection string is pre-configured in `src/backend/src/RecipeApi/Properties/launchSettings.json` for both the `http` and `https` launch profiles. Database and container names are in `src/backend/src/RecipeApi/appsettings.Development.json`.

No manual configuration is needed. The defaults are:

| Setting | Value | Source |
|---|---|---|
| Connection string | Emulator well-known key | `launchSettings.json` (env var `CosmosDb__ConnectionString`) |
| Database name | `RecipeCollection` | `appsettings.Development.json` |
| Container name | `RecipeData` | `appsettings.Development.json` |

If you need to override the connection string (e.g., different port), edit the `CosmosDb__ConnectionString` environment variable in `launchSettings.json`.

Notes:
- The backend automatically disables SSL certificate validation only for localhost emulator connections.
- No manual certificate trust step is required.
- If `CosmosDb:ConnectionString` is missing or empty, the backend falls back to the in-memory implementation used by tests.
- The connection string is in `launchSettings.json` (not `appsettings.Development.json`) so that automated tests continue to use the in-memory fallback.

## Step 3: Run the Backend

From the repository root:

```powershell
cd src/backend

dotnet restore

dotnet run --project src/RecipeApi/RecipeApi.csproj
```

Expected behavior:
- The API starts normally.
- On first run, the database and container are created automatically if they do not already exist.
- The health endpoint returns healthy when the emulator is reachable.

Check health:

```powershell
Invoke-RestMethod -Uri "http://localhost:5137/health"
```

Or if using the `https` launch profile:

```powershell
Invoke-RestMethod -Uri "https://localhost:7137/health" -SkipCertificateCheck
```

## Step 4: Verify Persistence

1. Create a recipe through the API.
2. Stop the API.
3. Start the API again.
4. Retrieve the recipe and confirm it still exists.

This confirms the emulator is being used instead of the in-memory fallback.

## Step 5: Production Configuration

Production uses the same configuration keys:

- `CosmosDb:ConnectionString`
- `CosmosDb:DatabaseName`
- `CosmosDb:ContainerName`

Only the connection string value changes.

For Azure App Service, set the Azure Cosmos DB connection string in app settings or Key Vault-backed configuration. SSL validation remains enabled for all non-localhost endpoints.

## Troubleshooting

### Emulator not running

Symptoms:
- `/health` returns unhealthy
- Startup logs show Cosmos connection failures

Fix:
- Start the Cosmos DB Emulator and wait for it to finish booting.
- Restart the API if needed.

### SSL or certificate errors

Symptoms:
- TLS or certificate validation failure when connecting to `localhost:8081`

Fix:
- Confirm the connection string points to `localhost` or `127.0.0.1`.
- The backend only applies SSL bypass for localhost endpoints.
- If a custom hostname is used, SSL validation remains enabled and the emulator certificate may fail.

### Port 8081 already in use

Symptoms:
- Emulator fails to start
- Browser cannot reach `https://localhost:8081`

Fix:
- Reconfigure the emulator to use a different port.
- Update `CosmosDb:ConnectionString` to match the new port.
- Restart the API.

### API starts but health is unhealthy

This is expected when Cosmos DB is unreachable.

Behavior:
- The API must still start successfully.
- The health endpoint reports unhealthy until connectivity is restored.
- Existing automated tests are unaffected because they do not configure a connection string.

## Frontend (Optional)

If running the frontend:

```powershell
cd src/frontend
npm install
npm run dev
```

Set the frontend API base URL to the backend URL you are running locally.
