# Quickstart: Frontend Tooling Upgrade Verification

**Feature**: `003-upgrade-react-vite`  
**Date**: 2026-03-09

This guide covers verifying the frontend tooling upgrade locally.

## Prerequisites

- **Node.js** 22 LTS (`node --version` should show v22.x or higher)
- **npm** (bundled with Node 22)
- **Git** (to clone/checkout the branch)

## Step 1: Checkout the Branch

```powershell
git checkout 003-upgrade-react-vite
```

## Step 2: Install Dependencies

```powershell
cd src/frontend
npm ci
```

Expected: 237 packages installed, 0 vulnerabilities.

## Step 3: Run Lint

```powershell
npm run lint
```

Expected: ESLint 9 with flat config (`eslint.config.mjs`) passes with zero warnings.

## Step 4: Run Type-Check

```powershell
npm run type-check
```

Expected: TypeScript 5.9 `tsc --noEmit` passes with zero errors. A `tsconfig.tsbuildinfo` file is created in `node_modules/.cache/tsbuildinfo/`.

## Step 5: Run Build

```powershell
npm run build
```

Expected: `tsc && vite build` completes successfully. Vite 7 produces output in `dist/`.

## Step 6: Start Dev Server

```powershell
npm run dev
```

Expected: Vite dev server starts on `http://localhost:5173` with HMR enabled. Open the URL in a browser — the app should load the Home page.

## Step 7: Verify Key Interactions

With the backend running (see [001-recipe-collection quickstart](../001-recipe-collection/quickstart.md)):

1. **Home page**: Loads with recipe list or empty state
2. **Search**: Type a keyword and click Search — results filter
3. **Clear search**: Click Clear — resets to full list
4. **Add Recipe**: Click "Add Recipe" button — navigates to `/add-recipe`
5. **Recipe Detail**: Click a recipe card — navigates to `/recipes/:id` and displays detail

## Upgraded Stack Summary

| Tool | Version |
|------|---------|
| React | 19.2.x |
| React DOM | 19.2.x |
| React Router DOM | 6.30.x |
| Vite | 7.3.x |
| TypeScript | 5.9.x |
| ESLint | 9.39.x (flat config) |
| Tailwind CSS | 3.4.x |
| Node.js | 22 LTS |
