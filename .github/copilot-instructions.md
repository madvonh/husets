## Purpose
This file gives focused, actionable guidance for AI coding agents (Copilot/GitHub Assistants) working in this repository.

## High-level architecture & why it matters
- The repo follows a "spec-driven" workflow powered by `.specify/*` artifacts. Key artifacts live under `.specify/memory/` and `.specify/templates/`.
- Work is organized around a feature `FEATURE_DIR` with 1) `plan.md` (architecture & tech stack), 2) `tasks.md` (implementation steps), 3) optional `data-model.md`, `research.md`, `quickstart.md`, and `contracts/` for API specs.
- The workflow enforces Test-First and checklists-based gating (see `CONSTITUTION` template in `.specify/memory/constitution.md`). Follow the TDD/task ordering in `tasks.md`.

## Key files to read first (examples)
- `.specify/memory/constitution.md` — governance, versioning rules, core principles (use for policy decisions).
- `.specify/scripts/powershell/check-prerequisites.ps1` — canonical command to discover `FEATURE_DIR` and available docs. Use `-Json -RequireTasks -IncludeTasks` to get machine-parsable context.
- `plan.md`, `tasks.md` inside the active feature directory — they drive what to implement and how to sequence work.
- `contracts/` — contains API contracts; tests and implementations must satisfy these.
- `.github/agents/` and `.github/prompts/` — existing agent templates you should follow or reuse.

## Concrete workflows & commands
- To discover current feature paths and docs (machine friendly):

  ```powershell
  .\.specify\scripts\powershell\check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
  ```

- If a change affects governance or principles, update `.specify/memory/constitution.md` following the version-bump rules in the constitution template.
- Before implementing, ensure checklists in `FEATURE_DIR/checklists/` are complete (agents commonly halt if checklists report incomplete items).

## Project-specific conventions (do not assume defaults)
- Test-First: tasks and `tasks.md` expect tests to be authored or run before implementation tasks. Respect the red-green-refactor cycle.
- Task parallelism: tasks marked with `[P]` in `tasks.md` may be executed concurrently; tasks that touch the same files should be run sequentially.
- Checklists: the `speckit.implement` agent expects checklist files to use `- [ ]` and `- [x]` markers; count completeness before proceeding.
- Ignore files: `.specify` scripts may create/validate `.gitignore`/`.dockerignore` based on `plan.md`; prefer using the project's ignore-generation logic rather than ad-hoc additions.

## Integration points & external dependencies
- External contracts: `contracts/` directory (if present) are authoritative; generate or run contract tests before merging breaking changes.
- Scripts: use `.specify/scripts/*` helpers (PowerShell) for environment detection and standard operations — they centralize path resolution and validation.

## How to modify the repo (agent behavior rules)
- When editing files referenced by spec templates (e.g., constitution, plan, tasks), preserve template structure and headings. If placeholders remain, add `TODO(<FIELD>): reason` and list them in the sync report.
- Provide a short Sync Impact Report when changing constitution/plan: version change, files updated, files requiring manual follow-up.
- Suggested commit message format for docs/governance changes: `docs: amend constitution to vX.Y.Z (<short reason>)`.

## Examples from this repo (patterns to follow)
- Use `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` to obtain `FEATURE_DIR` and `AVAILABLE_DOCS` before performing implementations.
- Read and respect `.specify/memory/constitution.md` for versioning and TDD requirements.
- Reuse prompts/agents under `.github/agents/` and `.github/prompts/` when forming multi-step responses or followup checks.

## When to ask for human clarification
- If `check-prerequisites` fails (missing `plan.md` or `tasks.md`), stop and ask whether to initialize the feature scaffolding.
- If constitution version bump semantics are ambiguous (major vs minor vs patch), propose a rationale and ask the maintainer to choose.

## Quick checklist for agents
- Run prerequisite script to collect feature paths.
- Open `plan.md` and `tasks.md` in `FEATURE_DIR` and follow their ordering.
- Validate checklists; if incomplete, ask user whether to continue.
- Run/author tests first for any functional change.
- Update constitution/templates only with a Sync Impact Report and a suggested commit message.

---
If anything above is unclear or you'd like a different level of strictness (e.g., auto-apply ignore changes vs. propose them), tell me what to change.
