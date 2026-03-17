using RecipeApi.Services;

namespace RecipeApi.Extensions;

public static class ServiceCollectionAIServicesExtensions
{
    /// <summary>
    /// Configures the OCR service based on the presence of Azure Vision credentials. If credentials are provided, it registers the AzureOcrService; otherwise, it falls back to a local TesseractOcrService implementation.
    /// </summary>
    /// <param name="services">The service collection to add the OCR service to.</param>
    /// <param name="azureVisionEndpoint">The Azure Vision endpoint URL.</param>
    /// <param name="azureVisionApiKey">The Azure Vision API key.</param>
    public static void ConfigureOcrService(this IServiceCollection services, string? azureVisionEndpoint, string? azureVisionApiKey)
    {

        if (!string.IsNullOrEmpty(azureVisionEndpoint) && !string.IsNullOrEmpty(azureVisionApiKey))
        {
            services.AddSingleton<IOcrService, AzureOcrService>();
        }
        else
        {
            // Fallback: use Tesseract for local OCR without Azure
            services.AddSingleton<IOcrService, TesseractOcrService>();
        }
    }
}