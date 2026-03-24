namespace RecipeCollection.DTOs.ResponseModels;

public class RecipeDetailResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string RawText { get; set; }
    public required string ImageRef { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<IngredientDto> Ingredients { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}