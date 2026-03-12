using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.DTOs.RequestModels;

namespace RecipeApi.Tests;

[TestFixture]
public class FoundationalTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert (might be Unhealthy if Cosmos not configured, but should return 200 or 503)
        Assert.That(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable, Is.True);
    }

    [Test]
    public async Task RootEndpoint_ReturnsServiceInfo()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Recipe Collection API"));
    }

    [Test]
    public async Task Response_IncludesCorrelationId()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response.Headers.Contains("X-Correlation-Id"), Is.True);
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest)); // Validation should return BadRequest
    }
}
