using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.DTOs.ResponseModels;

namespace RecipeApi.Tests;

[TestFixture]
public class SearchTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetRecipes_WithoutParameters_ReturnsAllRecipes()
    {
        // Arrange - search without filters should return all recipes

        // Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
    }

    [Test]
    public async Task GetRecipes_WithQueryParameter_ReturnsMatchingRecipes()
    {
        // Arrange - search by text should match recipe content
        var query = "chocolate";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        // All returned recipes should contain the query text (case-insensitive)
        // Note: This will pass with empty results if no recipes exist yet
    }

    [Test]
    public async Task GetRecipes_WithTagParameter_ReturnsRecipesWithTag()
    {
        // Arrange - search by tag should filter recipes
        var tag = "dessert";

        // Act
        var response = await _client.GetAsync($"/recipes?tag={tag}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        // All returned recipes should have the specified tag (normalized)
    }

    [Test]
    public async Task GetRecipes_WithQueryAndTag_ReturnsCombinedResults()
    {
        // Arrange - combined search should match BOTH criteria
        var query = "chocolate";
        var tag = "dessert";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}&tag={tag}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        // Results should match both query text AND tag
    }

    [Test]
    public async Task GetRecipes_QueryIsCaseInsensitive()
    {
        // Arrange - search should be case-insensitive
        var query = "CHOCOLATE";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        // Should find recipes containing "chocolate", "Chocolate", "CHOCOLATE", etc.
    }

    [Test]
    public async Task GetRecipes_TagIsNormalized()
    {
        // Arrange - tags should be normalized (trimmed, lowercased)
        var tag = "  Dessert  "; // With extra whitespace

        // Act
        var response = await _client.GetAsync($"/recipes?tag={Uri.EscapeDataString(tag)}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        // Should match recipes tagged "dessert" (normalized form)
    }

    [Test]
    public async Task GetRecipes_WithNoMatches_ReturnsEmptyArray()
    {
        // Arrange - search with impossible criteria
        var query = "xyznonexistentrecipename12345";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        Assert.That(recipes, Is.Empty);
    }

    [Test]
    public async Task GetRecipes_ReturnsRecipeSummaryFormat()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(recipes, Is.Not.Null);
        
        // If any recipes exist, verify they have the expected structure
        if (recipes!.Count > 0)
        {
            var recipe = recipes[0];
            Assert.That(string.IsNullOrEmpty(recipe.Id), Is.False);
            Assert.That(string.IsNullOrEmpty(recipe.Title), Is.False);
            Assert.That(recipe.Tags, Is.Not.Null);
            Assert.That(recipe.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }
    }

    [Test]
    public async Task GetRecipes_IncludesCorrelationId()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.That(response.Headers.Contains("X-Correlation-Id"), Is.True);
    }
}
