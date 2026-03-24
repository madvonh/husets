using Newtonsoft.Json;

namespace RecipeCollection.Domain;

public class Recipe 
{
    [JsonProperty("id")]
    public required string Id { get; set; }

    [JsonProperty("pk")]
    public string Pk { get; set; } = "recipe";

    [JsonProperty("type")]
    public string Type { get; set; } = "Recipe";

    [JsonProperty("title")]
    public required string Title { get; set; }

    [JsonProperty("rawText")]
    public required string RawText { get; set; }

    [JsonProperty("imageRef")]
    public required string ImageRef { get; set; }

    [JsonProperty("searchText")]
    public required string SearchText { get; set; }

    [JsonProperty("normalizedTags")]
    public List<string> NormalizedTags { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}