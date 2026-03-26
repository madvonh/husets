using Microsoft.EntityFrameworkCore;
using RecipeCollection.Services;
using RecipeCollection.Data;

namespace RecipeCollection.Extensions;

public static class ServiceCollectionPersistenceExtensions
{
    /// <summary>
    /// Registers RecipeDbContext using the Aspire Cosmos DB integration when the "CosmosDb" connection
    /// string is present (i.e. running under AppHost), or falls back to an in-memory database for tests
    /// and local runs without AppHost. After calling Build(), use EnsureCosmosCreatedAsync to create containers.
    /// </summary>
    public static void ConfigureCosmosDb(this IHostApplicationBuilder builder)
    {
        if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("CosmosDb")))
        {
            builder.AddCosmosDbContext<RecipeDbContext>("CosmosDb");
        }
        else
        {
            builder.Services.AddDbContext<RecipeDbContext>(options =>
                options.UseInMemoryDatabase("RecipeCollection"));
        }
    }

    /// <summary>
    /// Ensures the database for the configured provider is created (for Cosmos DB this may create containers).
    /// Call after app.Build().
    /// </summary>
    public static async Task EnsureCosmosCreatedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
        try 
        { 
            await dbContext.Database.EnsureCreatedAsync(); 
        }
        catch (Exception ex) 
        { 
            app.Logger.LogWarning(ex, "Could not initialize Cosmos DB; the app will continue."); 
        }
    }

    /// <summary>
    /// Configures blob storage services for the API. When running under Aspire, uses the
    /// "blobs" connection string injected via WithReference; otherwise falls back to an
    /// in-memory implementation for tests and local runs without AppHost.
    /// </summary>
    public static void ConfigureBlobStorage(this IHostApplicationBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("blobs")))
        {
            builder.AddAzureBlobServiceClient("blobs");
            builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
        }
        else
        {
            // Fallback: provide an in-memory implementation for tests and local runs
            builder.Services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
        }
    }
}
