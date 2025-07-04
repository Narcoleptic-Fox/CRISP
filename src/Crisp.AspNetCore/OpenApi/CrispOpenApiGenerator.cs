using Crisp.Endpoints;
using Crisp.Metadata;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Crisp.OpenApi;

/// <summary>
/// Generates OpenAPI documentation for CRISP endpoints.
/// Follows the procedural flow: Discover → Analyze → Generate → Document
/// <param name="endpointMapper">The endpoint mapper used to discover and map endpoints.</param>
/// </summary>
public class CrispOpenApiGenerator(EndpointMapper endpointMapper) : IDocumentFilter, IOperationFilter
{
    private readonly EndpointMapper _endpointMapper = endpointMapper;

    /// <summary>
    /// Applies CRISP-specific documentation to the OpenAPI document.
    /// </summary>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add CRISP-specific tags and descriptions
        swaggerDoc.Tags ??= [];

        // Group endpoints by resource
        var resourceTags = _endpointMapper.GetEndpoints()
            .Select(e => EndpointMetadata.FromRequestType(e.RequestType))
            .GroupBy(m => m.Tag)
            .Select(g => new OpenApiTag
            {
                Name = g.Key,
                Description = $"Operations for {g.Key} resources"
            });

        foreach (var tag in resourceTags)
        {
            if (!swaggerDoc.Tags.Any(t => t.Name == tag.Name))
            {
                swaggerDoc.Tags.Add(tag);
            }
        }

        // Add CRISP info to description
        swaggerDoc.Info.Description = $"""
            {swaggerDoc.Info.Description}
            
            ---
            
            This API uses the CRISP pattern for command and query handling.
            
            - **Commands**: Operations that change state (POST, PUT, DELETE, PATCH)
            - **Queries**: Operations that retrieve data (GET)
            
            All endpoints follow RESTful conventions and return appropriate HTTP status codes.
            """;
    }

    /// <summary>
    /// Enhances operation documentation with CRISP-specific information.
    /// </summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if this is a CRISP endpoint
        EndpointMetadata? endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<EndpointMetadata>()
            .FirstOrDefault();

        if (endpointMetadata == null)
            return;

        // Enhance operation documentation
        EnhanceOperationDocumentation(operation, endpointMetadata);

        // Add request/response examples
        AddExamples(operation, endpointMetadata);

        // Add error responses
        AddErrorResponses(operation, endpointMetadata);
    }

    private void EnhanceOperationDocumentation(OpenApiOperation operation, EndpointMetadata metadata)
    {
        // Set operation ID based on request type
        operation.OperationId = metadata.RequestType.Name;

        // Add summary from XML docs or generate one
        if (string.IsNullOrEmpty(operation.Summary))
        {
            operation.Summary = GenerateSummary(metadata);
        }

        // Add description
        if (string.IsNullOrEmpty(operation.Description))
        {
            operation.Description = GenerateDescription(metadata);
        }

        // Add tags
        operation.Tags = [new OpenApiTag { Name = metadata.Tag }];
    }

    private string GenerateSummary(EndpointMetadata metadata)
    {
        string typeName = metadata.RequestType.Name;
        string action = ExtractAction(typeName);
        string? resource = metadata.Tag;

        return action switch
        {
            "Create" => $"Create a new {resource}",
            "Update" => $"Update an existing {resource}",
            "Delete" => $"Delete a {resource}",
            "Get" => $"Get a {resource} by ID",
            "List" => $"Get a list of {resource}s",
            "Archive" => $"Archive a {resource}",
            _ => $"Perform {action} operation on {resource}"
        };
    }

    private string GenerateDescription(EndpointMetadata metadata)
    {
        Type requestType = metadata.RequestType;
        Type? responseType = metadata.ResponseType;

        string description = $"**Request Type**: `{requestType.Name}`\n\n";

        if (responseType != null)
        {
            description += $"**Response Type**: `{responseType.Name}`\n\n";
        }

        // Add property documentation
        PropertyInfo[] properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (properties.Any())
        {
            description += "**Request Properties**:\n";
            foreach (PropertyInfo prop in properties)
            {
                description += $"- `{prop.Name}` ({prop.PropertyType.Name}): {GetPropertyDescription(prop)}\n";
            }
        }

        return description;
    }

    private void AddExamples(OpenApiOperation operation, EndpointMetadata metadata)
    {
        // Add request example
        if (metadata.HttpMethod != "GET" && metadata.HttpMethod != "DELETE")
        {
            OpenApiString? example = GenerateExample(metadata.RequestType);
            if (example != null)
            {
                operation.RequestBody ??= new OpenApiRequestBody();
                operation.RequestBody.Content["application/json"].Example = example;
            }
        }

        // Add response example
        if (metadata.ResponseType != null)
        {
            OpenApiString? example = GenerateExample(metadata.ResponseType);
            if (example != null && operation.Responses.TryGetValue("200", out OpenApiResponse? response))
            {
                response.Content["application/json"].Example = example;
            }
        }
    }

    private void AddErrorResponses(OpenApiOperation operation, EndpointMetadata metadata)
    {
        // Add common error responses
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Validation failed or invalid input",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "ProblemDetails"
                            }
                        }
                    }
                }
            });
        }

        if (metadata.HttpMethod == "GET" || metadata.HttpMethod == "PUT" ||
            metadata.HttpMethod == "DELETE" || metadata.HttpMethod == "PATCH")
        {
            operation.Responses.Add("404", new OpenApiResponse
            {
                Description = "Not Found - Resource does not exist",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "ProblemDetails"
                            }
                        }
                    }
                }
            });
        }
    }

    private string ExtractAction(string typeName)
    {
        string[] actions = ["Create", "Update", "Delete", "Get", "List", "Archive", "Find", "Search"];

        foreach (string action in actions)
        {
            if (typeName.StartsWith(action))
                return action;
        }

        return "Execute";
    }

    private string GetPropertyDescription(PropertyInfo property) =>
        // Could read from XML docs or attributes
        property.PropertyType switch
        {
            Type t when t == typeof(int) => "Integer value",
            Type t when t == typeof(string) => "Text value",
            Type t when t == typeof(bool) => "Boolean value",
            Type t when t == typeof(DateTime) => "Date and time value",
            Type t when t == typeof(Guid) => "Unique identifier",
            _ => "Value"
        };

    private OpenApiString? GenerateExample(Type type) =>
        // Generate a simple example based on type
        // In a real implementation, this could be more sophisticated
        null;
}
