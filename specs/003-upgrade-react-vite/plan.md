# Implementation Plan: Frontend Tooling Upgrade

**Branch**: `003-upgrade-react-vite` | **Date**: 2026-03-09 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-upgrade-react-vite/spec.md`

## Summary

Upgrade the frontend baseline to React 19.2+, React DOM 19.2+, Vite 7+, TypeScript 5.9+, and ESLint 9+ flat config while preserving the existing SPA behavior and aligning with the frontend governance in `.specify/memory/constitution.md`. The work is primarily a dependency and tooling migration across the Vite app, TypeScript configuration, npm workflow, VS Code tasks, CI validation, and contributor docs, plus compatibility fixes required to keep all current screens and interactions working.

## Technical Context

**Language/Version**: TypeScript 5.9+ with React 19.2+ frontend; .NET 10 backend unchanged  
**Primary Dependencies**: React, React DOM, React Router, Vite, `@vitejs/plugin-react`, ESLint flat config stack, Tailwind CSS, PostCSS  
**Storage**: No frontend-owned storage; browser talks only to backend API  
**Testing**: Fail-first validation evidence for changed behavior/configuration, existing frontend validation workflows (`npm` install, lint, type-check, build), frontend CI execution, and manual smoke verification of all current screens/interactions  
**Target Platform**: Static frontend hosted from built assets; local dev on Node 22 LTS with Vite dev server + HMR  
**Project Type**: Web application (`src/frontend` Vite SPA plus existing backend API)  
**Performance Goals**: No regression in local dev/build workflows; stable incremental TypeScript validation with configured `tsBuildInfoFile`  
**Constraints**: npm only; Node 22 LTS for local and CI; ESLint 9+ flat config; Vite dev server with HMR; no version-only tests; preserve evergreen browser support; keep existing screens/interactions working  
**Scale/Scope**: Focused frontend runtime/toolchain migration touching package management, lint/build config, CI, workspace tooling, and docs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Pre-Phase 0 gate evaluation:

- ✅ Static-first frontend preserved: output remains static assets built from `src/frontend`.
- ✅ API-first boundary preserved: no browser-to-database access is introduced.
- ✅ Test-first intent preserved: fail-first validation evidence must be captured before compatibility fixes for changed behavior or workflow expectations.
- ✅ Security baseline preserved: no frontend secrets or privileged config added.
- ✅ Frontend platform requirements explicitly targeted:
  - ESLint 9+ flat config.
  - Vite dev server with HMR.
  - TypeScript 5.9+ with configured `tsBuildInfoFile`.

Post-Phase 1 design re-check:

- ✅ Planned changes remain within frontend governance and do not require constitution exceptions.
- ✅ Required workflow updates include local tooling, CI, and documentation alignment.

## Project Structure

### Documentation (this feature)

```text
specs/003-upgrade-react-vite/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── migration-validation.md
├── dependency-compatibility.md
├── quickstart.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── frontend/
│   ├── package.json
│   ├── package-lock.json
│   ├── tsconfig.json
│   ├── tsconfig.node.json
│   ├── vite.config.ts
│   ├── .gitignore
│   └── src/
│       ├── main.tsx
│       ├── App.tsx
│       ├── pages/
│       │   ├── Home.tsx
│       │   ├── AddRecipe.tsx
│       │   └── RecipeDetail.tsx
│       └── services/api.ts
├── backend/
│   └── ...

.vscode/
├── tasks.json
└── launch.json

.github/
└── workflows/
```

**Structure Decision**: Keep the existing split architecture and concentrate changes in `src/frontend`, shared VS Code tooling, frontend CI, and contributor documentation.

## Phase Plan

### Phase 0 — Research

- Confirm exact dependency compatibility set for React 19.2+, Vite 7+, TypeScript 5.9+, and ESLint 9 flat config.
- Confirm Node 22 and npm enforcement approach through `package.json` `engines`, CI setup, and existing workflow expectations.
- Confirm current screen/interactions that must be preserved during smoke verification.
- Confirm any compatibility fixes required by the current app, including route mismatches that would invalidate smoke checks.

Output: planning-ready dependency and compatibility baseline.

### Phase 1 — Design & Workflow Alignment

- Define updated package, lint, build, and incremental TypeScript configuration targets.
- Define flat-config lint structure and script expectations.
- Define CI validation flow for frontend install, lint, type-check, and build on Node 22.
- Define documentation/workspace updates needed to keep local onboarding and debug flows aligned.

Output: implementation-ready design decisions captured in this plan and referenced by future tasks.

### Phase 2 — Task Planning Hand-off

- Break work into fail-first validation and implementation tasks for dependency updates, config migration, compatibility fixes, CI, and docs.
- Sequence validation-sensitive work so lint/build/type-check and smoke verification remain explicit completion gates.

Output: ready for `/speckit.tasks`.

## Implementation Outline

1. Capture fail-first validation evidence for current workflow gaps and known behavior mismatches in `specs/003-upgrade-react-vite/migration-validation.md`.
2. Update `src/frontend/package.json` and `src/frontend/package-lock.json` to the requested runtime/tooling baseline, including Node 22 `engines` and npm-authoritative workflow.
3. Introduce an ESLint 9 flat config file under `src/frontend` and revise lint scripts to remove legacy CLI assumptions.
4. Configure `tsBuildInfoFile` in `src/frontend/tsconfig.json` and `src/frontend/tsconfig.node.json`, then align scripts and ignore rules for stable incremental validation.
5. Verify `src/frontend/vite.config.ts` and local dev workflows preserve Vite HMR behavior and current port assumptions.
6. Validate and fix compatibility issues in `src/frontend/src/main.tsx`, `src/frontend/src/App.tsx`, `src/frontend/src/pages/Home.tsx`, `src/frontend/src/pages/AddRecipe.tsx`, `src/frontend/src/pages/RecipeDetail.tsx`, and `src/frontend/src/services/api.ts` as needed to preserve all existing screens/interactions.
7. Add frontend CI under `.github/workflows/` for Node 22 npm install, lint, type-check, and build.
8. Update `.vscode/tasks.json`, `.vscode/launch.json`, `README.md`, and relevant quickstart docs to match the upgraded frontend workflow.
9. Finish with full smoke verification across all existing frontend screens and required interactions on supported evergreen browsers.

## Verification Strategy

- Install dependencies with npm using the committed lockfile on Node 22 LTS.
- Run frontend validation commands: `npm run lint`, `npm run type-check`, `npm run build`, and `npm run dev`.
- Confirm repeated TypeScript validation runs reuse the configured `tsBuildInfoFile` without blocking errors.
- Verify VS Code task/debug workflows continue to work against the Vite dev server.
- Execute manual smoke verification across Home, Add Recipe, and Recipe Detail flows, including navigation, loading/error states, OCR/upload flow, save flow, and tag interactions.
- Verify frontend CI passes independently of backend CI.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
