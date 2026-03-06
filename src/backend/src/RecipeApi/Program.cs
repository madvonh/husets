using FluentValidation;
using Microsoft.Azure.Cosmos;
using RecipeApi.Middleware;
using RecipeApi.Services;
using RecipeApi.Endpoints;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
var allowedOriginsSection = builder.Configuration.GetSection("AllowedOrigins");
var allowedOrigins = allowedOriginsSection.Get<string[]>()
    ?? builder.Configuration["AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("X-Correlation-Id");
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("X-Correlation-Id");
        }
    });
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recipe Collection API",
        Version = "v1",
        Description = "API for managing recipes with OCR support"
    });
});

// Add Azure services (or fallback in-memory implementation for local/tests)
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    var isLocalEmulator = cosmosConnectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                       || cosmosConnectionString.Contains("127.0.0.1", StringComparison.Ordinal);

    builder.Services.AddSingleton(_ =>
    {
        var options = new CosmosClientOptions();

        if (isLocalEmulator)
        {
            // Bypass SSL validation for the Cosmos DB Emulator's self-signed certificate (FR-011)
            options.ConnectionMode = ConnectionMode.Gateway;
            options.HttpClientFactory = () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return new HttpClient(handler);
            };
        }

        return new CosmosClient(cosmosConnectionString, options);
    });
    builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

    // Auto-create database and container on startup (FR-004)
    builder.Services.AddSingleton<CosmosDbInitializer>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<CosmosDbInitializer>());
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

// Add OCR service (Azure Vision or Tesseract fallback)
var azureVisionEndpoint = builder.Configuration["AzureVision:Endpoint"];
var azureVisionApiKey = builder.Configuration["AzureVision:ApiKey"];

if (!string.IsNullOrEmpty(azureVisionEndpoint) && !string.IsNullOrEmpty(azureVisionApiKey))
{
    builder.Services.AddSingleton<IOcrService, AzureOcrService>();
}
else
{
    // Fallback: use Tesseract for local OCR without Azure
    builder.Services.AddSingleton<IOcrService, TesseractOcrService>();
}

builder.Services.AddTransient<IIngredientParser, IngredientParser>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<CosmosDbHealthCheck>("cosmosdb");

builder.Services.AddSingleton<CosmosDbHealthCheck>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Collection API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use middleware
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "Recipe Collection API", version = "1.0.0" }));

app.MapRecipeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
