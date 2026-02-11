using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;

namespace RecipeApi.Services;

public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbHealthCheck> _logger;

    public CosmosDbHealthCheck(IConfiguration configuration, ILogger<CosmosDbHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration["CosmosDb:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy("CosmosDb connection string not configured");
            }

            var cosmosClient = new CosmosClient(connectionString);
            var databaseName = _configuration["CosmosDb:DatabaseName"];
            var database = cosmosClient.GetDatabase(databaseName);

            // Simple read to verify connectivity
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
