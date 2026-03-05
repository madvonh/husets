namespace RecipeApi.Tests.PlatformValidation;

/// <summary>
/// Tests that verify backend projects target the correct framework and language version.
/// These tests validate the platform baseline defined in the constitution and spec.
/// </summary>
public class PlatformBaselineTests
{
    private const string ApiCsproj = "src/backend/src/RecipeApi/RecipeApi.csproj";
    private const string TestCsproj = "src/backend/tests/RecipeApi.Tests/RecipeApi.Tests.csproj";
    private const string DirectoryBuildProps = "src/backend/Directory.Build.props";

    // --- T010: API project targets net10.0 ---

    [Fact]
    public void ApiProject_TargetFramework_IsNet10()
    {
        var tfm = FileAssertionHelpers.GetCsprojProperty(ApiCsproj, "TargetFramework");
        Assert.Equal("net10.0", tfm);
    }

    // --- T011: Test project targets net10.0 and uses C# 12 ---

    [Fact]
    public void TestProject_TargetFramework_IsNet10()
    {
        var tfm = FileAssertionHelpers.GetCsprojProperty(TestCsproj, "TargetFramework");
        Assert.Equal("net10.0", tfm);
    }

    [Fact]
    public void Backend_LangVersion_IsCSharp12()
    {
        // C# 12 is enforced via Directory.Build.props
        var langVersion = FileAssertionHelpers.GetDirectoryBuildPropsLangVersion(DirectoryBuildProps);
        Assert.NotNull(langVersion);
        Assert.StartsWith("12", langVersion);
    }

    [Fact]
    public void GlobalJson_PinsSdk_To10_0_x()
    {
        var version = FileAssertionHelpers.GetGlobalJsonSdkVersion();
        Assert.NotNull(version);
        Assert.Matches(@"^10\.0\.\d+$", version);
    }

    [Fact]
    public void ApiProject_NoLegacyFrameworkReference()
    {
        var tfm = FileAssertionHelpers.GetCsprojProperty(ApiCsproj, "TargetFramework");
        Assert.DoesNotContain("net9", tfm ?? "");
        Assert.DoesNotContain("net8", tfm ?? "");
    }

    [Fact]
    public void TestProject_NoLegacyFrameworkReference()
    {
        var tfm = FileAssertionHelpers.GetCsprojProperty(TestCsproj, "TargetFramework");
        Assert.DoesNotContain("net9", tfm ?? "");
        Assert.DoesNotContain("net8", tfm ?? "");
    }
}
