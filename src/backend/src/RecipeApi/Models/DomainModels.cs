namespace RecipeApi.Models;

public class Recipe
{
    public required string Id { get; set; }
    public required string Pk { get; set; } = "recipe";
    public string Type { get; set; } = "Recipe";
    public required string Title { get; set; }
    public required string RawText { get; set; }
    public required string ImageRef { get; set; }
    public required string SearchText { get; set; }
    public List<string> NormalizedTags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class RecipeIngredient
{
    public required string Id { get; set; }
    public required string Pk { get; set; } = "recipe";
    public string Type { get; set; } = "RecipeIngredient";
    public required string RecipeId { get; set; }
    public required string FreeText { get; set; }
    public string? CanonicalName { get; set; }
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
