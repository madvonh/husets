# Data Model: Cosmos DB Emulator for Local Development

**Feature**: 002-cosmosdb-emulator-setup
**Date**: 2026-03-06

## Summary

This feature introduces **no data model changes**. The existing domain entities (`Recipe`, `RecipeIngredient`) and their Cosmos DB serialization remain unchanged. The feature is purely a configuration and infrastructure concern.

## Existing Entities (unchanged)

### Recipe

| Field | Type | Cosmos DB | Notes |
|---|---|---|---|
| `Id` | `string` | Document ID | GUID, unique |
| `Pk` | `string` | Partition key (`/pk`) | Default `"recipe"` |
| `Type` | `string` | Discriminator | `"Recipe"` |
| `Title` | `string` | Indexed | Required |
| `RawText` | `string` | Stored | Full recipe text |
| `ImageRef` | `string` | Stored | Blob storage reference |
| `SearchText` | `string` | Indexed | Denormalized for search |
| `NormalizedTags` | `List<string>` | Stored/Queryable | Tag-based filtering |
| `CreatedAt` | `DateTime` | Stored | UTC |
| `UpdatedAt` | `DateTime` | Stored | UTC |

### RecipeIngredient

| Field | Type | Cosmos DB | Notes |
|---|---|---|---|
| `Id` | `string` | Document ID | GUID, unique |
| `Pk` | `string` | Partition key (`/pk`) | Default `"recipe"` |
| `Type` | `string` | Discriminator | `"RecipeIngredient"` |
| `RecipeId` | `string` | Stored | FK to Recipe |
| `FreeText` | `string` | Stored | Raw ingredient text |
| `CanonicalName` | `string?` | Stored | Parsed ingredient name |
| `Position` | `int` | Stored | Display order |
| `CreatedAt` | `DateTime` | Stored | UTC |

## Container Configuration

| Setting | Value | Source |
|---|---|---|
| Database name | Configurable | `CosmosDb:DatabaseName` |
| Container name | Configurable | `CosmosDb:ContainerName` |
| Partition key path | `/pk` | Hard-coded (matches `Recipe.Pk` / `RecipeIngredient.Pk`) |
| Throughput | Default (serverless/emulator) | Not specified |

## Configuration Model (new)

The `CosmosDb` configuration section uses the same keys for both emulator and production:

```json
{
  "CosmosDb": {
    "ConnectionString": "<emulator or azure connection string>",
    "DatabaseName": "RecipeCollectionDb",
    "ContainerName": "Recipes"
  }
}
```

### Environment Variants

| Environment | ConnectionString | SSL Bypass |
|---|---|---|
| Local (emulator) | `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+...` | Yes (auto-detected via localhost) |
| Production (Azure) | Azure Cosmos DB connection string from App Service config | No |
| Tests (in-memory) | Not configured (empty/missing) | N/A — uses `InMemoryCosmosDbService` |

## State Transitions

No new state transitions. The existing Recipe lifecycle (Create → Read → Update → Delete) is unchanged. The only new runtime state is the `CosmosDbInitializer.IsInitialized` flag:

```
App Startup → CosmosDbInitializer.StartAsync
  ├── Success → IsInitialized = true (health: healthy)
  └── Failure → IsInitialized = false, exception logged (health: unhealthy)
```
