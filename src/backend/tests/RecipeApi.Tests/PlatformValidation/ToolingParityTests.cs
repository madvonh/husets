using System.Text.Json;

namespace RecipeApi.Tests.PlatformValidation;

/// <summary>
/// Tests that verify VS Code workspace tasks, launch config, and CI workflow
/// are aligned with the pinned SDK and backend project paths.
/// </summary>
public class ToolingParityTests
{
    private const string TasksJson = ".vscode/tasks.json";
    private const string LaunchJson = ".vscode/launch.json";
    private const string GlobalJson = "src/backend/global.json";
    private const string CiWorkflow = ".github/workflows/backend-ci.yml";
    private const string TestProject = "src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj";

    // --- T018: VS Code backend task path/cmd alignment ---

    [Test]
    public void TasksJson_BackendBuild_ReferencesCorrectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(TasksJson);
        // Should reference src/backend path, not just backend
        Assert.That(content.Replace("\\", "/"), Does.Contain("src/backend"));
        // Should NOT reference the old stale path without src/ prefix
        Assert.That(content, Does.Not.Contain("${workspaceFolder}/backend/"));
    }

    [Test]
    public void TasksJson_FrontendTask_ReferencesCorrectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(TasksJson);
        // Frontend should reference src/frontend
        Assert.That(content.Replace("\\", "/"), Does.Contain("src/frontend"));
    }

    // --- T019: VS Code backend launch output path (net10.0) ---

    [Test]
    public void LaunchJson_BackendDebug_ReferencesNet10OutputPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(LaunchJson);
        Assert.That(content, Does.Contain("net10.0"));
        Assert.That(content, Does.Not.Contain("net9.0"));
    }

    [Test]
    public void LaunchJson_BackendDebug_ReferencesCorrectProjectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(LaunchJson);
        Assert.That(content.Replace("\\", "/"), Does.Contain("src/backend"));
        Assert.That(content, Does.Not.Contain("${workspaceFolder}/backend/"));
    }

    // --- T020: CI workflow SDK/version parity with global.json ---

    [Test]
    public void CiWorkflow_Exists()
    {
        var fullPath = Path.Combine(FileAssertionHelpers.GetRepoRoot(), CiWorkflow);
        Assert.That(File.Exists(fullPath), Is.True, $"CI workflow not found: {CiWorkflow}");
    }

    [Test]
    public void CiWorkflow_UsesSameSdkAsGlobalJson()
    {
        var globalSdkVersion = FileAssertionHelpers.GetGlobalJsonSdkVersion();
        var ciContent = FileAssertionHelpers.ReadTextFile(CiWorkflow);
        Assert.That(ciContent, Does.Contain(globalSdkVersion!));
    }

    [Test]
    public void TestProject_UsesNUnitAsPrimaryFramework()
    {
        var packageReferences = FileAssertionHelpers.GetPackageReferences(TestProject);

        Assert.That(packageReferences.Any(p => p.Include == "NUnit"), Is.True);
        Assert.That(packageReferences.Any(p => p.Include == "NUnit3TestAdapter"), Is.True);
        Assert.That(packageReferences.Any(p => p.Include == "xunit"), Is.False);
        Assert.That(packageReferences.Any(p => p.Include == "xunit.runner.visualstudio"), Is.False);
    }

    [Test]
    public void TestProject_UsesApprovedExecutionAndSubstitutePackages()
    {
        var packageReferences = FileAssertionHelpers.GetPackageReferences(TestProject);

        Assert.That(packageReferences.Any(p => p.Include == "Microsoft.NET.Test.Sdk"), Is.True);
        Assert.That(packageReferences.Any(p => p.Include == "NSubstitute"), Is.True);
    }

    [Test]
    public void TestProject_DoesNotUseLegacyXunitGlobalUsing()
    {
        var content = FileAssertionHelpers.ReadTextFile(TestProject);

        Assert.That(content, Does.Not.Contain("<Using Include=\"Xunit\" />"));
    }
}
