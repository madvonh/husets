namespace RecipeApi.Utilities;

public static class TagNormalizer
{
    public static string Normalize(string tag)
    {
        return tag.Trim().ToLowerInvariant();
    }

    public static List<string> Normalize(List<string> tags)
    {
        return tags.Select(Normalize).Distinct().ToList();
    }
}

public static class SearchTextBuilder
{
    public static string BuildSearchText(string title, string rawText)
    {
        var combined = $"{title} {rawText}";
        return combined.ToLowerInvariant()
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Trim();
    }

    public static string Build(string title, string rawText, List<string> tags)
    {
        var tagText = tags.Count > 0 ? string.Join(" ", tags) : "";
        var combined = $"{title} {rawText} {tagText}";
        return combined.ToLowerInvariant()
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Trim();
    }
}
