namespace RecipeApi.DTOs;

public class SearchRecipesResponse
{
    public List<RecipeSummaryResponse> Recipes { get; set; } = new();
}
