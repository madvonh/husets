using FluentValidation;
using RecipeApi.Middleware;
using RecipeApi.Services;
using RecipeApi.Endpoints;
using RecipeApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

var allowedOriginsSection = builder.Configuration.GetSection("AllowedOrigins");
var allowedOrigins = allowedOriginsSection.Get<string[]>()
    ?? builder.Configuration["AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

builder.Services.ConfigureCors(allowedOrigins);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerGen();

var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
builder.Services.ConfigureCosmosDb(cosmosConnectionString);
   

var blobConnectionString = builder.Configuration["BlobStorage:ConnectionString"];
builder.Services.ConfigureBlobStorage(blobConnectionString);

var azureVisionEndpoint = builder.Configuration["AzureVision:Endpoint"];
var azureVisionApiKey = builder.Configuration["AzureVision:ApiKey"];
builder.Services.ConfigureOcrService(azureVisionEndpoint, azureVisionApiKey);

builder.Services.AddTransient<IIngredientParser, IngredientParser>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<CosmosDbHealthCheck>("cosmosdb");

builder.Services.AddSingleton<CosmosDbHealthCheck>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseOptionsSwaggerUI();
}

// Enforce HTTPS and HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "Recipe Collection API", version = "1.0.0" }));

app.MapRecipeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
