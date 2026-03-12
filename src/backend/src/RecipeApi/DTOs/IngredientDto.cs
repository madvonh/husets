namespace RecipeApi.DTOs;

public class IngredientDto
{
    public required string FreeText { get; set; }
    public string? CanonicalName { get; set; }
    public int Position { get; set; }
}