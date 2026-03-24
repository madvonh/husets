namespace RecipeCollection.DTOs.RequestModels;

public class CreateRecipeRequest
{
    public required string Title { get; set; }
    public required string RawText { get; set; }
    public required string ImageRef { get; set; }
    public List<string>? Tags { get; set; }
}