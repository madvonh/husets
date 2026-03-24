namespace RecipeCollection.DTOs.RequestModels;

public class OcrRequest
{
    public required IFormFile Image { get; set; }
}