namespace RecipeCollection.Extensions;

public static class ServiceCollectionSecurityExtensions
{
    /// <summary>
    /// Configures CORS policies for the application. If allowedOrigins is empty or contains "*", it allows requests from any origin. Otherwise, it restricts access to the specified origins. In both cases, it allows any header and method, and exposes the "X-Correlation-Id" header to clients.
    /// </summary>
    /// <param name="services">The service collection to add CORS policies to.</param>
    /// <param name="allowedOrigins">The list of allowed origins for CORS. If empty or contains "*", all origins are allowed.</param>
    public static void ConfigureCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Correlation-Id");
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Correlation-Id");
                }
            });
        });
    }
}