using FluentValidation;
using RecipeCollection.Endpoints;
using RecipeCollection.Extensions;
using RecipeCollection.Middleware;
using RecipeCollection.Services;

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

builder.ConfigureCosmosDb();

builder.ConfigureBlobStorage();

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

app.UseRequestTimeouts();
app.UseOutputCache();

app.UseCors();

app.MapGet("/", () => Results.Ok(new { service = "Recipe Collection API", version = "1.0.0" }));

app.MapRecipeEndpoints();

await app.EnsureCosmosCreatedAsync();

await app.RunAsync();

// Make Program accessible to tests
public partial class Program { }
