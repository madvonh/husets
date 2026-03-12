using Microsoft.Azure.Cosmos;

namespace RecipeApi.Repositories;

public class CosmosDbInitializer : IHostedService
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbInitializer> _logger;

    public bool IsInitialized { get; private set; }
    public Exception? InitializationException { get; private set; }

    public CosmosDbInitializer(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<CosmosDbInitializer> logger)
    {
        _cosmosClient = cosmosClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? databaseName = null;
        string? containerName = null;
        const string partitionKeyPath = "/pk";

        try
        {
            databaseName = _configuration["CosmosDb:DatabaseName"]
                ?? throw new InvalidOperationException("CosmosDb:DatabaseName not configured");
            containerName = _configuration["CosmosDb:ContainerName"]
                ?? throw new InvalidOperationException("CosmosDb:ContainerName not configured");
            _logger.LogInformation(
                "Initializing Cosmos DB: database '{DatabaseName}', container '{ContainerName}'",
                databaseName, containerName);

            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                databaseName, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Database '{DatabaseName}': {StatusCode}",
                databaseName, databaseResponse.StatusCode);

            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                containerName, partitionKeyPath, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Container '{ContainerName}': {StatusCode}",
                containerName, containerResponse.StatusCode);

            IsInitialized = true;
            _logger.LogInformation("Cosmos DB initialization completed successfully");
        }
        catch (Exception ex) when (ex is CosmosException or HttpRequestException or InvalidOperationException)
        {
            InitializationException = ex;
            _logger.LogError(ex,
                "Cosmos DB initialization failed. The API will start but Cosmos operations will fail until connectivity is restored. " +
                "Database: '{DatabaseName}', Container: '{ContainerName}'",
                databaseName, containerName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
