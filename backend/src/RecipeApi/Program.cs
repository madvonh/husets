using FluentValidation;
using Microsoft.Azure.Cosmos;
using RecipeApi.Middleware;
using RecipeApi.Services;
using RecipeApi.Validators;
using RecipeApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Correlation-Id");
    });
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Azure services (or fallback in-memory implementation for local/tests)
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddSingleton(_ =>
    {
        return new CosmosClient(cosmosConnectionString);
    });
    builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
}
else
{
    // Fallback: provide an in-memory implementation so tests and local runs don't require Cosmos DB
    builder.Services.AddSingleton<ICosmosDbService, InMemoryCosmosDbService>();
}

// Add Blob Storage service
var blobConnectionString = builder.Configuration["BlobStorage:ConnectionString"];
if (!string.IsNullOrEmpty(blobConnectionString))
{
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
}
else
{
    // Fallback: provide an in-memory implementation for tests and local runs
    builder.Services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
}

// Add OCR service
builder.Services.AddSingleton<IOcrService, AzureOcrService>();
builder.Services.AddTransient<IIngredientParser, IngredientParser>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<CosmosDbHealthCheck>("cosmosdb");

builder.Services.AddSingleton<CosmosDbHealthCheck>();

var app = builder.Build();

// Use middleware
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();
app.UseHttpsRedirection();

// Map endpoints
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "Recipe Collection API", version = "1.0.0" }));

app.MapRecipeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
