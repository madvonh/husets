using FluentValidation;
using RecipeApi.Endpoints;
using RecipeApi.Extensions;
using RecipeApi.Middleware;
using RecipeApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults for observability and resilience
builder.AddServiceDefaults();

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

var app = builder.Build();

// Map Aspire ServiceDefaults endpoints
app.MapDefaultEndpoints();

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

//app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "Recipe Collection API", version = "1.0.0" }));

app.MapRecipeEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
