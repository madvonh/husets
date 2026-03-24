namespace RecipeCollection.DTOs.ResponseModels;

public class SearchRecipesResponse
{
    public List<RecipeSummaryResponse> Recipes { get; set; } = new();
}
