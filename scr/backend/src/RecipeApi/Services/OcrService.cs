using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Tesseract;

namespace RecipeApi.Services;

public interface IOcrService
{
    Task<string> ExtractTextFromImageAsync(Stream imageStream);
}

public class AzureOcrService : IOcrService
{
    private readonly ImageAnalysisClient? _client;
    private readonly ILogger<AzureOcrService> _logger;

    public AzureOcrService(IConfiguration configuration, ILogger<AzureOcrService> logger)
    {
        _logger = logger;

        var endpoint = configuration["AzureVision:Endpoint"];
        var apiKey = configuration["AzureVision:ApiKey"];

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            _client = new ImageAnalysisClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));
        }
        else
        {
            _logger.LogWarning("Azure Vision not configured. OCR will return placeholder text.");
        }
    }

    public async Task<string> ExtractTextFromImageAsync(Stream imageStream)
    {
        if (_client == null)
        {
            // Return placeholder for dev/testing when Azure Vision not configured
            _logger.LogWarning("OCR called without Azure Vision configured - returning placeholder");
            return "Placeholder OCR text:\n\nChocolate Chip Cookies\n\nIngredients:\n- 2 cups flour\n- 1 cup sugar\n- 1 cup butter\n- 2 eggs\n- 1 tsp vanilla\n- 1 cup chocolate chips\n\nInstructions:\nMix dry ingredients. Cream butter and sugar. Add eggs and vanilla. Combine. Fold in chocolate chips. Bake at 350°F for 12 minutes.";
        }

        try
        {
            var binaryData = await BinaryData.FromStreamAsync(imageStream);

            var result = await _client.AnalyzeAsync(
                binaryData,
                VisualFeatures.Read);

            if (result.Value.Read?.Blocks == null || result.Value.Read.Blocks.Count == 0)
            {
                _logger.LogWarning("No text found in image");
                return string.Empty;
            }

            // Extract all text lines
            var extractedText = new System.Text.StringBuilder();
            foreach (var block in result.Value.Read.Blocks)
            {
                foreach (var line in block.Lines)
                {
                    extractedText.AppendLine(line.Text);
                }
            }

            return extractedText.ToString().Trim();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Vision OCR request failed");
            throw new InvalidOperationException("OCR processing failed. Please try again.", ex);
        }
    }
}

public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessDataPath;
    private readonly string _language;

    public TesseractOcrService(IConfiguration configuration, ILogger<TesseractOcrService> logger)
    {
        _logger = logger;
        _tessDataPath = configuration["Tesseract:DataPath"] ?? Path.Combine(AppContext.BaseDirectory, "tessdata");
        _language = configuration["Tesseract:Language"] ?? "eng";

        if (string.IsNullOrWhiteSpace(_language))
        {
            _language = "eng";
        }

        if (!Directory.Exists(_tessDataPath))
        {
            _logger.LogWarning($"Tesseract data path not found: {_tessDataPath}. OCR will return placeholder text.");
        }
    }

    public async Task<string> ExtractTextFromImageAsync(Stream imageStream)
    {
        if (!Directory.Exists(_tessDataPath))
        {
            _logger.LogWarning("Tesseract data not available - returning placeholder");
            return "Placeholder OCR text:\n\nChocolate Chip Cookies\n\nIngredients:\n- 2 cups flour\n- 1 cup sugar\n- 1 cup butter\n- 2 eggs\n- 1 tsp vanilla\n- 1 cup chocolate chips\n\nInstructions:\nMix dry ingredients. Cream butter and sugar. Add eggs and vanilla. Combine. Fold in chocolate chips. Bake at 350°F for 12 minutes.";
        }

        try
        {
            // Ensure stream is at the beginning
            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            // Read stream into byte array
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // Perform OCR
            using var engine = new TesseractEngine(_tessDataPath, _language, EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            
            var text = page.GetText().Trim();
            
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("No text found in image");
                return string.Empty;
            }

            _logger.LogInformation($"Extracted {text.Length} characters from image");
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tesseract OCR processing failed");
            throw new InvalidOperationException("OCR processing failed. Please try again.", ex);
        }
    }
}
