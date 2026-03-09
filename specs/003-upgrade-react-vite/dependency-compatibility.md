# Dependency Compatibility Tracker: Frontend Tooling Upgrade

**Feature**: `003-upgrade-react-vite`  
**Date**: 2026-03-09  
**Purpose**: Track resolved dependency versions, compatibility decisions, and any known issues across the upgraded frontend baseline.

---

## Target Baseline

| Package | Current Version | Target Version | Resolved | Status |
|---------|----------------|---------------|----------|--------|
| react | ^18.2.0 | ^19.2.0 | 19.2.4 | ✅ Resolved |
| react-dom | ^18.2.0 | ^19.2.0 | 19.2.4 | ✅ Resolved |
| react-router-dom | ^6.22.0 | ^6.30.0 | 6.30.3 | ✅ Resolved (kept on v6) |
| vite | ^5.1.4 | ^7.3.0 | 7.3.1 | ✅ Resolved |
| typescript | ^5.2.2 | ^5.9.0 | 5.9.3 | ✅ Resolved |
| eslint | ^8.56.0 | ^9.39.0 | 9.39.4 | ✅ Resolved |
| @vitejs/plugin-react | ^4.2.1 | ^5.1.0 | 5.1.4 | ✅ Resolved |
| @types/react | ^18.2.56 | ^19.2.0 | 19.2.14 | ✅ Resolved |
| @types/react-dom | ^18.2.19 | ^19.2.0 | 19.2.3 | ✅ Resolved |
| typescript-eslint | (new) | ^8.56.0 | 8.56.1 | ✅ Added |
| globals | (new) | ^17.4.0 | 17.4.0 | ✅ Added |
| eslint-plugin-react-hooks | ^4.6.0 | ^5.2.0 | 5.2.0 | ✅ Resolved |
| eslint-plugin-react-refresh | ^0.4.5 | ^0.5.0 | 0.5.2 | ✅ Resolved |
| tailwindcss | ^3.4.1 | ^3.4.19 | 3.4.19 | ✅ Resolved (kept on v3) |
| postcss | ^8.4.35 | ^8.5.0 | 8.5.8 | ✅ Resolved |
| autoprefixer | ^10.4.17 | ^10.4.27 | 10.4.27 | ✅ Resolved |

### Removed Packages

| Package | Former Version | Reason |
|---------|---------------|--------|
| @typescript-eslint/eslint-plugin | ^6.21.0 | Replaced by `typescript-eslint` unified package |
| @typescript-eslint/parser | ^6.21.0 | Replaced by `typescript-eslint` unified package |

## Runtime & Tooling

| Tool | Current | Target | Resolved | Status |
|------|---------|--------|----------|--------|
| Node.js | 18+ | 22 LTS | >=22.0.0 (engines) | ✅ Configured |
| npm | 9+ | (bundled with Node 22) | 11.9.0 (local) | ✅ Compatible |
| ESLint config | .eslintrc (legacy) | eslint.config.mjs (flat) | Created | ✅ Configured |
| tsBuildInfoFile | Not configured | tsconfig.json + tsconfig.node.json | Configured | ✅ Done |

---

## Compatibility Decisions (T011/T023)

**Documented**: 2026-03-09

1. **react-router-dom kept at v6**: v7 introduces significant breaking API changes (data routers, loaders/actions). Upgrading to v7 is out of scope for this tooling migration. Updated to latest 6.x (6.30.3).
2. **tailwindcss kept at v3**: Tailwind CSS v4 uses a new CSS-first configuration approach. Staying on v3 avoids configuration rewrite. Updated to latest 3.x (3.4.19).
3. **ESLint unified typescript-eslint package**: Replaced separate `@typescript-eslint/eslint-plugin` and `@typescript-eslint/parser` with the unified `typescript-eslint` package, which is the recommended approach for ESLint 9 flat config.
4. **ESLint 9.x (not 10.x)**: ESLint 10 is available but ecosystem support (plugins) is better validated against ESLint 9. Using 9.39.4.
5. **eslint-plugin-react-hooks 5.x**: v5 is the React 19 + ESLint 9 compatible version. v7 targets ESLint 10.
6. **Lint script simplified**: Removed `--ext ts,tsx` and `--report-unused-disable-directives` flags which are ESLint 8-specific. ESLint 9 flat config handles file filtering through the config's `files` array.
7. **globals package added**: Required for ESLint 9 flat config to define browser globals (replaces `env: { browser: true }`).
8. **npm install zero vulnerabilities**: Clean install resolves 237 packages with 0 vulnerabilities.

---

## Known Issues & Workarounds

1. **No pre-existing ESLint config**: The project had no `.eslintrc*` file before the upgrade. The ESLint 9 flat config (`eslint.config.mjs`) was created as part of T007, making `npm run lint` functional for the first time.
2. **Route mismatch (pre-existing bug)**: `Home.tsx` navigated to `/recipe/${id}` instead of `/recipes/${id}`. Fixed in T016 as a compatibility fix to make smoke verification pass.
3. **react-router-dom v6 to v7**: Not upgraded — v7 introduces breaking API changes (data routers). Kept at latest v6 (6.30.3) which is fully compatible with React 19.
4. **tailwindcss v3 to v4**: Not upgraded — v4 uses CSS-first config and is not a drop-in replacement. Kept at latest v3 (3.4.19).

---

## Release Blockers

**Status**: No release blockers identified.

All dependencies resolved cleanly. Automated validation (install, lint, type-check, build) passes. Manual smoke verification of all screens/interactions is the final remaining gate before merge (T034-T036).
