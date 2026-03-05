# Dependency Compatibility Matrix: net10.0 / C# 12

**Date**: 2026-03-05
**SDK**: .NET 10.0.103

## API Project (`RecipeApi.csproj`)

| Package | Current | Target | Action | Blocker? | Notes |
|---------|---------|--------|--------|----------|-------|
| Microsoft.AspNetCore.OpenApi | 9.0.9 | 10.0.3 | **Upgrade** | Yes | Must match TFM major version |
| Azure.AI.Vision.ImageAnalysis | 1.0.0 | 1.0.0 | Keep | No | netstandard2.0 compatible |
| Azure.Storage.Blobs | 12.27.0 | 12.27.0 | Keep | No | netstandard2.0 compatible |
| FluentValidation.AspNetCore | 11.3.1 | 11.3.1 | Keep | No | netstandard2.0 compatible |
| Microsoft.Azure.Cosmos | 3.57.0 | 3.57.1 | Patch bump | No | netstandard2.0 compatible |
| Newtonsoft.Json | 13.0.4 | 13.0.4 | Keep | No | netstandard2.0 compatible |
| Swashbuckle.AspNetCore | 10.1.2 | 10.1.4 | Patch bump | No | Supports net10.0 |
| Tesseract | 5.2.0 | 5.2.0 | Keep | No | Native interop, netstandard2.0 |

## Test Project (`RecipeApi.Tests.csproj`)

| Package | Current | Target | Action | Blocker? | Notes |
|---------|---------|--------|--------|----------|-------|
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | 10.0.3 | **Upgrade** | Yes | Must match TFM major version |
| Microsoft.NET.Test.Sdk | 17.12.0 | 18.3.0 | Upgrade | No | Latest SDK tooling |
| xunit | 2.9.2 | 2.9.3 | Patch bump | No | |
| xunit.runner.visualstudio | 2.8.2 | 3.1.5 | Major bump | No | v3 runner for modern tooling |
| coverlet.collector | 6.0.2 | 8.0.0 | Major bump | No | Latest coverage collector |

## Blocker Summary

| # | Package | Reason | Resolution |
|---|---------|--------|------------|
| 1 | Microsoft.AspNetCore.OpenApi | 9.x binds to net9.0 shared framework | Upgrade to 10.0.3 |
| 2 | Microsoft.AspNetCore.Mvc.Testing | 9.x binds to net9.0 shared framework | Upgrade to 10.0.3 |

**Status**: All blockers have known resolutions. No unresolvable incompatibilities found.
