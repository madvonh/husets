namespace RecipeApi.Tests.PlatformValidation;

/// <summary>
/// Tests that verify README and quickstart docs reference the correct
/// platform baseline (.NET 10, C# 12) and backend paths.
/// </summary>
public class DocumentationBaselineTests
{
    private const string ReadmeMd = "README.md";
    private const string RecipeQuickstart = "specs/001-recipe-collection/quickstart.md";

    // --- T027: README platform/version and backend path guidance ---

    [Fact]
    public void Readme_References_DotNet10()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        // Should mention .NET 10 somewhere (SDK, runtime, or platform reference)
        Assert.Matches(@"\.NET\s+10", content);
    }

    [Fact]
    public void Readme_References_CSharp12()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        Assert.Matches(@"C#\s*12", content);
    }

    [Fact]
    public void Readme_BackendPaths_AreCorrect()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        // Should reference src/backend paths
        Assert.Contains("src/backend", content);
    }

    [Fact]
    public void Readme_DoesNotReference_DotNet9()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        Assert.DoesNotMatch(@"\.NET\s+9\b", content);
    }

    // --- T028: Quickstart baseline guidance and command consistency ---

    [Fact]
    public void RecipeQuickstart_References_DotNet10Sdk()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Should reference .NET 10 SDK requirement (may have markdown bold markers)
        Assert.Matches(@"\.NET[\s*]+(SDK[\s*]+)?10", content);
    }

    [Fact]
    public void RecipeQuickstart_BackendCommands_UseCorrectPaths()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Commands like "cd backend/" should be "cd src/backend/"
        // Should NOT reference bare "cd backend/" without "src/" prefix
        Assert.DoesNotMatch(@"cd\s+backend/", content);
    }

    [Fact]
    public void RecipeQuickstart_FrontendCommands_UseCorrectPaths()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Frontend commands should use src/frontend paths
        Assert.DoesNotMatch(@"cd\s+frontend\b(?!/)", content);
    }
}
