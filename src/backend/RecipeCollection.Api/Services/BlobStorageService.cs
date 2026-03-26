using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RecipeCollection.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);
    Task<Stream> DownloadImageAsync(string imageRef);
    Task DeleteImageAsync(string imageRef);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        var containerName = configuration["BlobStorage:ContainerName"] ?? "recipe-images";
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _logger = logger;

        // Ensure container exists (dev convenience)
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        var blobName = $"recipes/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}_{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(imageStream, options);
        _logger.LogInformation("Uploaded image to blob: {BlobName}", blobName);

        return blobName; // Return blob path as imageRef
    }

    public async Task<Stream> DownloadImageAsync(string imageRef)
    {
        var blobClient = _containerClient.GetBlobClient(imageRef);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task DeleteImageAsync(string imageRef)
    {
        var blobClient = _containerClient.GetBlobClient(imageRef);
        await blobClient.DeleteIfExistsAsync();
        _logger.LogInformation("Deleted image from blob: {ImageRef}", imageRef);
    }
}
