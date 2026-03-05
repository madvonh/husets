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

    // --- T018: VS Code backend task path/cmd alignment ---

    [Fact]
    public void TasksJson_BackendBuild_ReferencesCorrectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(TasksJson);
        // Should reference src/backend path, not just backend
        Assert.Contains("src/backend", content.Replace("\\", "/"));
        // Should NOT reference the old stale path without src/ prefix
        Assert.DoesNotContain("${workspaceFolder}/backend/", content);
    }

    [Fact]
    public void TasksJson_FrontendTask_ReferencesCorrectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(TasksJson);
        // Frontend should reference src/frontend
        Assert.Contains("src/frontend", content.Replace("\\", "/"));
    }

    // --- T019: VS Code backend launch output path (net10.0) ---

    [Fact]
    public void LaunchJson_BackendDebug_ReferencesNet10OutputPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(LaunchJson);
        Assert.Contains("net10.0", content);
        Assert.DoesNotContain("net9.0", content);
    }

    [Fact]
    public void LaunchJson_BackendDebug_ReferencesCorrectProjectPath()
    {
        var content = FileAssertionHelpers.ReadTextFile(LaunchJson);
        Assert.Contains("src/backend", content.Replace("\\", "/"));
        Assert.DoesNotContain("${workspaceFolder}/backend/", content);
    }

    // --- T020: CI workflow SDK/version parity with global.json ---

    [Fact]
    public void CiWorkflow_Exists()
    {
        var fullPath = Path.Combine(FileAssertionHelpers.GetRepoRoot(), CiWorkflow);
        Assert.True(File.Exists(fullPath), $"CI workflow not found: {CiWorkflow}");
    }

    [Fact]
    public void CiWorkflow_UsesSameSdkAsGlobalJson()
    {
        var globalSdkVersion = FileAssertionHelpers.GetGlobalJsonSdkVersion();
        var ciContent = FileAssertionHelpers.ReadTextFile(CiWorkflow);
        Assert.Contains(globalSdkVersion!, ciContent);
    }
}
