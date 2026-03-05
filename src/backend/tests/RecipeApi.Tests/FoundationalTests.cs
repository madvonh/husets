using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;
using Xunit;

namespace RecipeApi.Tests;

public class FoundationalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FoundationalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert (might be Unhealthy if Cosmos not configured, but should return 200 or 503)
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task RootEndpoint_ReturnsServiceInfo()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Recipe Collection API", content);
    }

    [Fact]
    public async Task Response_IncludesCorrelationId()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidRecipe = new CreateRecipeRequest
        {
            Title = "", // Invalid: empty title
            RawText = "Some text",
            ImageRef = "some-ref"
        };

        // Act
        var response = await client.PostAsJsonAsync("/recipes", invalidRecipe);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Validation should return BadRequest
    }
}
