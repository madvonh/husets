# Implementation Plan: Recipe Collection (Photo Upload + OCR + Tag Search)

**Branch**: `001-recipe-collection` | **Date**: 2026-02-10 | **Spec**: ./spec.md
**Input**: Feature specification from `/specs/001-recipe-collection/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a static frontend that lets users upload a recipe photo, review OCR-extracted text, save recipes with structured ingredients, tag recipes, and search from the first page by text and tags.

Minimal architecture:
- Static frontend calls a backend HTTP API.
- Backend API runs on Azure App Service and is the only component that talks to Cosmos DB.
- OCR is performed server-side via an OCR provider (TBD) and the original image is stored and referenced by the recipe.

## Technical Context

**Language/Version**:  .Net c# for API; React with Vite and TypeScript for frontend  
**Primary Dependencies**:  minimal API; minimal frontend framework/build tool  
**Storage**: Azure Cosmos DB (API-managed access only) using a docker image locally for development
**Testing**: must support test-first workflow; contract tests with fluent validation for API changes 
**Target Platform**: Azure Static Hosting for frontend + Azure App Service for API + Azure Cosmos DB
**Project Type**: web  
**Performance Goals**: MVP-grade; prioritize correctness and usability over throughput  
**Constraints**: No secrets in frontend; HTTPS only; API validates inputs; Cosmos partition keys required  
**Scale/Scope**: Small personal collection (assume low concurrency; optimize later if needed)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Gates from `.specify/memory/constitution.md`:
- Static-first frontend: deliverable is static assets; no secrets in client.
- API-first backend: all data access via API; HTTPS only.
- Test-first: tests (or contract tests) written before implementation.
- Cosmos DB: accessed only by API; partition key defined; handle 429 with bounded retries.
- Observability/security: structured logs + correlation IDs; input validation.

## Project Structure

### Documentation (this feature)

```text
specs/001-recipe-collection/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
```text
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/
```

**Structure Decision**: Web application with separate `frontend/` (static web app) and `backend/` (App Service API).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
