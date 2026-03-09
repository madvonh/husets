using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;

namespace RecipeApi.Tests;

[TestFixture]
public class TagManagementTests
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
    public async Task AddTag_ToExistingRecipe_ReturnsOk()
    {
        // Arrange - create a recipe first
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "dessert" };

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.That(updatedRecipe, Is.Not.Null);
        Assert.That(updatedRecipe!.Tags, Contains.Item("dessert"));
    }

    [Test]
    public async Task AddTag_NormalizesTagName()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "  Dessert  " }; // With extra whitespace

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.That(updatedRecipe, Is.Not.Null);
        Assert.That(updatedRecipe!.Tags, Contains.Item("dessert")); // Should be normalized to lowercase
    }

    [Test]
    public async Task AddTag_DuplicateTag_IsIdempotent()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        var tagRequest = new { tag = "dessert" };

        // Act - add same tag twice
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", tagRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.That(updatedRecipe, Is.Not.Null);
        Assert.That(updatedRecipe!.Tags, Has.Exactly(1).EqualTo("dessert")); // Should only appear once
    }

    [Test]
    public async Task AddTag_ToNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "recipe_" + Guid.NewGuid();
        var tagRequest = new { tag = "dessert" };

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{nonExistentId}/tags", tagRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task RemoveTag_FromRecipe_ReturnsOk()
    {
        // Arrange - create recipe with a tag
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "dessert" });

        // Act
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/dessert");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.That(updatedRecipe, Is.Not.Null);
        Assert.That(updatedRecipe!.Tags, Has.No.Member("dessert"));
    }

    [Test]
    public async Task RemoveTag_NormalizesTagName()
    {
        // Arrange
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "dessert" });

        // Act - remove using different casing and whitespace
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/  DESSERT  ");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedRecipe = await response.Content.ReadFromJsonAsync<RecipeDetailResponse>();
        Assert.That(updatedRecipe, Is.Not.Null);
        Assert.That(updatedRecipe!.Tags, Has.No.Member("dessert"));
    }

    [Test]
    public async Task RemoveTag_NonExistentTag_IsIdempotent()
    {
        // Arrange
        var recipe = await CreateTestRecipe();

        // Act - remove tag that doesn't exist
        var response = await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/nonexistent");

        // Assert - should succeed (idempotent)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RemoveTag_FromNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "recipe_" + Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/recipes/{nonExistentId}/tags/dessert");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task TagChanges_ReflectInSearch()
    {
        // Arrange - create recipe and add tag
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "chocolate" });

        // Act - search by tag
        var searchResponse = await _client.GetAsync("/recipes?tag=chocolate");

        // Assert
        Assert.That(searchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var results = await searchResponse.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(results, Is.Not.Null);
        Assert.That(results!, Has.Some.Matches<RecipeSummaryResponse>(r => r.Id == recipe.Id));
    }

    [Test]
    public async Task RemovedTag_NoLongerAppearsInSearch()
    {
        // Arrange - create recipe, add tag, then remove it
        var recipe = await CreateTestRecipe();
        await _client.PostAsJsonAsync($"/recipes/{recipe.Id}/tags", new { tag = "chocolate" });
        await _client.DeleteAsync($"/recipes/{recipe.Id}/tags/chocolate");

        // Act - search by removed tag
        var searchResponse = await _client.GetAsync("/recipes?tag=chocolate");

        // Assert
        Assert.That(searchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var results = await searchResponse.Content.ReadFromJsonAsync<List<RecipeSummaryResponse>>();
        Assert.That(results, Is.Not.Null);
        Assert.That(results!, Has.None.Matches<RecipeSummaryResponse>(r => r.Id == recipe.Id));
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
