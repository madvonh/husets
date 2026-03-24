namespace RecipeCollection.DTOs.ResponseModels;

public class RecipeSummaryResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public required string ImageRef { get; set; }
}