using RecipeCollection.Utilities;

namespace RecipeApi.Tests;

/// <summary>
/// Unit tests for search query and tag normalization logic
/// These test the helper functions used by the search endpoint
/// </summary>
public class SearchNormalizationTests
{
    [TestCase("dessert", "dessert")]
    [TestCase("Dessert", "dessert")]
    [TestCase("DESSERT", "dessert")]
    [TestCase("  dessert  ", "dessert")]
    [TestCase("  Dessert  ", "dessert")]
    [TestCase("gluten-free", "gluten-free")]
    [TestCase("Gluten-Free", "gluten-free")]
    public void NormalizeTag_HandlesVariousInputs(string input, string expected)
    {
        // Act
        var result = TagNormalizer.Normalize(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void NormalizeTag_HandlesEmptyInput(string? input)
    {
        // Act
        var result = TagNormalizer.Normalize(input ?? "");

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormalizeTagList_RemovesDuplicates()
    {
        // Arrange
        var tags = new List<string> { "dessert", "Dessert", "DESSERT", "chocolate", "Chocolate" };

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2)); // Should only have 'dessert' and 'chocolate'
        Assert.That(result, Contains.Item("dessert"));
        Assert.That(result, Contains.Item("chocolate"));
    }

    [Test]
    public void NormalizeTagList_TrimsAndLowercasesAll()
    {
        // Arrange
        var tags = new List<string> { "  Dessert  ", "CHOCOLATE  ", "  vegan" };

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        foreach (var tag in result)
        {
            Assert.That(tag, Is.EqualTo(tag.ToLowerInvariant().Trim()));
        }
    }

    [Test]
    public void NormalizeTagList_HandlesEmptyList()
    {
        // Arrange
        var tags = new List<string>();

        // Act
        var result = TagNormalizer.Normalize(tags);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase("chocolate cake", "chocolate cake")]
    [TestCase("  chocolate cake  ", "chocolate cake")]
    [TestCase("  Chocolate   Cake  ", "chocolate   cake")] // Preserves internal spacing
    public void NormalizeQuery_TrimsAndLowercases(string input, string expected)
    {
        // Act
        var result = SearchQueryNormalizer.Normalize(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void BuildSearchText_CombinesFieldsWithSpaces()
    {
        // Arrange
        var title = "Chocolate Chip Cookies";
        var rawText = "Mix flour, sugar, chocolate chips. Bake at 350°F.";
        var tags = new List<string> { "dessert", "cookies", "chocolate" };

        // Act
        var searchText = SearchTextBuilder.Build(title, rawText, tags);

        // Assert
        Assert.That(searchText.ToLower(), Does.Contain("chocolate chip cookies"));
        Assert.That(searchText.ToLower(), Does.Contain("flour"));
        Assert.That(searchText.ToLower(), Does.Contain("dessert"));
        Assert.That(searchText.ToLower(), Does.Contain("cookies"));
    }

    [Test]
    public void BuildSearchText_HandlesEmptyFields()
    {
        // Arrange
        var title = "Test Recipe";
        var rawText = "";
        var tags = new List<string>();

        // Act
        var searchText = SearchTextBuilder.Build(title, rawText, tags);

        // Assert
        Assert.That(searchText.ToLower(), Does.Contain("test recipe"));
        Assert.That(searchText.ToLower(), Does.Not.Contain("null"));
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
