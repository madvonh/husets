namespace RecipeCollection.DTOs.ResponseModels;

public class OcrResponse
{
    public required string ImageRef { get; set; }
    public required string ExtractedText { get; set; }
}