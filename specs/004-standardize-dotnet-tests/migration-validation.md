# Migration Validation: Standardize .NET Test Tooling

## Baseline

- Active backend test project: `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`
- Current primary framework baseline before migration: `xUnit`
- Approved target stack: `NUnit`, `NSubstitute`, `Microsoft.NET.Test.Sdk`
- Current contributor guidance scope for this feature: `README.md` and `specs/001-recipe-collection/quickstart.md`

## Task Evidence

### T002 Current backend test project baseline

- Active backend test project before migration: `src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj`
- Pre-migration package references:
	- `coverlet.collector` `8.0.0`
	- `Microsoft.AspNetCore.Mvc.Testing` `10.0.3`
	- `Microsoft.NET.Test.Sdk` `18.3.0`
	- `xunit` `2.9.3`
	- `xunit.runner.visualstudio` `3.1.5`
- Pre-migration global test using: `<Using Include="Xunit" />`
- Baseline test inventory before migration: 73 tests in the active backend test project.

### T005 Fail-first governance evidence

- Added fail-first governance checks in platform-validation tests for:
	- approved package references (`NUnit`, `NUnit3TestAdapter`, `NSubstitute`, `Microsoft.NET.Test.Sdk`)
	- removal of legacy `xunit` package references and global using
	- current contributor-facing docs referencing the approved stack and no longer referencing `xUnit`
- Fail-first run:
	- Command: `dotnet test .\tests\RecipeApi.Tests\RecipeApi.Tests.csproj --no-restore`
	- Result: failed as expected
	- Summary: 73 total; 6 failed; 67 succeeded; 0 skipped
	- Representative failures:
		- missing `NUnit` package reference
		- missing `NSubstitute` package reference
		- legacy `<Using Include="Xunit" />` still present
		- `README.md` missing `NUnit` guidance and still referencing `xUnit`
		- `specs/001-recipe-collection/quickstart.md` missing approved stack guidance

### T007 Active xUnit-based syntax baseline

- Pre-migration syntax patterns captured:
	- `IClassFixture<WebApplicationFactory<Program>>` in integration-heavy test classes
	- `[Fact]`, `[Theory]`, and `[InlineData]` across the active suite
	- handwritten `FakeOcrService` collaborator replacement in `ValidationTests.cs`
- Migration target applied:
	- NUnit `[TestFixture]`, `[Test]`, `[TestCase]`, `[OneTimeSetUp]`, `[OneTimeTearDown]`
	- `NSubstitute`-based `IOcrService` substitute in `ValidationTests.cs`

### T014 Full-suite passing evidence

- Command: `dotnet test .\tests\RecipeApi.Tests\RecipeApi.Tests.csproj`
- Result: passed
- Summary: 73 total; 0 failed; 73 succeeded; 0 skipped
- Additional note: NUnit adapter discovered all 73 tests successfully.

### T015 CI/backend-test execution assumptions

- CI workflow checked: `.github/workflows/backend-ci.yml`
- Existing backend CI test command remained valid after migration:
	- `dotnet test src/backend/RecipeCollection.sln --no-build --verbosity normal`
- No CI workflow change was required; parity was preserved by keeping the same solution-level test execution path.

### T019 Stable repeated-pass evidence

- Second project test run:
	- Command: `dotnet test .\tests\RecipeApi.Tests\RecipeApi.Tests.csproj`
	- Result: 73 total; 0 failed; 73 succeeded; 0 skipped
- CI-equivalent solution run:
	- Command: `dotnet test .\RecipeCollection.sln --no-restore --verbosity normal`
	- Result: 73 total; 0 failed; 73 succeeded; 0 skipped
- Conclusion: migrated suite is stable across repeated local and CI-equivalent execution.

### T020 Outdated backend testing guidance

- Outdated guidance identified before migration:
	- `README.md` described `RecipeApi.Tests` as an `xUnit` project
	- `specs/001-recipe-collection/quickstart.md` described how to run tests but did not name the approved backend test stack

### T024 Documentation/governance validation

- Updated current contributor-facing guidance to reference `NUnit`, `NSubstitute`, and `Microsoft.NET.Test.Sdk`.
- Updated `.specify/memory/constitution.md` to make the approved backend .NET test stack a governance requirement.
- Documentation and governance checks now pass as part of the 73-test suite.

### T025 Final migration notes and blockers

- Migration completed for the full active backend test project.
- Final active backend test project stack:
	- `Microsoft.NET.Test.Sdk` `18.3.0`
	- `NUnit` `4.2.2`
	- `NUnit3TestAdapter` `4.6.0`
	- `NSubstitute` `5.3.0`
- No blocking migration issues remain.
- Placeholder note: `UnitTest1.cs` was converted to an explicit NUnit stub so the suite contains no legacy xUnit syntax.
