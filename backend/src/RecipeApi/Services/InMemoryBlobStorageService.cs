using System.Collections.Concurrent;

namespace RecipeApi.Services;

/// <summary>
/// In-memory implementation of IBlobStorageService for testing and development
/// </summary>
public class InMemoryBlobStorageService : IBlobStorageService
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();
    private readonly ILogger<InMemoryBlobStorageService> _logger;

    public InMemoryBlobStorageService(ILogger<InMemoryBlobStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        var blobName = $"recipes/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}_{fileName}";
        
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        _storage[blobName] = memoryStream.ToArray();
        
        _logger.LogInformation("Uploaded image to in-memory blob: {BlobName} (size: {Size} bytes)", 
            blobName, memoryStream.Length);

        return blobName;
    }

    public Task<Stream> DownloadImageAsync(string imageRef)
    {
        if (_storage.TryGetValue(imageRef, out var data))
        {
            _logger.LogInformation("Downloaded image from in-memory blob: {ImageRef}", imageRef);
            return Task.FromResult<Stream>(new MemoryStream(data));
        }

        throw new FileNotFoundException($"Image not found: {imageRef}");
    }

    public Task DeleteImageAsync(string imageRef)
    {
        _storage.TryRemove(imageRef, out _);
        _logger.LogInformation("Deleted image from in-memory blob: {ImageRef}", imageRef);
        return Task.CompletedTask;
    }
}
