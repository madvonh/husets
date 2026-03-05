<#
.SYNOPSIS
    Validates that the installed .NET SDK matches global.json and that backend projects
    target the expected framework.

.DESCRIPTION
    Checks:
    1. global.json exists and declares a specific .NET 10.0.x SDK version.
    2. Installed SDK version matches global.json.
    3. Backend project files target net10.0.
    4. Directory.Build.props enforces C# 12.

.PARAMETER RepoRoot
    Path to the repository root. Defaults to the script's grandparent directory.

.EXAMPLE
    .\scripts\powershell\assert-sdk-parity.ps1
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'
$failures = @()

Write-Host "=== SDK Parity Check ===" -ForegroundColor Cyan
Write-Host "Repo root: $RepoRoot"

# 1. global.json exists and has specific 10.0.x version
$globalJsonPath = Join-Path $RepoRoot 'src\backend\global.json'
if (-not (Test-Path $globalJsonPath)) {
    $failures += "global.json not found at $globalJsonPath"
} else {
    $globalJson = Get-Content $globalJsonPath -Raw | ConvertFrom-Json
    $pinnedVersion = $globalJson.sdk.version
    Write-Host "global.json SDK version: $pinnedVersion"

    if ($pinnedVersion -notmatch '^10\.0\.\d+$') {
        $failures += "global.json sdk.version '$pinnedVersion' is not a specific 10.0.x version"
    }

    # 2. Installed SDK matches
    $installedVersion = & dotnet --version 2>$null
    Write-Host "Installed SDK version:   $installedVersion"
    if ($installedVersion -ne $pinnedVersion) {
        $failures += "Installed SDK '$installedVersion' does not match global.json '$pinnedVersion'"
    }
}

# 3. Backend projects target net10.0
$projectFiles = @(
    'src\backend\src\RecipeApi\RecipeApi.csproj',
    'src\backend\tests\RecipeApi.Tests\RecipeApi.Tests.csproj'
)

foreach ($proj in $projectFiles) {
    $projPath = Join-Path $RepoRoot $proj
    if (-not (Test-Path $projPath)) {
        $failures += "Project file not found: $projPath"
        continue
    }
    [xml]$csproj = Get-Content $projPath
    $tfm = $csproj.Project.PropertyGroup.TargetFramework | Where-Object { $_ }
    Write-Host "$proj TFM: $tfm"
    if ($tfm -ne 'net10.0') {
        $failures += "$proj targets '$tfm' instead of 'net10.0'"
    }
}

# 4. Directory.Build.props enforces C# 12
$dirBuildProps = Join-Path $RepoRoot 'src\backend\Directory.Build.props'
if (-not (Test-Path $dirBuildProps)) {
    $failures += "Directory.Build.props not found at $dirBuildProps"
} else {
    [xml]$props = Get-Content $dirBuildProps
    $langVersion = $props.Project.PropertyGroup.LangVersion | Where-Object { $_ }
    Write-Host "Directory.Build.props LangVersion: $langVersion"
    if ($langVersion -notmatch '^12') {
        $failures += "Directory.Build.props LangVersion '$langVersion' does not enforce C# 12"
    }
}

# Report
Write-Host ""
if ($failures.Count -eq 0) {
    Write-Host "✅ All SDK parity checks passed." -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ SDK parity check FAILED:" -ForegroundColor Red
    foreach ($f in $failures) {
        Write-Host "  - $f" -ForegroundColor Yellow
    }
    exit 1
}
