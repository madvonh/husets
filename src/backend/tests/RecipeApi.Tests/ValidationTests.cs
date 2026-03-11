using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RecipeApi.Models;
using RecipeApi.Models.DTOs;
using RecipeApi.Services;

namespace RecipeApi.Tests;

[TestFixture]
public class ValidationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var ocrService = Substitute.For<IOcrService>();
        ocrService.ExtractTextFromImageAsync(Arg.Any<Stream>()).Returns(Task.FromResult("Test OCR text"));

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                var ocrServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrService));
                if (ocrServiceDescriptor != null)
                {
                    services.Remove(ocrServiceDescriptor);
                }

                services.AddSingleton(ocrService);
            });
        });

        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Title must be at least 3 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Title must not exceed 200 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Recipe text must not exceed 10,000 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Recipe cannot have more than 20 tags"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Tag must be at least 2 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Tag must not exceed 50 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Tag must contain only lowercase letters, numbers, and hyphens"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Tag must be at least 2 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Tag must not exceed 50 characters"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Code, Is.EqualTo("FILE_TOO_LARGE"));
        Assert.That(error.Message, Does.Contain("exceeds maximum allowed size"));
    }

    [Test]
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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Code, Is.EqualTo("INVALID_FILE_TYPE"));
        Assert.That(error.Message, Does.Contain("Invalid image type"));
    }

    [Test]
    public async Task OcrEndpoint_WithValidJpeg_Returns200()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var jpegBytes = Convert.FromBase64String("/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxAQEBUQEA8QDw8PEA8PDw8PEA8QDxAQFREWFhURFRUYHSggGBolGxUVITEhJSkrLi4uFx8zODMtNygtLisBCgoKDQ0NDg0NDisZHhkrKysrKystKy0tKy0rLS0rKysrKysrKysrKysrKysrKysrKysrKysrKysrKysrK//AABEIAAEAAQMBIgACEQEDEQH/xAAWAAEBAQAAAAAAAAAAAAAAAAAAAQL/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAgP/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCdAAf/2Q==");
        var byteContent = new ByteArrayContent(jpegBytes);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "image", "test.jpg");

        // Act
        var response = await _client.PostAsync("/ocr", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
