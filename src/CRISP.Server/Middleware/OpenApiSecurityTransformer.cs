using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using CRISP.Server.Authentication;

namespace CRISP.Server.Middleware;

/// <summary>
/// OpenAPI document transformer that registers both JWT Bearer and API Key
/// security schemes, and applies them as requirements on protected operations.
/// </summary>
public sealed class SecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Adds API security schemes and requirements to the generated OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document being transformed.</param>
    /// <param name="context">The OpenAPI transformation context.</param>
    /// <param name="cancellationToken">A token that cancels the transformation.</param>
    /// <returns>A completed task when the document has been transformed.</returns>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Azure AD / Entra ID JWT Bearer token. Obtain from your identity provider.",
            },
            ["ApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyAuthOptions.HeaderName,
                In = ParameterLocation.Header,
                Description = "API key for service-to-service authentication. Obtain from POST /api/v1/keys.",
            },
        };

        // Apply security requirements to protected operations
        if (document.Paths is null)
            return Task.CompletedTask;

        foreach (var pathItem in document.Paths.Values)
        {
            if (pathItem.Operations is null)
                continue;
            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
                });
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ApiKey", document)] = [],
                });
            }
        }

        return Task.CompletedTask;
    }
}
