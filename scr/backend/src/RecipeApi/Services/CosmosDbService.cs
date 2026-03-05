using Microsoft.Azure.Cosmos;

namespace RecipeApi.Services;

public interface ICosmosDbService
{
    Task<T> CreateItemAsync<T>(T item, string partitionKey);
    Task<T?> GetItemAsync<T>(string id, string partitionKey);
    Task<List<T>> QueryItemsAsync<T>(string query, Dictionary<string, object>? parameters = null);
    Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey);
    Task DeleteItemAsync(string id, string partitionKey);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;
    private const int MaxRetries = 3;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<CosmosDbService> logger)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? throw new InvalidOperationException("CosmosDb:DatabaseName not configured");
        var containerName = configuration["CosmosDb:ContainerName"] ?? throw new InvalidOperationException("CosmosDb:ContainerName not configured");

        _container = cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<T> CreateItemAsync<T>(T item, string partitionKey)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
            return response.Resource;
        }, "CreateItem");
    }

    public async Task<T?> GetItemAsync<T>(string id, string partitionKey)
    {
        try
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }, "GetItem");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<List<T>> QueryItemsAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var queryDefinition = new QueryDefinition(query);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    queryDefinition.WithParameter(param.Key, param.Value);
                }
            }

            var results = new List<T>();
            using var iterator = _container.GetItemQueryIterator<T>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }, "QueryItems");
    }

    public async Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _container.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
            return response.Resource;
        }, "UpdateItem");
    }

    public async Task DeleteItemAsync(string id, string partitionKey)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await _container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
            return true;
        }, "DeleteItem");
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        int retryCount = 0;
        
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (CosmosException ex) when (IsTransientError(ex) && retryCount < MaxRetries)
            {
                retryCount++;
                var delayMs = CalculateRetryDelay(retryCount, ex);
                
                _logger.LogWarning(
                    "Transient error during {OperationName} (StatusCode: {StatusCode}). Retry {RetryCount}/{MaxRetries} after {DelayMs}ms",
                    operationName, ex.StatusCode, retryCount, MaxRetries, delayMs);
                
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Rate limit exceeded - use server-provided retry-after or default
                var delayMs = ex.RetryAfter?.TotalMilliseconds ?? 1000;
                _logger.LogWarning(
                    "Rate limit exceeded during {OperationName}. Retrying after {DelayMs}ms",
                    operationName, delayMs);
                
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
                
                // After waiting, retry once
                return await operation();
            }
        }
    }

    private bool IsTransientError(CosmosException ex)
    {
        // Transient errors that should be retried
        return ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
               ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
               ex.StatusCode == System.Net.HttpStatusCode.InternalServerError;
    }

    private int CalculateRetryDelay(int retryCount, CosmosException ex)
    {
        // Exponential backoff: 100ms, 200ms, 400ms
        var baseDelay = 100;
        var exponentialDelay = baseDelay * Math.Pow(2, retryCount - 1);
        
        // Add jitter to avoid thundering herd
        var jitter = Random.Shared.Next(0, 50);
        
        return (int)exponentialDelay + jitter;
    }
}
