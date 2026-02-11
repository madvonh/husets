namespace RecipeApi.Models;

public class ErrorResponse
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public string? CorrelationId { get; set; }
}
