# husets Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-05

## Active Technologies
- C# 12 / .NET 10.0 (SDK 10.0.103 pinned via `src/backend/global.json`) + Microsoft.Azure.Cosmos 3.57.1, ASP.NET Core Minimal API, FluentValidation 11.3.1 (002-cosmosdb-emulator-setup)
- Azure Cosmos DB (production) + Cosmos DB Emulator Windows app (local dev) + InMemoryCosmosDbService (tests) (002-cosmosdb-emulator-setup)

- C# 12 (`net10.0`) backend, TypeScript 5.x frontend + ASP.NET Core Minimal API, Microsoft.NET.Test.Sdk, xUnit, Azure SDK libraries (Cosmos/Blob/Vision), FluentValidation, Vite/React toolchain (001-upgrade-dotnet10-csharp12)

## Project Structure

```text
src/backend/
src/frontend/
src/backend/tests/
```

## Commands

npm test; npm run lint

## Code Style

C# 12 (`net10.0`) backend, TypeScript 5.x frontend: Follow standard conventions

## Recent Changes
- 002-cosmosdb-emulator-setup: Added C# 12 / .NET 10.0 (SDK 10.0.103 pinned via `src/backend/global.json`) + Microsoft.Azure.Cosmos 3.57.1, ASP.NET Core Minimal API, FluentValidation 11.3.1

- 001-upgrade-dotnet10-csharp12: Added C# 12 (`net10.0`) backend, TypeScript 5.x frontend + ASP.NET Core Minimal API, Microsoft.NET.Test.Sdk, xUnit, Azure SDK libraries (Cosmos/Blob/Vision), FluentValidation, Vite/React toolchain

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
