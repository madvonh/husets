using RecipeApi.Models;

namespace RecipeApi.Services;

public interface IIngredientParser
{
    List<RecipeIngredient> ParseIngredients(string rawText, string recipeId);
}

public class IngredientParser : IIngredientParser
{
    private readonly ILogger<IngredientParser> _logger;

    public IngredientParser(ILogger<IngredientParser> logger)
    {
        _logger = logger;
    }

    public List<RecipeIngredient> ParseIngredients(string rawText, string recipeId)
    {
        var ingredients = new List<RecipeIngredient>();

        // Simple heuristic: look for lines starting with - or • or numbers
        // This is MVP-level parsing; production would use ML
        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int position = 0;

        bool inIngredientsSection = false;

        foreach (var line in lines)
        {
            // Detect ingredients section
            if (line.Contains("ingredient", StringComparison.OrdinalIgnoreCase))
            {
                inIngredientsSection = true;
                continue;
            }

            // Stop when hitting instructions or method section
            if (line.Contains("instruction", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("method", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("directions", StringComparison.OrdinalIgnoreCase))
            {
                inIngredientsSection = false;
                break;
            }

            if (inIngredientsSection)
            {
                // Check if line looks like an ingredient (starts with -, •, or digit)
                var trimmed = line.Trim();
                if (trimmed.StartsWith("-") || trimmed.StartsWith("•") ||
                    trimmed.StartsWith("*") || char.IsDigit(trimmed[0]))
                {
                    var freeText = trimmed.TrimStart('-', '•', '*').Trim();
                    var canonicalName = ExtractCanonicalName(freeText);

                    ingredients.Add(new RecipeIngredient
                    {
                        Id = $"recipeingredient_{Guid.NewGuid()}",
                        Pk = "recipe",
                        RecipeId = recipeId,
                        FreeText = freeText,
                        CanonicalName = canonicalName,
                        Position = position++
                    });
                }
            }
        }

        _logger.LogInformation("Parsed {Count} ingredients from recipe text", ingredients.Count);
        return ingredients;
    }

    private string? ExtractCanonicalName(string freeText)
    {
        // Best-effort extraction: take last "word" that isn't a number or unit
        var words = freeText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var units = new[] { "cup", "cups", "tsp", "tbsp", "teaspoon", "tablespoon", "oz", "lb", "g", "kg", "ml", "l" };

        for (int i = words.Length - 1; i >= 0; i--)
        {
            var word = words[i].ToLower().Trim(',', '.', ';');
            if (!double.TryParse(word, out _) && !units.Contains(word) && word.Length > 2)
            {
                return word;
            }
        }

        return null;
    }
}
