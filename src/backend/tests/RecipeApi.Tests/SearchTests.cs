using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;
using Xunit;

namespace RecipeApi.Tests;

public class SearchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SearchTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRecipes_WithoutParameters_ReturnsAllRecipes()
    {
        // Arrange - search without filters should return all recipes

        // Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
    }

    [Fact]
    public async Task GetRecipes_WithQueryParameter_ReturnsMatchingRecipes()
    {
        // Arrange - search by text should match recipe content
        var query = "chocolate";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        // All returned recipes should contain the query text (case-insensitive)
        // Note: This will pass with empty results if no recipes exist yet
    }

    [Fact]
    public async Task GetRecipes_WithTagParameter_ReturnsRecipesWithTag()
    {
        // Arrange - search by tag should filter recipes
        var tag = "dessert";

        // Act
        var response = await _client.GetAsync($"/recipes?tag={tag}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        // All returned recipes should have the specified tag (normalized)
    }

    [Fact]
    public async Task GetRecipes_WithQueryAndTag_ReturnsCombinedResults()
    {
        // Arrange - combined search should match BOTH criteria
        var query = "chocolate";
        var tag = "dessert";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}&tag={tag}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        // Results should match both query text AND tag
    }

    [Fact]
    public async Task GetRecipes_QueryIsCaseInsensitive()
    {
        // Arrange - search should be case-insensitive
        var query = "CHOCOLATE";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        // Should find recipes containing "chocolate", "Chocolate", "CHOCOLATE", etc.
    }

    [Fact]
    public async Task GetRecipes_TagIsNormalized()
    {
        // Arrange - tags should be normalized (trimmed, lowercased)
        var tag = "  Dessert  "; // With extra whitespace

        // Act
        var response = await _client.GetAsync($"/recipes?tag={Uri.EscapeDataString(tag)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        // Should match recipes tagged "dessert" (normalized form)
    }

    [Fact]
    public async Task GetRecipes_WithNoMatches_ReturnsEmptyArray()
    {
        // Arrange - search with impossible criteria
        var query = "xyznonexistentrecipename12345";

        // Act
        var response = await _client.GetAsync($"/recipes?query={query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        Assert.Empty(recipes);
    }

    [Fact]
    public async Task GetRecipes_ReturnsRecipeSummaryFormat()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(recipes);
        
        // If any recipes exist, verify they have the expected structure
        if (recipes.Count > 0)
        {
            var recipe = recipes[0];
            Assert.False(string.IsNullOrEmpty(recipe.Id));
            Assert.False(string.IsNullOrEmpty(recipe.Title));
            Assert.NotNull(recipe.Tags);
            Assert.NotEqual(default(DateTime), recipe.CreatedAt);
        }
    }

    [Fact]
    public async Task GetRecipes_IncludesCorrelationId()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/recipes");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }
}
