using System.Xml.Linq;

namespace RecipeApi.Tests.PlatformValidation;

/// <summary>
/// Helpers for asserting project file properties and configuration values.
/// </summary>
public static class FileAssertionHelpers
{
    /// <summary>
    /// Resolves a path relative to the repository root (walks up from test bin output
    /// looking for the .git directory as an anchor).
    /// </summary>
    public static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            $"Cannot find repo root (.git directory) from {AppContext.BaseDirectory}");
    }

    /// <summary>
    /// Reads a .csproj file and returns the value of a property element.
    /// </summary>
    public static string? GetCsprojProperty(string csprojRelativePath, string propertyName)
    {
        var fullPath = Path.Combine(GetRepoRoot(), csprojRelativePath);
        Assert.That(File.Exists(fullPath), Is.True, $"Project file not found: {fullPath}");

        var doc = XDocument.Load(fullPath);
        return doc.Descendants(propertyName).FirstOrDefault()?.Value;
    }

    /// <summary>
    /// Reads a .csproj file and returns all PackageReference elements as (Include, Version) tuples.
    /// </summary>
    public static IReadOnlyList<(string Include, string? Version)> GetPackageReferences(string csprojRelativePath)
    {
        var fullPath = Path.Combine(GetRepoRoot(), csprojRelativePath);
        Assert.That(File.Exists(fullPath), Is.True, $"Project file not found: {fullPath}");

        var doc = XDocument.Load(fullPath);
        return doc.Descendants("PackageReference")
            .Select(e => (
                Include: e.Attribute("Include")?.Value ?? "",
                Version: e.Attribute("Version")?.Value))
            .ToList();
    }

    /// <summary>
    /// Reads global.json and returns the sdk.version value.
    /// </summary>
    public static string? GetGlobalJsonSdkVersion()
    {
        var fullPath = Path.Combine(GetRepoRoot(), "src", "backend", "global.json");
        Assert.That(File.Exists(fullPath), Is.True, $"global.json not found: {fullPath}");

        var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(fullPath));
        return json.RootElement.GetProperty("sdk").GetProperty("version").GetString();
    }

    /// <summary>
    /// Reads a JSON file and returns the parsed document.
    /// </summary>
    public static System.Text.Json.JsonDocument ReadJsonFile(string relPath)
    {
        var fullPath = Path.Combine(GetRepoRoot(), relPath);
        Assert.That(File.Exists(fullPath), Is.True, $"File not found: {fullPath}");
        return System.Text.Json.JsonDocument.Parse(File.ReadAllText(fullPath));
    }

    /// <summary>
    /// Reads a text file and returns its content.
    /// </summary>
    public static string ReadTextFile(string relPath)
    {
        var fullPath = Path.Combine(GetRepoRoot(), relPath);
        Assert.That(File.Exists(fullPath), Is.True, $"File not found: {fullPath}");
        return File.ReadAllText(fullPath);
    }

    /// <summary>
    /// Gets the Directory.Build.props LangVersion if it exists.
    /// </summary>
    public static string? GetDirectoryBuildPropsLangVersion(string dirBuildPropsRelativePath)
    {
        var fullPath = Path.Combine(GetRepoRoot(), dirBuildPropsRelativePath);
        if (!File.Exists(fullPath)) return null;

        var doc = XDocument.Load(fullPath);
        return doc.Descendants("LangVersion").FirstOrDefault()?.Value;
    }
}
