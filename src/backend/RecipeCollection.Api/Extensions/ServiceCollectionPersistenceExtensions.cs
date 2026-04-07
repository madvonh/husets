using Azure.Identity;
using Azure.Core;
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
        var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.AddCosmosDbContext<RecipeDbContext>("CosmosDb");
                return;
            }

            var endpoint = ResolveCosmosEndpoint(builder.Configuration, cosmosConnectionString);
            var databaseName = ResolveCosmosDatabaseName(builder.Configuration, cosmosConnectionString);
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

            TokenCredential credential = !string.IsNullOrWhiteSpace(clientId)
                ? new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId))
                : new DefaultAzureCredential();

            builder.Services.AddDbContext<RecipeDbContext>(options =>
                options.UseCosmos(endpoint, credential, databaseName));
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
        if (app.Environment.IsProduction())
        {
            app.Logger.LogInformation(
                "Cosmos startup auth context: HasCosmosConnectionString={HasCosmosConnectionString}, AzureClientId={AzureClientId}, IdentityEndpointPresent={IdentityEndpointPresent}, MsiEndpointPresent={MsiEndpointPresent}",
                !string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("CosmosDb")),
                Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "(null)",
                !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT")),
                !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MSI_ENDPOINT")));
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
        try
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (AuthenticationFailedException ex)
        {
            app.Logger.LogError(
                ex,
                "Cosmos DB initialization failed due to managed identity authentication. The app will continue running without startup failure.");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Could not initialize Cosmos DB at startup; the app will continue.");
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

    private static string ResolveCosmosEndpoint(IConfiguration configuration, string connectionString)
    {
        var directEndpoint = configuration["COSMOSDB_URI"];
        if (!string.IsNullOrWhiteSpace(directEndpoint))
        {
            return directEndpoint;
        }

        const string prefix = "AccountEndpoint=";
        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return segment[prefix.Length..];
            }
        }

        throw new InvalidOperationException("Could not resolve Cosmos endpoint from configuration.");
    }

    private static string ResolveCosmosDatabaseName(IConfiguration configuration, string connectionString)
    {
        var configuredDatabaseName = configuration["COSMOSDB_DATABASENAME"];
        if (!string.IsNullOrWhiteSpace(configuredDatabaseName))
        {
            return configuredDatabaseName;
        }

        const string prefix = "Database=";
        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return segment[prefix.Length..];
            }
        }

        return "cosmosdb";
    }
}
