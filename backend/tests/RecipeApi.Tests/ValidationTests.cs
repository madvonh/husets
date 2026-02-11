using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;
using Xunit;

namespace RecipeApi.Tests;

public class ValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRecipe_WithTooShortTitle_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = "AB", // Too short (min 3)
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Title must be at least 3 characters", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithTooLongTitle_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = new string('A', 201), // Too long (max 200)
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Title must not exceed 200 characters", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithTooLongRawText_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = new string('A', 10001), // Too long (max 10000)
            ImageRef = "image-ref",
            Tags = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Recipe text must not exceed 10,000 characters", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithTooManyTags_Returns400()
    {
        // Arrange
        var tags = new List<string>();
        for (int i = 0; i < 21; i++)
        {
            tags.Add($"tag{i}");
        }

        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = tags
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Recipe cannot have more than 20 tags", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithTooShortTag_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = new List<string> { "a" } // Too short (min 2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Tag must be at least 2 characters", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithTooLongTag_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = new List<string> { new string('a', 51) } // Too long (max 50)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Tag must not exceed 50 characters", error.Message);
    }

    [Fact]
    public async Task CreateRecipe_WithInvalidTagCharacters_Returns400()
    {
        // Arrange
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = new List<string> { "Invalid Tag!" } // Has space and special chars
        };

        // Act
        var response = await _client.PostAsJsonAsync("/recipes", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Tag must contain only lowercase letters, numbers, and hyphens", error.Message);
    }

    [Fact]
    public async Task AddTag_WithTooShortTag_Returns400()
    {
        // Arrange - First create a recipe
        var createRequest = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = null
        };
        var createResponse = await _client.PostAsJsonAsync("/recipes", createRequest);
        var recipe = await createResponse.Content.ReadFromJsonAsync<RecipeDetailResponse>();

        var addTagRequest = new AddTagRequest { Tag = "a" }; // Too short

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe!.Id}/tags", addTagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Tag must be at least 2 characters", error.Message);
    }

    [Fact]
    public async Task AddTag_WithTooLongTag_Returns400()
    {
        // Arrange - First create a recipe
        var createRequest = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            RawText = "Some recipe text",
            ImageRef = "image-ref",
            Tags = null
        };
        var createResponse = await _client.PostAsJsonAsync("/recipes", createRequest);
        var recipe = await createResponse.Content.ReadFromJsonAsync<RecipeDetailResponse>();

        var addTagRequest = new AddTagRequest { Tag = new string('a', 51) }; // Too long

        // Act
        var response = await _client.PostAsJsonAsync($"/recipes/{recipe!.Id}/tags", addTagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("Tag must not exceed 50 characters", error.Message);
    }

    [Fact]
    public async Task OcrEndpoint_WithTooLargeFile_Returns400()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        
        // Create a fake 11MB file (exceeds 10MB limit)
        var largeFileContent = new byte[11 * 1024 * 1024];
        var byteContent = new ByteArrayContent(largeFileContent);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "image", "large.jpg");

        // Act
        var response = await _client.PostAsync("/ocr", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("FILE_TOO_LARGE", error.Code);
        Assert.Contains("exceeds maximum allowed size", error.Message);
    }

    [Fact]
    public async Task OcrEndpoint_WithInvalidFileType_Returns400()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake pdf content"));
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(byteContent, "image", "document.pdf");

        // Act
        var response = await _client.PostAsync("/ocr", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("INVALID_FILE_TYPE", error.Code);
        Assert.Contains("Invalid image type", error.Message);
    }

    [Fact]
    public async Task OcrEndpoint_WithValidJpeg_Returns200()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(new byte[1024]); // 1KB file
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "image", "test.jpg");

        // Act
        var response = await _client.PostAsync("/ocr", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
