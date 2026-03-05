# Research: Upgrade to .NET 10 with C# 12

## Decision 1: Migration sequence and validation order

- **Decision**: Update backend app and test projects to `net10.0` and C# 12 first, then reconcile package compatibility, then validate with `dotnet restore`, `dotnet build`, and `dotnet test` at solution level.
- **Rationale**: Framework/language baseline changes expose true compatibility issues early and prevent hidden drift between app and tests.
- **Alternatives considered**:
  - Package-first migration (rejected: obscures whether failures come from TFM or package changes).
  - Per-project validation only (rejected: misses cross-project restore/build interactions).

## Decision 2: SDK pinning and environment parity

- **Decision**: Pin SDK in repository-root `global.json` to a specific `.NET 10.0.x` and require CI to use that same pinned version.
- **Rationale**: Deterministic SDK selection eliminates local/CI drift and meets constitution + spec requirements.
- **Alternatives considered**:
  - Major/minor-only pinning (`10.0`) (rejected: less reproducible).
  - CI-only pinning (rejected: local developer drift remains).
  - No pinning (rejected: non-deterministic behavior).

## Decision 3: Dependency compatibility policy

- **Decision**: Treat dependency incompatibilities with `net10.0`/C# 12 as release blockers.
- **Rationale**: The spec defines incompatibilities as blocking; migration is incomplete while any build/test dependency is unresolved.
- **Alternatives considered**:
  - Defer incompatibilities to follow-up tickets (rejected: violates blocker policy).
  - Temporary dual targeting (rejected: explicit out-of-scope).
- **Resolution Status**: ✅ All blockers resolved. See `dependency-compatibility.md` for full matrix.
  - `Microsoft.AspNetCore.OpenApi` 9.0.9 → 10.0.3
  - `Microsoft.AspNetCore.Mvc.Testing` 9.0.0 → 10.0.3

## Decision 4: Scope synchronization for tooling and docs

- **Decision**: Update project configuration, CI configuration, VS Code `tasks.json`/`launch.json`, and local dev docs in one migration stream.
- **Rationale**: Avoids partial migration where code compiles but developer workflow fails due to stale paths/commands/framework assumptions.
- **Alternatives considered**:
  - Project-only updates first (rejected: leaves tooling/docs broken).
  - Docs-only or tooling-only updates (rejected: does not satisfy runtime baseline objective).

## Decision 5: Completion verification gates

- **Decision**: Use strict done gates: all backend app/tests target `net10.0` only, C# 12 enforced, pinned `global.json` present, build/test pass, tooling and docs aligned.
- **Rationale**: Maps directly to FR-001..FR-011 and gives objective completion evidence.
- **Alternatives considered**:
  - “Build passes once” only (rejected: insufficient for parity and tooling validation).

## Unknowns Resolution

All Technical Context unknowns are resolved for planning scope. No remaining `NEEDS CLARIFICATION` items block plan completion.
