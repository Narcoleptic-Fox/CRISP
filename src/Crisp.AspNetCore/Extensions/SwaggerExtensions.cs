using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Crisp.OpenApi;

/// <summary>
/// Extensions for integrating CRISP with Swagger/OpenAPI.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds CRISP-aware Swagger generation.
    /// </summary>
    public static IServiceCollection AddCrispSwagger(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? configure = null)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Add CRISP documentation
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CRISP API",
                Version = "v1",
                Description = "API built with the CRISP pattern"
            });

            // Add CRISP filters
            options.DocumentFilter<CrispOpenApiGenerator>();
            options.OperationFilter<CrispOpenApiGenerator>();

            // Add Problem Details schema
            options.SchemaFilter<ProblemDetailsSchemaFilter>();

            // Support for nullable reference types
            options.SupportNonNullableReferenceTypes();

            // Custom schema IDs to avoid conflicts
            options.CustomSchemaIds(type =>
            {
                string name = type.Name;

                // Handle generic types
                if (type.IsGenericType)
                {
                    string genericArgs = string.Join("", type.GetGenericArguments().Select(t => t.Name));
                    name = $"{name.Split('`')[0]}Of{genericArgs}";
                }

                return name;
            });

            // Include XML comments if available
            string[] xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (string xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile);
            }

            // Apply custom configuration
            configure?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Maps Swagger UI for CRISP APIs.
    /// </summary>
    public static IApplicationBuilder UseCrispSwagger(
        this IApplicationBuilder app,
        Action<SwaggerOptions>? configureSwagger = null,
        Action<SwaggerUIOptions>? configureUI = null)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
            configureSwagger?.Invoke(options);
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRISP API v1");
            options.RoutePrefix = "swagger";

            // Enhanced UI settings
            options.DefaultModelsExpandDepth(-1); // Hide schemas by default
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableTryItOutByDefault();

            configureUI?.Invoke(options);
        });

        return app;
    }
}

/// <summary>
/// Schema filter for Problem Details.
/// </summary>
internal class ProblemDetailsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Name == "ProblemDetails")
        {
            schema.Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new() { Type = "string", Description = "A URI reference that identifies the problem type" },
                ["title"] = new() { Type = "string", Description = "A short, human-readable summary of the problem type" },
                ["status"] = new() { Type = "integer", Description = "The HTTP status code" },
                ["detail"] = new() { Type = "string", Description = "A human-readable explanation specific to this occurrence" },
                ["instance"] = new() { Type = "string", Description = "A URI reference that identifies the specific occurrence" },
                ["errors"] = new() { Type = "object", Description = "Additional error details", AdditionalProperties = new OpenApiSchema() }
            };
        }
    }
}