using Newtonsoft.Json;

namespace RecipeCollection.Domain;

public class RecipeIngredient 
{
    [JsonProperty("id")]
    public required string Id { get; set; }

    [JsonProperty("pk")]
    public string Pk { get; set; } = "recipe";

    [JsonProperty("type")]
    public string Type { get; set; } = "RecipeIngredient";

    [JsonProperty("recipeId")]
    public required string RecipeId { get; set; }

    [JsonProperty("freeText")]
    public required string FreeText { get; set; }

    [JsonProperty("canonicalName")]
    public string? CanonicalName { get; set; }

    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}