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

    [Test]
    public void Readme_References_DotNet10()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        // Should mention .NET 10 somewhere (SDK, runtime, or platform reference)
        Assert.That(content, Does.Match(@"\.NET\s+10"));
    }

    [Test]
    public void Readme_References_CSharp12()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        Assert.That(content, Does.Match(@"C#\s*12"));
    }

    [Test]
    public void Readme_BackendPaths_AreCorrect()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        // Should reference src/backend paths
        Assert.That(content, Does.Contain("src/backend"));
    }

    [Test]
    public void Readme_DoesNotReference_DotNet9()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);
        Assert.That(content, Does.Not.Match(@"\.NET\s+9\b"));
    }

    // --- T028: Quickstart baseline guidance and command consistency ---

    [Test]
    public void RecipeQuickstart_References_DotNet10Sdk()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Should reference .NET 10 SDK requirement (may have markdown bold markers)
        Assert.That(content, Does.Match(@"\.NET[\s*]+(SDK[\s*]+)?10"));
    }

    [Test]
    public void RecipeQuickstart_BackendCommands_UseCorrectPaths()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Commands like "cd backend/" should be "cd src/backend/"
        // Should NOT reference bare "cd backend/" without "src/" prefix
        Assert.That(content, Does.Not.Match(@"cd\s+backend/"));
    }

    [Test]
    public void RecipeQuickstart_FrontendCommands_UseCorrectPaths()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);
        // Frontend commands should use src/frontend paths
        Assert.That(content, Does.Not.Match(@"cd\s+frontend\b(?!/)"));
    }

    [Test]
    public void Readme_BackendTestingGuidance_ReferencesApprovedStack()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);

        Assert.That(content, Does.Contain("NUnit"));
        Assert.That(content, Does.Contain("NSubstitute"));
        Assert.That(content, Does.Contain("Microsoft.NET.Test.Sdk"));
    }

    [Test]
    public void Readme_BackendTestingGuidance_DoesNotReferenceXunit()
    {
        var content = FileAssertionHelpers.ReadTextFile(ReadmeMd);

        Assert.That(content, Does.Not.Contain("xUnit"));
        Assert.That(content, Does.Not.Contain("Xunit"));
    }

    [Test]
    public void RecipeQuickstart_BackendTestingGuidance_ReferencesApprovedStack()
    {
        var content = FileAssertionHelpers.ReadTextFile(RecipeQuickstart);

        Assert.That(content, Does.Contain("NUnit"));
        Assert.That(content, Does.Contain("NSubstitute"));
        Assert.That(content, Does.Contain("Microsoft.NET.Test.Sdk"));
    }
}
