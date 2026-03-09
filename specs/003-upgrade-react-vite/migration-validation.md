# Migration Validation Log: Frontend Tooling Upgrade

**Feature**: `003-upgrade-react-vite`  
**Date**: 2026-03-09  
**Purpose**: Track fail-first validation evidence, workflow results, and smoke verification throughout the upgrade.

---

## Pre-Upgrade Baseline

### Workflow & Configuration Gaps (T003)

**Captured**: 2026-03-09

**Environment**:
- Node.js: v24.14.0 (note: above Node 22 LTS target — local machine has newer version installed)
- npm: 11.9.0

**Pre-upgrade workflow results** (on current `package.json` dependencies):

| Command | Result | Notes |
|---------|--------|-------|
| `npm run type-check` | ✅ PASS | `tsc --noEmit` completes with zero errors |
| `npm run build` | ✅ PASS | `tsc && vite build` produces dist/ (vite 5.4.21) |
| `npm run lint` | ❌ FAIL | ESLint 8.57.1 cannot find configuration file — no `.eslintrc*` exists in `src/frontend/` |
| `npm run dev` | ✅ PASS (assumed) | Vite dev server starts on port 5173 |

**Configuration gaps identified**:

1. **No ESLint configuration file**: `npm run lint` fails because there is no `.eslintrc*` or `eslint.config.*` file. The ESLint 9 flat config (T007) will close this gap.
2. **No `engines` field in package.json**: No Node version enforcement. T005 will add `engines` with Node 22 LTS requirement.
3. **No `tsBuildInfoFile`**: TypeScript incremental build metadata is not configured. T008/T009 will add this.
4. **No frontend CI workflow**: Only `backend-ci.yml` exists. T022 will create `frontend-ci.yml`.
5. **Legacy ESLint CLI flags**: `npm run lint` uses `--ext ts,tsx` which is ESLint 8-specific. T007 will remove these.
6. **Route mismatch**: `Home.tsx` navigates to `/recipe/${id}` but `App.tsx` defines route as `/recipes/:id`. T015/T016 will fix this.

### Smoke Matrix (T004)

**Captured**: 2026-03-09  
**Source**: FR-015

The following screen-and-interaction matrix must be verified during final smoke testing:

| # | Screen | Interaction | Expected Behavior |
|---|--------|-------------|-------------------|
| 1 | Home | App load | Page renders with recipe list or empty state |
| 2 | Home | Search (keyword/tag) | Results filter based on query |
| 3 | Home | Clear search | Resets to full list |
| 4 | Home | Retry/error states | Error message displays with retry button |
| 5 | Home → Add Recipe | Navigation | Navigates to `/add-recipe` |
| 6 | Add Recipe | OCR/upload | Image upload triggers OCR extraction |
| 7 | Add Recipe | Recipe save | Saves recipe and navigates to detail |
| 8 | Home → Recipe Detail | Navigation | Click recipe card navigates to `/recipes/:id` |
| 9 | Recipe Detail | Detail load | Recipe title, text, ingredients, tags display |
| 10 | Recipe Detail | Add tag | New tag appears in tag list |
| 11 | Recipe Detail | Remove tag | Tag is removed from list |
| 12 | Recipe Detail | Back navigation | Returns to Home |

---

## User Story 1: Keep the Frontend Working

### Pre-Fix Navigation Failure Evidence (T012)

**Captured**: 2026-03-09

**Issue**: Route mismatch between `Home.tsx` navigation and `App.tsx` route definition.

- `App.tsx` defines route: `<Route path="/recipes/:id" element={<RecipeDetail />} />`
- `Home.tsx` navigates: `` navigate(`/recipe/${recipe.id}`) `` (missing trailing "s")
- Result: Clicking a recipe card from the Home page navigates to `/recipe/<id>` which does not match any route, so `RecipeDetail` never renders.

**Evidence location**: [Home.tsx line ~175](../../../src/frontend/src/pages/Home.tsx) — `navigate(\`/recipe/${recipe.id}\`)`  
**Expected fix**: Change to `` navigate(`/recipes/${recipe.id}`) `` in T016.

### Pre-Fix Type-Check/Build Failure Evidence (T013)

**Captured**: 2026-03-09

After upgrading dependencies (React 19.2.4, Vite 7.3.1, TypeScript 5.9.3) but before any code changes:

| Command | Result | Notes |
|---------|--------|-------|
| `npm run type-check` | ✅ PASS | No type-check errors — existing code is compatible with React 19 + TS 5.9 |
| `npm run build` | ✅ PASS | Vite 7.3.1 builds successfully (37 modules, 7.88s) |

**Finding**: The codebase already uses `ReactDOM.createRoot` (React 18+ API), so there are no compile-time failures from the React 19 upgrade. The `main.tsx` import of `React` is only used for `React.StrictMode` JSX, which remains valid.

**Remaining runtime issue**: The route mismatch (T012) is a runtime navigation bug, not a compile-time failure. It existed pre-upgrade but will be fixed as part of US1 compatibility work.

### Post-Fix Validation (T020)

**Validated**: 2026-03-09

| Command | Result | Details |
|---------|--------|---------|
| `npm install` | ✅ PASS | 237 packages, 0 vulnerabilities |
| `npm run type-check` | ✅ PASS | `tsc --noEmit` completes with zero errors |
| `npm run build` | ✅ PASS | Vite 7.3.1, 37 modules, 1.46s build time |

**Changes applied**:
- `main.tsx`: Updated to use direct `{ StrictMode }` and `{ createRoot }` imports (React 19 best practice)
- `Home.tsx`: Fixed route navigation from `/recipe/${id}` to `/recipes/${id}` (matches App.tsx route)
- `App.tsx`: No changes needed (route definition was already correct)
- `AddRecipe.tsx`: No changes needed (already compatible)
- `RecipeDetail.tsx`: No changes needed (already compatible)
- `api.ts`: No changes needed (already compatible)

---

## User Story 2: Preserve Validation Confidence

### Pre-Implementation CI Gap Evidence (T021)

**Captured**: 2026-03-09

**Current CI state**:
- Only `backend-ci.yml` exists in `.github/workflows/`
- No frontend CI workflow exists — frontend changes have zero automated validation in CI
- `backend-ci.yml` triggers on `src/backend/**` paths only — frontend path changes are not tested

**Pre-upgrade lint gap**:
- `npm run lint` was failing pre-upgrade because no `.eslintrc*` configuration file existed
- The ESLint 9 flat config (`eslint.config.mjs`) was created in T007 to close this gap

**Gap to close**: Create `frontend-ci.yml` running npm install, lint, type-check, and build on Node 22 (T022).

### ESLint 9 Flat-Config Validation (T024)

**Validated**: 2026-03-09

| Run | Command | Result | Notes |
|-----|---------|--------|-------|
| 1 | `npm run lint` | ✅ PASS | ESLint 9.39.4 with `eslint.config.mjs` flat config, zero warnings |

- Config file: `src/frontend/eslint.config.mjs`
- Plugins: `typescript-eslint` 8.56.1, `eslint-plugin-react-hooks` 5.2.0, `eslint-plugin-react-refresh` 0.5.2
- Lint script: `eslint . --max-warnings 0` (removed legacy `--ext` flag)

### tsBuildInfoFile Stability Evidence (T025)

**Validated**: 2026-03-09

| Run | Command | Result | tsBuildInfo exists |
|-----|---------|--------|-------------------|
| 1 | `npm run type-check` | ✅ PASS | ✅ `node_modules/.cache/tsbuildinfo/tsconfig.tsbuildinfo` created |
| 2 | `npm run type-check` | ✅ PASS | ✅ File reused without errors |

- `incremental: true` added to `tsconfig.json` to enable build info output with `noEmit: true`
- `tsBuildInfoFile` path: `./node_modules/.cache/tsbuildinfo/tsconfig.tsbuildinfo`
- File is stored inside `node_modules/.cache/` (auto-excluded from git, survives `npm ci`)

### CI-Equivalent Validation Flow (T026)

**Validated**: 2026-03-09

Full CI-equivalent flow executed locally:

| Step | Command | Result | Notes |
|------|---------|--------|-------|
| 1 | `npm ci` | ✅ PASS | 237 packages, 0 vulnerabilities |
| 2 | `npm run lint` | ✅ PASS | ESLint 9 flat config, zero warnings |
| 3 | `npm run type-check` | ✅ PASS | TypeScript 5.9.3, zero errors |
| 4 | `npm run build` | ✅ PASS | Vite 7.3.1, 37 modules, 11.76s |

**No version-only tests introduced**: Confirmed zero new test files or test assertions related to package version numbers. FR-007 satisfied.

---

## User Story 3: Keep Shared Tooling Accurate

### Outdated References Evidence (T027)

**Captured**: 2026-03-09

**Outdated Node/tooling references found**:

| File | Current Reference | Needed Update |
|------|------------------|---------------|
| `specs/001-recipe-collection/quickstart.md` | "Node.js 18+ and npm 9+" | Update to "Node.js 22+" |
| `specs/002-cosmosdb-emulator-setup/quickstart.md` | "Node.js 18+ if running the frontend" | Update to "Node.js 22+" |
| `README.md` | No explicit Node version, but references outdated frontend status | Update frontend status + Node 22 |

**VS Code tooling status**:
- `.vscode/tasks.json`: `start-frontend` task is correct — uses `npm run dev` with Vite background pattern matcher. No changes needed.
- `.vscode/launch.json`: Frontend debug configs reference `localhost:5173` and use `start-frontend` task. Vite 7 default port remains 5173. The `serverReadyAction` pattern `Local:.*(https?://localhost:[0-9]+)` works with Vite 7 output. No changes needed.

**Conclusion**: VS Code tasks/launch configs are already compatible. Documentation updates needed in README.md and two quickstart files.

---

## Smoke Verification

### T034: App Load, Search, Clear Search, Retry/Error States

**Status**: ⏳ PENDING MANUAL VERIFICATION

Requires running backend + frontend locally and interacting in a browser. Automated validation (install, lint, type-check, build) has passed.

| # | Interaction | Expected | Verified |
|---|-------------|----------|----------|
| 1 | App load (Home) | Page renders | ☐ |
| 2 | Search (keyword) | Results filter | ☐ |
| 3 | Search (tag) | Results filter by tag | ☐ |
| 4 | Clear search | Resets to full list | ☐ |
| 5 | Error state | Error message with retry button | ☐ |
| 6 | Retry | Reloads recipes | ☐ |

### T035: Add Recipe Navigation, OCR/Upload, Recipe Save

**Status**: ⏳ PENDING MANUAL VERIFICATION

| # | Interaction | Expected | Verified |
|---|-------------|----------|----------|
| 1 | Navigate to Add Recipe | `/add-recipe` renders | ☐ |
| 2 | Upload image | File picker works, preview shows | ☐ |
| 3 | Extract text (OCR) | Text appears in editable textarea | ☐ |
| 4 | Save recipe | Navigates to `/recipes/:id` | ☐ |

### T036: Recipe Detail Navigation, Detail Load, Add Tag, Remove Tag

**Status**: ⏳ PENDING MANUAL VERIFICATION

| # | Interaction | Expected | Verified |
|---|-------------|----------|----------|
| 1 | Navigate to recipe detail | `/recipes/:id` renders with data | ☐ |
| 2 | Detail load | Title, text, ingredients, tags display | ☐ |
| 3 | Add tag | Tag appears in list | ☐ |
| 4 | Remove tag | Tag disappears from list | ☐ |
| 5 | Back navigation | Returns to Home | ☐ |

### T037: Evergreen Browser & HMR Notes

**Documented**: 2026-03-09

**Evergreen browser support**:
- Vite 7 targets modern browsers by default (`esnext` for dev, `modules` for build)
- No legacy browser polyfills needed — spec requires evergreen only (FR-014)
- Build output uses ES modules natively

**Vite HMR behavior**:
- `vite.config.ts` unchanged — `server.port: 5173`, `server.open: true`
- `@vitejs/plugin-react` 5.1.4 provides React Fast Refresh HMR
- VS Code background task pattern `VITE.*ready in` / `Local:.*http://localhost:[0-9]+` works with Vite 7 output
- HMR verification requires manual testing (modify a component, confirm hot reload in browser)

**Note**: Full HMR verification is part of manual smoke testing.

---

## Summary

| Phase | Status | Date |
|-------|--------|------|
| Pre-Upgrade Baseline | ✅ Complete | 2026-03-09 |
| US1 Validation | ✅ Complete | 2026-03-09 |
| US2 Validation | ✅ Complete | 2026-03-09 |
| US3 Validation | ✅ Complete | 2026-03-09 |
| Smoke Verification | ⏳ Pending manual | 2026-03-09 |
