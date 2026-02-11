using RecipeApi.Utilities;
using Xunit;

namespace RecipeApi.Tests;

/// <summary>
/// Unit tests for search query and tag normalization logic
/// These test the helper functions used by the search endpoint
/// </summary>
public class SearchNormalizationTests
{
    [Theory]
    [InlineData("dessert", "dessert")]
    [InlineData("Dessert", "dessert")]
    [InlineData("DESSERT", "dessert")]
    [InlineData("  dessert  ", "dessert")]
    [InlineData("  Dessert  ", "dessert")]
    [InlineData("gluten-free", "gluten-free")]
    [InlineData("Gluten-Free", "gluten-free")]
    public void NormalizeTag_HandlesVariousInputs(string input, string expected)
    {
        // Act
        var result = TagNormalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NormalizeTag_HandlesEmptyInput(string? input)
    {
        // Act
        var result = TagNormalizer.Normalize(input ?? "");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeTagList_RemovesDuplicates()
    {
        // Arrange
        var tags = new List<string> { "dessert", "Dessert", "DESSERT", "chocolate", "Chocolate" };

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.Equal(2, result.Count); // Should only have 'dessert' and 'chocolate'
        Assert.Contains("dessert", result);
        Assert.Contains("chocolate", result);
    }

    [Fact]
    public void NormalizeTagList_TrimsAndLowercasesAll()
    {
        // Arrange
        var tags = new List<string> { "  Dessert  ", "CHOCOLATE  ", "  vegan" };

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.All(result, tag => Assert.Equal(tag, tag.ToLowerInvariant().Trim()));
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void NormalizeTagList_HandlesEmptyList()
    {
        // Arrange
        var tags = new List<string>();

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("chocolate cake", "chocolate cake")]
    [InlineData("  chocolate cake  ", "chocolate cake")]
    [InlineData("  Chocolate   Cake  ", "chocolate   cake")] // Preserves internal spacing
    public void NormalizeQuery_TrimsAndLowercases(string input, string expected)
    {
        // Act
        var result = SearchQueryNormalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildSearchText_CombinesFieldsWithSpaces()
    {
        // Arrange
        var title = "Chocolate Chip Cookies";
        var rawText = "Mix flour, sugar, chocolate chips. Bake at 350°F.";
        var tags = new List<string> { "dessert", "cookies", "chocolate" };

        // Act
        var searchText = SearchTextBuilder.Build(title, rawText, tags);

        // Assert
        Assert.Contains("chocolate chip cookies", searchText.ToLower());
        Assert.Contains("flour", searchText.ToLower());
        Assert.Contains("dessert", searchText.ToLower());
        Assert.Contains("cookies", searchText.ToLower());
    }

    [Fact]
    public void BuildSearchText_HandlesEmptyFields()
    {
        // Arrange
        var title = "Test Recipe";
        var rawText = "";
        var tags = new List<string>();

        // Act
        var searchText = SearchTextBuilder.Build(title, rawText, tags);

        // Assert
        Assert.Contains("test recipe", searchText.ToLower());
        Assert.DoesNotContain("null", searchText.ToLower());
    }
}

/// <summary>
/// Placeholder for search query normalization utility
/// Will be implemented in the actual production code
/// </summary>
public static class SearchQueryNormalizer
{
    public static string Normalize(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return string.Empty;

        return query.Trim().ToLowerInvariant();
    }
}
