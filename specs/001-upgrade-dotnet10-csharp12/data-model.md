# Data Model: Platform Upgrade Configuration

## Overview

This feature does not introduce business-domain entities or API payload changes. The model below describes configuration artifacts and validation relationships required for migration governance.

## Entities

### 1) PlatformBaseline

- **Purpose**: Defines required backend runtime and language baseline.
- **Fields**:
  - `targetFramework` (string, required): must equal `net10.0`
  - `languageVersion` (string, required): must equal `12.0` or `12`
  - `sdkVersion` (string, required): must match `.NET 10.0.x` semantic format (e.g., `10.0.103`)
  - `dualTargetingAllowed` (boolean, required): must be `false`
- **Validation Rules**:
  - All backend application and test project files must resolve to this baseline.
  - Any value outside `net10.0` for targeted backend projects is invalid.

### 2) BackendProjectConfig

- **Purpose**: Represents each backend project participating in build/test workflows.
- **Fields**:
  - `projectPath` (string, required)
  - `projectType` (enum, required): `application | test`
  - `targetFramework` (string, required)
  - `languageVersion` (string, optional when inherited)
  - `packageReferences` (array, required)
- **Validation Rules**:
  - `targetFramework` must equal `PlatformBaseline.targetFramework`.
  - If `languageVersion` is set, it must align with `PlatformBaseline.languageVersion`.
  - Any incompatible package in build/test path blocks completion.

### 3) SdkPinningConfig

- **Purpose**: Controls deterministic SDK selection.
- **Fields**:
  - `filePath` (string, required): repository-root `global.json`
  - `sdk.version` (string, required): specific `.NET 10.0.x`
  - `rollForward` (string, optional): constrained behavior consistent with policy
- **Validation Rules**:
  - `global.json` must exist and declare a specific `.NET 10.0.x` SDK.
  - CI must use the same SDK version as `global.json`.

### 4) ValidationWorkflow

- **Purpose**: Defines executable migration verification flow.
- **Fields**:
  - `restoreCommand` (string, required)
  - `buildCommand` (string, required)
  - `testCommand` (string, required)
  - `ciParityCheck` (boolean, required)
  - `workspaceToolingAligned` (boolean, required)
- **Validation Rules**:
  - Restore/build/test must succeed after migration.
  - Workspace tasks and launch settings must reference valid paths and baseline outputs.
  - Documentation commands must match actual repository paths.

## Relationships

- `PlatformBaseline` 1→N `BackendProjectConfig`
- `PlatformBaseline` 1→1 `SdkPinningConfig`
- `ValidationWorkflow` validates both `BackendProjectConfig` and `SdkPinningConfig`

## State Transitions

### MigrationState

1. **Current**: backend projects at legacy baseline (`net9.0`) and no pinned SDK contract.
2. **In Migration**: project files and package compatibility being aligned to baseline.
3. **Validated**: restore/build/test succeed and CI parity is confirmed.
4. **Complete**: docs/tooling aligned and all blocker incompatibilities resolved.

Invalid transition:
- `In Migration` → `Complete` while dependency incompatibilities remain open.
