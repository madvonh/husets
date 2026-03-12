using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;
using RecipeApi.Repositories;

namespace RecipeApi.Services;

public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient? _cosmosClient;
    private readonly CosmosDbInitializer? _initializer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbHealthCheck> _logger;

    public CosmosDbHealthCheck(
        IConfiguration configuration,
        ILogger<CosmosDbHealthCheck> logger,
        CosmosClient? cosmosClient = null,
        CosmosDbInitializer? initializer = null)
    {
        _configuration = configuration;
        _logger = logger;
        _cosmosClient = cosmosClient;
        _initializer = initializer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration["CosmosDb:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Degraded("CosmosDb connection string not configured; using in-memory fallback");
            }

            if (_initializer is { IsInitialized: false })
            {
                return HealthCheckResult.Unhealthy(
                    "Cosmos DB initialization failed",
                    _initializer.InitializationException);
            }

            if (_cosmosClient is null)
            {
                return HealthCheckResult.Unhealthy("CosmosDb client not available");
            }

            var databaseName = _configuration["CosmosDb:DatabaseName"];
            var database = _cosmosClient.GetDatabase(databaseName);

            await database.ReadAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("CosmosDb is connected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CosmosDb health check failed");
            return HealthCheckResult.Unhealthy("CosmosDb is not connected", ex);
        }
    }
}
