# Feature Specification: Recipe Collection (Photo Upload + OCR + Tag Search)

**Feature Branch**: `001-recipe-collection`  
**Created**: 2026-02-10  
**Status**: Draft  
**Input**: User description: "I am building a recipe collection. I want to be able to upload recipes as a photo with the text in it. On the first page there is a search function. I want to be able to tag them and search on those tags. I want the ingrediens to be saved in the database as separate items."

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Add Recipe From Photo (Priority: P1)

As a user, I can upload a photo of a recipe (with text in the image), review the extracted text (OCR), and save it as a recipe where ingredients are stored as separate items in the database.

**Why this priority**: This is the primary input method and unlocks all later functionality (tags/search) by creating real data.

**Independent Test**: Upload a single image, confirm OCR text is captured, confirm a recipe is created, and confirm ingredients are persisted as separate items linked to the recipe.

**Acceptance Scenarios**:

1. **Given** I am on the add-recipe flow, **When** I upload a recipe photo, **Then** the app shows extracted text for review and allows saving.
2. **Given** extracted text contains an ingredients section, **When** I save the recipe, **Then** the recipe is stored and ingredients are stored as separate items linked to that recipe.
3. **Given** OCR returns low confidence or partial text, **When** I edit the text before saving, **Then** the saved recipe reflects my edited content.

---

### User Story 2 - Search Recipes On First Page (Text + Tags) (Priority: P2)

As a user, the first page lets me search recipes by free text and by tags, and I can open a matching recipe.

**Why this priority**: Search is the primary way to retrieve value from the collection after recipes exist.

**Independent Test**: With at least two recipes saved (from US1 or seeded), searching by a tag or keyword returns the expected recipe list.

**Acceptance Scenarios**:

1. **Given** recipes exist with tags, **When** I search by a tag, **Then** only recipes with that tag are returned.
2. **Given** recipes exist with titles and body text, **When** I search by keyword, **Then** relevant recipes are returned.
3. **Given** I have a list of search results, **When** I select a recipe, **Then** I can view its details.

---

### User Story 3 - Tag Management (Priority: P3)

As a user, I can add and remove tags on a recipe so that I can organize recipes and improve search.

**Why this priority**: Tags improve organization and search precision but depend on having recipes.

**Independent Test**: Create a recipe, add two tags, verify they appear in search filters and search results; remove a tag and verify it no longer affects search.

**Acceptance Scenarios**:

1. **Given** a recipe exists, **When** I add a tag to it, **Then** the recipe shows that tag and can be found by searching for it.
2. **Given** a recipe has a tag, **When** I remove the tag, **Then** the recipe no longer appears for tag-only searches of that tag.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- OCR returns no text or unreadable output (blurry/rotated photo).
- Large image uploads (size limits, slow networks, retries).
- Duplicate recipe uploads (same photo or near-identical extracted text).
- Ingredients parsing ambiguity (e.g., "1 can tomatoes" vs "1 tomato"); user edits should override.
- Tag normalization (case/whitespace), duplicate tags, empty tags.
- Search with no matches.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow a user to create a recipe by uploading a photo.
- **FR-002**: System MUST extract text from the uploaded photo using OCR.
- **FR-003**: System MUST allow the user to review and edit extracted text before saving.
- **FR-004**: System MUST persist recipes in a database.
- **FR-005**: System MUST persist ingredients as separate items (not just a single text blob) and link them to the owning recipe.
- **FR-006**: System MUST allow a user to tag a recipe (add/remove tags).
- **FR-007**: System MUST support searching recipes from the first page by free-text query.
- **FR-008**: System MUST support searching/filtering recipes by tag.
- **FR-009**: System MUST store a reference to the original uploaded image (or a stored copy) for the recipe.

- **FR-010**: System MUST support a single-user mode with no explicit login (MVP assumption).
- **FR-011**: System MUST perform OCR using Azure AI Vision (Read OCR) (MVP assumption).
- **FR-012**: System MUST store each ingredient line as a `RecipeIngredient` item with `freeText` preserved exactly as entered.
- **FR-013**: System MUST attempt to normalize ingredient names into an `Ingredient` entity (best-effort) and link via `RecipeIngredient`.
- **FR-014**: System MUST normalize tags by trimming whitespace and treating tags case-insensitively (e.g., storing a canonical lowercase form).

### Key Entities *(include if feature involves data)*

- **Recipe**: A saved recipe (title, raw/extracted text, optional notes, createdAt, imageRef).
- **Ingredient**: A normalized ingredient item (name/canonicalName).
- **RecipeIngredient**: Join entity linking a Recipe to Ingredient, with quantity/unit/freeText/position.
- **Tag**: A normalized tag (name).
- **RecipeTag**: Join entity linking a Recipe to Tag.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can upload a recipe photo and save a recipe end-to-end.
- **SC-002**: For a saved recipe, ingredients can be retrieved as structured items (at least name + freeText).
- **SC-003**: Searching by a known tag returns expected recipes.
- **SC-004**: Searching by a known keyword returns expected recipes.
