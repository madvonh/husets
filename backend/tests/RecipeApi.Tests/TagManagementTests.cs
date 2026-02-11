using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;
using Xunit;

namespace RecipeApi.Tests;

public class TagManagementTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TagManagementTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddTag_ToExistingRecipe_ReturnsOk()
    {
        // Arrange - create a recipe first
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "dessert" };

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.Contains("dessert", updatedRecipe.Tags);
    }

    [Fact]
    public async Task AddTag_NormalizesTagName()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "  Dessert  " }; // With extra whitespace

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.Contains("dessert", updatedRecipe.Tags); // Should be normalized to lowercase
    }

    [Fact]
    public async Task AddTag_DuplicateTag_IsIdempotent()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "dessert" };

        // Act - add same tag twice
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.Single(updatedRecipe.Tags.Where(t => t == "dessert")); // Should only appear once
    }

    [Fact]
    public async Task AddTag_ToNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "recipe_" + Guid.NewGuid();
        var tagRequest = new { tag = "dessert" };

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{nonExistentId}/tags", tagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveTag_FromRecipe_ReturnsOk()
    {
        // Arrange - create recipe with a tag
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "dessert" });

        // Act
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/dessert");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.DoesNotContain("dessert", updatedRecipe.Tags);
    }

    [Fact]
    public async Task RemoveTag_NormalizesTagName()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "dessert" });

        // Act - remove using different casing and whitespace
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/  DESSERT  ");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.DoesNotContain("dessert", updatedRecipe.Tags);
    }

    [Fact]
    public async Task RemoveTag_NonExistentTag_IsIdempotent()
    {
        // Arrange
        var recipe = await CreateTestRecipe();

        // Act - remove tag that doesn't exist
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/nonexistent");

        // Assert - should succeed (idempotent)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveTag_FromNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "recipe_" + Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/recipes/{nonExistentId}/tags/dessert");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TagChanges_ReflectInSearch()
    {
        // Arrange - create recipe and add tag
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "chocolate" });

        // Act - search by tag
        var searchResponse = await _client.GetAsync("/recipes?tag=chocolate");

        // Assert
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var results = await searchResponse.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(results);
        Assert.Contains(results, r => r.Id == recipe.Id);
    }

    [Fact]
    public async Task RemovedTag_NoLongerAppearsInSearch()
    {
        // Arrange - create recipe, add tag, then remove it
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "chocolate" });
        await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/chocolate");

        // Act - search by removed tag
        var searchResponse = await _client.GetAsync("/recipes?tag=chocolate");

        // Assert
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var results = await searchResponse.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.NotNull(results);
        Assert.DoesNotContain(results, r => r.Id == recipe.Id);
    }

    private async Task<RecipeDetailResponse> CreateTestRecipe()
    {
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe " + Guid.NewGuid().ToString().Substring(0, 8),
            RawText = "Test recipe text",
            ImageRef = "test-image-ref"
        };

        var response = await _client.PostAsJsonAsync("/recipes", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RecipeDetailResponse>())!;
    }
}
