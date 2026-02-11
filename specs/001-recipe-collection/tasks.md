# Tasks: Recipe Collection (Photo Upload + OCR + Tag Search)

**Input**: Design documents in `specs/001-recipe-collection/` (`plan.md`, `spec.md`)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps to user stories in `spec.md` (US1, US2, US3)
- **Test-first is required** by `.specify/memory/constitution.md`: write tests (or contract tests) first, see them fail, then implement.

---

## Phase 0: Spec Artifacts (Contracts + Data Model)

**Purpose**: Lock the contract and data model before writing production code.

- [x] T001 [P] Create API contract file `specs/001-recipe-collection/contracts/openapi.yaml` (health, OCR, recipes CRUD-lite, tags, search)
- [x] T002 [P] Create `specs/001-recipe-collection/data-model.md` describing Cosmos documents, partition key (`pk` constant for MVP), and indexing assumptions for search
- [x] T003 [P] Create `specs/001-recipe-collection/quickstart.md` (local dev steps for backend+frontend, Cosmos emulator/docker, required env vars)

**Checkpoint**: OpenAPI + data model exist and are internally consistent with `spec.md`.

---

## Phase 1: Repo Scaffolding (Backend + Frontend)

**Purpose**: Create the project structure declared in `plan.md`.

- [x] T010 Create backend solution structure under `backend/` (API project + test project)
- [x] T011 [P] Create frontend scaffold under `frontend/` (Vite + React + TS + Tailwind) and confirm mobile-responsive baseline
- [x] T012 [P] Add root-level developer docs pointers (update `specs/001-recipe-collection/quickstart.md` if needed)

**Checkpoint**: `backend` builds/tests run; `frontend` builds and runs with a configurable API base URL.

---

## Phase 2: Foundational Backend Infrastructure (Blocks All Stories)

**Purpose**: Establish cross-cutting requirements: validation, errors, logs, correlation IDs, config, and Cosmos access patterns.

### Tests (write first)

- [x] T020 [P] Add backend tests for consistent error shape + correlation id behavior (e.g., missing required field returns `{ code, message }` and echoes/creates correlation id)
- [x] T021 [P] Add backend tests for input validation on representative endpoints (400 on invalid payloads)

### Implementation

- [x] T022 Add request correlation ID middleware to `backend/src/*` (propagate from header, generate if missing, include in logs/response)
- [x] T023 Add global exception handling producing consistent JSON error responses
- [x] T024 Add input validation pipeline (FluentValidation) and wire into Minimal API endpoints
- [x] T025 Add `GET /health` endpoint (and test) including Cosmos connectivity check
- [x] T026 Implement Cosmos client setup + resilience (bounded retries for 429) behind a small data access layer
- [x] T027 Define core domain models/DTOs shared across stories (Recipe, Tag, Ingredient, RecipeIngredient, etc.) in `backend/src/models/`

**Checkpoint**: Health, errors, validation, logs, and Cosmos wiring are in place.

---

## Phase 3: User Story 1 (P1) — Add Recipe From Photo (Upload → OCR → Review/Edit → Save)

**Goal**: User can upload an image, see OCR text, edit, and save a recipe with ingredients stored as separate items, plus an `imageRef` stored.

### Contract + Tests (write first; ensure failing)

- [x] T030 [US1] Add contract tests for `POST /ocr` (multipart upload) matching `specs/001-recipe-collection/contracts/openapi.yaml`
- [x] T031 [US1] Add contract tests for `POST /recipes` (create recipe with edited text, tags optional, `imageRef` required)
- [x] T032 [US1] Add unit tests for ingredient parsing (extract ingredient lines, preserve `freeText`, stable ordering/position)
- [x] T033 [US1] Add unit tests for best-effort ingredient normalization into `Ingredient` (case/whitespace normalization; ensure `freeText` unchanged)

### Backend implementation

- [x] T034 [US1] Implement Blob storage adapter (Azure Blob) + configuration via env vars (no secrets in frontend/repo)
- [x] T035 [US1] Implement Azure AI Vision Read OCR client adapter behind an interface so it can be faked in tests
- [x] T036 [US1] Implement `POST /ocr` endpoint: validate upload, store original image in Blob, run OCR, return extracted text + `imageRef`
- [x] T037 [US1] Implement `POST /recipes` endpoint: validate payload, persist Recipe document, persist each `RecipeIngredient` as a separate item linked to recipe, persist/lookup normalized `Ingredient` items best-effort
- [x] T038 [US1] Implement `GET /recipes/{id}` endpoint returning recipe + ingredients + tags + `imageRef`

### Frontend tests (minimal) + implementation

- [x] T039 [US1] Add minimal frontend smoke test(s) (or typecheck/build gate if tests aren't set up) for the add-recipe flow
- [x] T040 [US1] Implement Add Recipe page in `frontend/src/pages/`:
  - image upload
  - call `POST /ocr`
  - editable text area for OCR result
  - call `POST /recipes`
  - handle error states using API error `code/message`
- [x] T041 [US1] Ensure layout is usable on mobile widths (responsive form controls; no horizontal scrolling)

**Checkpoint**: End-to-end US1 works with real image upload and recipe persists with separate ingredient items.

---

## Phase 4: User Story 2 (P2) — Search Recipes On First Page (Text + Tags)

**Goal**: First page lets user search by free text and tags, returning a list; user can open a recipe.

### Contract + Tests (write first)

- [x] T050 [US2] Add contract tests for `GET /recipes?query=&tag=` (text contains + tag filter)
- [x] T051 [US2] Add backend integration/unit tests for search query normalization (trim, case-insensitive for tags)

### Backend implementation

- [x] T052 [US2] Implement `GET /recipes` endpoint supporting:
  - `query` (MVP contains search over stored recipe searchable fields)
  - `tag` (filter by normalized tag)
- [x] T053 [US2] Ensure Cosmos indexing/queries are compatible with the chosen partition strategy (constant `pk`) and remain MVP-correct

### Frontend implementation

- [x] T054 [US2] Implement Home page in `frontend/src/pages/` with:
  - search input
  - tag filter input
  - results list
  - navigation to recipe detail
- [x] T055 [US2] Implement Recipe details page (render ingredients list + tags + image preview/reference)
- [x] T056 [US2] Mobile usability check for search + results list

**Checkpoint**: Searching by keyword/tag returns expected recipes and recipe details open correctly.

---

## Phase 5: User Story 3 (P3) — Tag Management (Add/Remove)

**Goal**: User can add/remove tags on a recipe; tags are normalized and affect search.

### Contract + Tests (write first)

- [x] T060 [US3] Add contract tests for tag add/remove endpoints (e.g., `POST /recipes/{id}/tags`, `DELETE /recipes/{id}/tags/{tag}`)
- [x] T061 [US3] Add unit tests for tag normalization rules (trim whitespace, case-insensitive canonical form)

### Backend implementation

- [x] T062 [US3] Implement tag persistence model per `data-model.md` (Tag + RecipeTag and/or denormalized `normalizedTags` on Recipe)
- [x] T063 [US3] Implement endpoints to add/remove tags; ensure idempotency where practical
- [x] T064 [US3] Ensure `GET /recipes` tag filtering reflects changes immediately

### Frontend implementation

- [x] T065 [US3] Add tag UI on Recipe details page: add tag input + remove buttons
- [x] T066 [US3] Validate mobile layout for tag chips/buttons

**Checkpoint**: Tags can be managed on a recipe and used in search.

---

## Phase 6: Cross-Cutting Polish (Only After US1–US3)

- [x] T070 Add rate-limit/transient failure handling where needed (bounded retries; user-friendly errors)
- [x] T071 Tighten validation rules (max image size, allowed mime types, max tag length, etc.) and update tests
- [x] T072 Ensure all required env vars are documented in `specs/001-recipe-collection/quickstart.md`
- [x] T073 Verify `frontend` build produces static assets suitable for deployment and uses only public config
