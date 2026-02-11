using System.Net;
using System.Text.Json;
using RecipeApi.Models;
using FluentValidation;

namespace RecipeApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        _logger.LogWarning(exception, "Validation error for correlation ID {CorrelationId}", correlationId);

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errors = string.Join("; ", exception.Errors.Select(e => e.ErrorMessage));

        var errorResponse = new ErrorResponse
        {
            Code = "VALIDATION_ERROR",
            Message = errors,
            CorrelationId = correlationId
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        _logger.LogError(exception, "Unhandled exception for correlation ID {CorrelationId}", correlationId);

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Code = "INTERNAL_ERROR",
            Message = "An internal server error occurred. Please try again later.",
            CorrelationId = correlationId
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }
}
