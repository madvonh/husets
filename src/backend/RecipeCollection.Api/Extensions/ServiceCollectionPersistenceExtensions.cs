using Microsoft.Azure.Cosmos;
using RecipeApi.Repositories;
using RecipeApi.Services;

namespace RecipeApi.Extensions;

public static class ServiceCollectionPersistenceExtensions
{
    /// <summary>
    /// Configures Cosmos DB services for the API, including the repository and initializer. If no connection string is provided, an in-memory implementation is registered for testing and local development.     
    /// </summary>
    /// <param name="services">The service collection to add persistence services to.</param>
    /// <param name="cosmosConnectionString">The Cosmos DB connection string.</param>
    public static void ConfigureCosmosDb(this IServiceCollection services, string? cosmosConnectionString)
    {
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            var isLocalEmulator = cosmosConnectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                            || cosmosConnectionString.Contains("127.0.0.1", StringComparison.Ordinal);

            services.AddSingleton(_ =>
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
            services.AddSingleton<ICosmosDbRepository, CosmosDbRepository>();

            // Auto-create database and container on startup (FR-004)
            services.AddSingleton<CosmosDbInitializer>();
            services.AddHostedService(sp => sp.GetRequiredService<CosmosDbInitializer>());
        }
        else
        {
            // Fallback: provide an in-memory implementation so tests and local runs don't require Cosmos DB
            services.AddSingleton<ICosmosDbRepository, InMemoryCosmosDbRepository>();
        }
    }

    /// <summary>
    /// Configures blob storage services for the API. If a connection string is provided, it registers the BlobStorageService; otherwise, it falls back to an in-memory implementation for testing and local development.
    /// </summary>
    /// <param name="services">The service collection to add the blob storage service to.</param>
    /// <param name="blobConnectionString">The blob storage connection string.</param>
    public static void ConfigureBlobStorage(this IServiceCollection services, string? blobConnectionString)
    {
        
        if (!string.IsNullOrEmpty(blobConnectionString))
        {
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
        }
        else
        {
            // Fallback: provide an in-memory implementation for tests and local runs
            services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
        }
    }
}
