namespace RecipeApi.Models.DTOs;

public class OcrRequest
{
    public required IFormFile Image { get; set; }
}

public class OcrResponse
{
    public required string ImageRef { get; set; }
    public required string ExtractedText { get; set; }
}

public class CreateRecipeRequest
{
    public required string Title { get; set; }
    public required string RawText { get; set; }
    public required string ImageRef { get; set; }
    public List<string>? Tags { get; set; }
}

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

public class RecipeSummaryResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public required string ImageRef { get; set; }
}

public class IngredientDto
{
    public required string FreeText { get; set; }
    public string? CanonicalName { get; set; }
    public int Position { get; set; }
}

public class AddTagRequest
{
    public required string Tag { get; set; }
}

public class SearchRecipesResponse
{
    public List<RecipeSummaryResponse> Recipes { get; set; } = new();
}
