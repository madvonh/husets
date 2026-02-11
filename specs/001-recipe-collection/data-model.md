# Data Model: Recipe Collection

**Feature**: `001-recipe-collection`  
**Date**: 2026-02-10  
**Storage**: Azure Cosmos DB (NoSQL API)

## Overview

The MVP uses a **single-user, single-partition** strategy for simplicity. All documents share a constant partition key (`pk = "recipe"`). This enables straightforward cross-document queries (search, filtering) without cross-partition overhead, suitable for a personal recipe collection.

### Containers

- **Container Name**: `RecipeData`
- **Partition Key**: `/pk` (all documents use `"recipe"` as the value)
- **Indexing**: Default Cosmos indexing, with explicit indexing on `searchText` and `normalizedTags[]` for search performance

## Document Types

All documents in the `RecipeData` container have a `type` field to distinguish their purpose.

---

## 1. Recipe Document

Stores the core recipe metadata, with denormalized fields for search optimization.

```json
{
  "id": "recipe_<guid>",
  "pk": "recipe",
  "type": "Recipe",
  "title": "Chocolate Chip Cookies",
  "rawText": "Ingredients:\n- 2 cups flour\n- 1 cup sugar\n\nInstructions:\nMix and bake at 350°F for 12 minutes.",
  "imageRef": "recipes/2026-02-10/abc123.jpg",
  "searchText": "chocolate chip cookies ingredients 2 cups flour 1 cup sugar instructions mix and bake",
  "normalizedTags": ["dessert", "cookies"],
  "createdAt": "2026-02-10T10:30:00Z",
  "updatedAt": "2026-02-10T10:30:00Z"
}
```

### Fields

- **id**: Unique identifier (`recipe_<guid>`)
- **pk**: Partition key (constant `"recipe"`)
- **type**: `"Recipe"`
- **title**: Recipe title (user-editable)
- **rawText**: Full recipe text (user-edited after OCR)
- **imageRef**: Blob storage reference (path/key for the uploaded image)
- **searchText**: Denormalized, lowercase, space-joined text for MVP "contains" search (derived from title + rawText)
- **normalizedTags**: Array of normalized tag strings (lowercase, trimmed) for fast tag filtering
- **createdAt**: ISO 8601 timestamp
- **updatedAt**: ISO 8601 timestamp

---

## 2. RecipeIngredient Document

Stores each ingredient line as a separate document, linked to the owning recipe.

```json
{
  "id": "recipeingredient_<guid>",
  "pk": "recipe",
  "type": "RecipeIngredient",
  "recipeId": "recipe_<guid>",
  "freeText": "2 cups all-purpose flour",
  "canonicalName": "flour",
  "position": 0,
  "createdAt": "2026-02-10T10:30:00Z"
}
```

### Fields

- **id**: Unique identifier (`recipeingredient_<guid>`)
- **pk**: Partition key (constant `"recipe"`)
- **type**: `"RecipeIngredient"`
- **recipeId**: Foreign key to owning Recipe document
- **freeText**: Original ingredient line exactly as entered (preserved per FR-012)
- **canonicalName**: Best-effort normalized ingredient name (nullable; used for future aggregation/substitution features)
- **position**: Zero-based order in the recipe (preserves list order)
- **createdAt**: ISO 8601 timestamp

---

## 3. Ingredient Document (Optional for MVP)

Stores normalized ingredient names for future lookup/substitution features. MVP may skip this and only populate `canonicalName` on `RecipeIngredient`.

```json
{
  "id": "ingredient_flour",
  "pk": "recipe",
  "type": "Ingredient",
  "canonicalName": "flour",
  "createdAt": "2026-02-10T10:30:00Z"
}
```

### Fields

- **id**: Unique identifier (`ingredient_<canonicalName>`)
- **pk**: Partition key (constant `"recipe"`)
- **type**: `"Ingredient"`
- **canonicalName**: Normalized ingredient name (lowercase, trimmed, common variations collapsed)
- **createdAt**: ISO 8601 timestamp

**Note**: For MVP, this document type is optional. Ingredient normalization can be a simple string transformation without a dedicated lookup table.

---

## 4. Tag Document (Optional for MVP)

Stores normalized tag names. MVP may denormalize tags entirely into `Recipe.normalizedTags` and skip separate Tag documents.

```json
{
  "id": "tag_dessert",
  "pk": "recipe",
  "type": "Tag",
  "canonicalName": "dessert",
  "createdAt": "2026-02-10T10:30:00Z"
}
```

### Fields

- **id**: Unique identifier (`tag_<canonicalName>`)
- **pk**: Partition key (constant `"recipe"`)
- **type**: `"Tag"`
- **canonicalName**: Normalized tag name (lowercase, trimmed)
- **createdAt**: ISO 8601 timestamp

**Note**: For MVP, this document type is optional. Tags can be stored only in `Recipe.normalizedTags[]` for simplicity.

---

## 5. RecipeTag Document (Optional for MVP)

Join document linking recipes to tags. MVP may skip this if tags are denormalized on Recipe.

```json
{
  "id": "recipetag_<guid>",
  "pk": "recipe",
  "type": "RecipeTag",
  "recipeId": "recipe_<guid>",
  "tagCanonicalName": "dessert",
  "createdAt": "2026-02-10T10:30:00Z"
}
```

### Fields

- **id**: Unique identifier (`recipetag_<guid>`)
- **pk**: Partition key (constant `"recipe"`)
- **type**: `"RecipeTag"`
- **recipeId**: Foreign key to Recipe
- **tagCanonicalName**: Foreign key to Tag (or just the normalized tag string)
- **createdAt**: ISO 8601 timestamp

**Note**: For MVP, this document type is optional. Denormalizing tags on Recipe simplifies queries.

---

## Partitioning Strategy

**Chosen Strategy**: Single-user, single-partition (constant `pk = "recipe"`).

### Rationale

- **Personal collection**: Small scale (hundreds to low thousands of recipes).
- **Cross-document queries**: Search needs to span all recipes; single partition avoids cross-partition query overhead.
- **Simpler writes**: No need to route by dynamic keys (e.g., recipeId, userId).
- **Future scaling**: If the collection grows large or multi-user is needed, migrate to partition-by-userId or partition-by-recipeId (see migration plan below).

### Limitations

- Cosmos single partition max: 20 GB storage, 10K RU/s throughput. Acceptable for personal MVP.
- No horizontal scaling within this partition. If the app becomes multi-user, partition by `userId`.

---

## Indexing

Default Cosmos indexing covers most fields. Explicit optimization:

1. **searchText**: Ensure indexed for `CONTAINS` queries (`SELECT * FROM c WHERE CONTAINS(c.searchText, "keyword")`).
2. **normalizedTags**: Array indexing enabled for fast tag filters (`WHERE ARRAY_CONTAINS(c.normalizedTags, "dessert")`).
3. **recipeId**: Indexed for fast lookups of `RecipeIngredient` by recipe.

No custom indexing policy needed initially; use default and monitor query performance.

---

## Data Integrity & Validation

- **Foreign keys**: `recipeId` references are soft (no Cosmos FK constraints). Application validates existence before writes.
- **Normalization**: Tags and ingredient names normalized at write time (lowercase, trim).
- **Idempotency**: Tag add/remove operations check for duplicates before mutating.
- **Deletion**: When a Recipe is deleted, associated `RecipeIngredient` and `RecipeTag` documents must be deleted (application-level cascade).

---

## Migration Plan (Future)

If multi-user support is needed:

1. Add `userId` field to all documents.
2. Change partition key to `/userId` (requires new container + data migration).
3. Update queries to scope by userId.

For MVP, this is out of scope.

---

## Sample Queries

### Get Recipe with Ingredients

```sql
-- Fetch recipe
SELECT * FROM c WHERE c.type = "Recipe" AND c.id = "recipe_abc123"

-- Fetch ingredients for recipe
SELECT * FROM c 
WHERE c.type = "RecipeIngredient" 
  AND c.recipeId = "recipe_abc123" 
ORDER BY c.position
```

### Search Recipes by Keyword

```sql
SELECT * FROM c 
WHERE c.type = "Recipe" 
  AND CONTAINS(LOWER(c.searchText), "chocolate")
ORDER BY c.createdAt DESC
```

### Search Recipes by Tag

```sql
SELECT * FROM c 
WHERE c.type = "Recipe" 
  AND ARRAY_CONTAINS(c.normalizedTags, "dessert")
ORDER BY c.createdAt DESC
```

### Search by Keyword AND Tag

```sql
SELECT * FROM c 
WHERE c.type = "Recipe" 
  AND CONTAINS(LOWER(c.searchText), "cookie")
  AND ARRAY_CONTAINS(c.normalizedTags, "dessert")
ORDER BY c.createdAt DESC
```

---

## Summary

- **Single container** (`RecipeData`) with constant partition key for MVP simplicity.
- **Core documents**: `Recipe` (with denormalized search fields), `RecipeIngredient` (separate items per requirement).
- **Optional documents**: `Ingredient`, `Tag`, `RecipeTag` (can be added later if normalization/reporting is needed).
- **Search**: MVP uses `CONTAINS` over `searchText` and `ARRAY_CONTAINS` over `normalizedTags`.
- **Scalability**: Single-partition acceptable for personal use; migrate to `userId` partition if multi-user needed.
