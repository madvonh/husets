using System.Reflection;
using Microsoft.OpenApi;
public static class ServiceCollectionSwaggerExtensions
{
    /// <summary>
    /// Configures Swagger generation for the API.
    /// </summary>
    /// <param name="services">The service collection to add Swagger services to.</param>
    public static void ConfigureSwaggerGen(this IServiceCollection services)
    {
        var xmlDocFile = Path.Combine(
            AppContext.BaseDirectory,
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Recipe Collection API",
                Version = "v1",
                Description = "API for managing recipes with OCR support"
            });

            if (File.Exists(xmlDocFile))
            {
                options.IncludeXmlComments(xmlDocFile);
            }
        });
    }
    
    /// <summary>
    /// Configures the Swagger UI middleware for the API.   
    /// </summary>
    /// <param name="app">The application builder to configure the Swagger UI middleware.</param>
    public static void UseOptionsSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Collection API v1");
            options.RoutePrefix = "swagger";
        });
    }
}