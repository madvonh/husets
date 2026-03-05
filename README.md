# Recipe Collection - Implementation Status

**Date**: February 10, 2026  
**Feature**: 001-recipe-collection  
**Branch**: (to be created)

## 🎯 Summary

A **partial MVP implementation** of the Recipe Collection app has been completed. The backend API foundation and core User Story 1 (upload photo → OCR → save recipe) endpoints are implemented and building. Frontend scaffolding exists but needs US1, US2, and US3 page implementations. 

**Current State**: Backend ~70% complete (US1 done, US2/US3 need implementation). Frontend ~20% complete (scaffold only, no API integration).

---

## ✅ Completed

### Phase 0: Spec Artifacts
- [x] OpenAPI contract ([specs/001-recipe-collection/contracts/openapi.yaml](specs/001-recipe-collection/contracts/openapi.yaml))
- [x] Data model doc ([specs/001-recipe-collection/data-model.md](specs/001-recipe-collection/data-model.md))
- [x] Quickstart guide ([specs/001-recipe-collection/quickstart.md](specs/001-recipe-collection/quickstart.md))

### Phase 1: Project Scaffolding
- [x] Backend: .NET 9 Minimal API solution ([scr/backend/](scr/backend/))
  - Solution structure: `RecipeApi` project + `RecipeApi.Tests` xUnit project
  - NuGet packages: Cosmos DB, Azure Blob Storage, Azure Vision ImageAnalysis, FluentValidation
- [x] Frontend: Vite + React + TypeScript + Tailwind CSS ([scr/frontend/](scr/frontend/))
  - Builds successfully
  - Responsive baseline (mobile-ready)
  - Placeholder pages: Home, Add Recipe, Recipe Detail

### Phase 2: Foundational Backend Infrastructure
- [x] **Middleware**:
  - Correlation ID middleware (X-Correlation-Id header)
  - Global exception handler (consistent JSON error responses)
- [x] **Validation**: FluentValidation integrated
- [x] **Health checks**: `GET /health` (checks Cosmos connectivity)
- [x] **Cosmos DB service**: CRUD operations, retry logic for 429 rate limits
- [x] **Domain models**: Recipe, RecipeIngredient, DTOs, ErrorResponse
- [x] **Tests**: Foundational integration tests (5 tests passing)

### Phase 3: User Story 1 (Upload Photo → OCR → Save Recipe)
- [x] **Services**:
  - `BlobStorageService`: Upload/download/delete images in Azure Blob Storage
  - `AzureOcrService`: Extract text via Azure AI Vision Read OCR (with placeholder fallback for dev)
  - `IngredientParser`: Heuristic parsing of ingredient lines from raw text
- [x] **Endpoints**:
  - `POST /ocr`: Upload image → returns `{ imageRef, extractedText }`
  - `POST /recipes`: Create recipe with ingredients stored as separate Cosmos items
  - `GET /recipes/{id}`: Retrieve recipe with ingredients + tags + imageRef
- [x] **Tags**: Normalization utility (lowercase, trim, dedupe)

---

## 🚧 Remaining Work

### Backend (High Priority)

#### User Story 2: Search Recipes
- [ ] `GET /recipes?query=&tag=` endpoint (MVP "contains" search over `searchText`, tag filtering over `normalizedTags`)
- [ ] Tests for search functionality

#### User Story 3: Tag Management
- [ ] `POST /recipes/{id}/tags` endpoint (add tag)
- [ ] `DELETE /recipes/{id}/tags/{tag}` endpoint (remove tag)
- [ ] Tests for tag operations

#### Cross-Cutting
- [ ] More comprehensive tests (contract tests for OCR/recipe endpoints, ingredient parsing unit tests)
- [ ] Rate limiting on uploads
- [ ] Max image size validation
- [ ] MIME type restrictions (JPEG/PNG only)

### Frontend (Critical - Not Yet Implemented)

#### User Story 1 Pages
- [ ] **Add Recipe Page** ([scr/frontend/src/pages/AddRecipe.tsx](scr/frontend/src/)):
  - Image upload UI
  - Call `POST /ocr`
  - Display extracted text in editable textarea
  - Title input
  - Optional tags input
  - Call `POST /recipes`
  - Handle errors (correlation ID display)
  - Mobile-responsive
- [ ] **Recipe Detail Page** ([scr/frontend/src/pages/RecipeDetail.tsx](scr/frontend/src/)):
  - Fetch `GET /recipes/{id}`
  - Display title, text, ingredients list, tags, image preview
  - Mobile-responsive

#### User Story 2 Pages
- [ ] **Home/Search Page** ([scr/frontend/src/pages/Home.tsx](scr/frontend/src/)):
  - Free-text search input
  - Tag filter input
  - Call `GET /recipes?query=&tag=`
  - Display results list (title, tags, createdAt)
  - Click to navigate to recipe detail
  - Mobile-responsive

#### User Story 3 UI
- [ ] Tag add/remove UI on Recipe Detail page
  - Call `POST /recipes/{id}/tags`, `DELETE /recipes/{id}/tags/{tag}`

#### Frontend Infrastructure
- [ ] API client service (fetch wrapper with baseURL from env, correlation ID handling)
- [ ] Error boundary component (show correlation ID on errors)
- [ ] Loading states
- [ ] Basic smoke tests (optional)

---

## 📁 File Structure

```
husets/
├── specs/001-recipe-collection/
│   ├── plan.md                     # Implementation plan
│   ├── spec.md                     # Feature spec (user stories)
│   ├── tasks.md                    # Task breakdown (this status doc references it)
│   ├── data-model.md               # Cosmos DB schema
│   ├── quickstart.md               # Local dev guide
│   └── contracts/openapi.yaml      # API contract
├── scr/
│   ├── backend/
│   │   ├── RecipeCollection.sln
│   │   ├── src/RecipeApi/
│   │   │   ├── Program.cs              # Entry point, service registration
│   │   │   ├── Models/                 # Domain models, DTOs, errors
│   │   │   ├── Services/               # Cosmos, Blob, OCR, ingredient parsing, health checks
│   │   │   ├── Middleware/             # Correlation ID, exception handling
│   │   │   ├── Validators/             # FluentValidation rules
│   │   │   ├── Endpoints/              # Recipe endpoints (OCR, CRUD)
│   │   │   └── Utilities/              # Tag normalization, search text builder
│   │   └── tests/RecipeApi.Tests/
│   │       └── FoundationalTests.cs    # Integration tests
│   └── frontend/
│       ├── package.json
│       ├── vite.config.ts
│       ├── tailwind.config.js
│       ├── src/
│       │   ├── main.tsx                # Entry point
│       │   ├── App.tsx                 # Router + placeholder pages
│       │   └── index.css               # Tailwind imports
│       └── .env.example                # API base URL config template
```

---

## 🚀 Next Steps (Recommended Priority)

1. **Frontend US1 Implementation** (Add Recipe + Recipe Detail pages)
   - Highest user value: enables end-to-end flow (upload → save → view)
   - Estimated: 2-3 hours
2. **Frontend US2 Implementation** (Home/Search page)
   - Enables recipe discovery
   - Estimated: 1-2 hours
3. **Backend US2 Implementation** (Search endpoint)
   - Unblocks frontend US2
   - Estimated: 30 minutes
4. **Backend + Frontend US3** (Tag management)
   - Nice-to-have for organization
   - Estimated: 1 hour total
5. **Testing & Polish**
   - Add contract/unit tests
   - Harden validation (image size, MIME types)
   - End-to-end testing
   - Estimated: 2 hours

---

## 🧪 Running Locally

See [specs/001-recipe-collection/quickstart.md](specs/001-recipe-collection/quickstart.md) for full setup instructions.

**Quick Start (assumes Cosmos emulator + Azurite + Azure Vision configured)**:

```powershell
# Backend
cd scr/backend
dotnet run --project src/RecipeApi
# Listens on http://localhost:5137 (or https://localhost:7137 with the https launch profile)

# Frontend
cd scr/frontend
npm install
npm run dev
# Listens on http://localhost:5173
```

**Tests**:
```powershell
cd scr/backend
dotnet test
# 54 tests passing
```

---

## 📝 Notes

- **OCR Placeholder**: If Azure Vision credentials are not configured, OCR returns placeholder text (for local dev).
- **Single-User MVP**: Cosmos partition key is constant (`"recipe"`) for all documents.
- **Ingredient Parsing**: Heuristic-based (looks for lines starting with `-`, `•`, or digits under "Ingredients" heading).
- **Test Coverage**: Basic integration tests exist; contract/unit tests for US1 endpoints are marked complete in tasks but need more coverage.
- **CORS**: Configured for `http://localhost:5173` (frontend dev server).
- **Troubleshooting upload/save failures**: If frontend shows network errors on upload or save, verify `VITE_API_BASE_URL` matches the backend URL. Default local setup is `http://localhost:5137` (see `scr/frontend/.env.local`/`.env.example`). If you run backend with HTTPS profile, use `https://localhost:7137` and restart the frontend dev server.

---

## 🔗 References

- [Constitution (.specify/memory/constitution.md)](.specify/memory/constitution.md) - governs test-first, API-first, security baseline
- [OpenAPI Contract (specs/001-recipe-collection/contracts/openapi.yaml)](specs/001-recipe-collection/contracts/openapi.yaml)
- [Data Model (specs/001-recipe-collection/data-model.md)](specs/001-recipe-collection/data-model.md)
- [Tasks (specs/001-recipe-collection/tasks.md)](specs/001-recipe-collection/tasks.md)
