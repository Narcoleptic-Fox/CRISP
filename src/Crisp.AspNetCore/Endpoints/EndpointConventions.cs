using Crisp.Binding;
using Crisp.Commands;
using Crisp.Queries;
using Crisp.Utilities;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Crisp.Endpoints;

/// <summary>
/// Defines conventions for mapping commands and queries to HTTP endpoints.
/// Follows RESTful patterns by default with procedural clarity.
/// </summary>
internal static class EndpointConventions
{
    /// <summary>
    /// Determines the HTTP method based on the request type and name.
    /// </summary>
    internal static string DetermineHttpMethod(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        
        // Check for explicit HttpMethod attribute first
        var httpMethodAttribute = requestType.GetCustomAttribute<Crisp.Metadata.HttpMethodAttribute>();
        if (httpMethodAttribute != null)
            return httpMethodAttribute.Method;
        
        // Queries are always GET
        if (RequestTypeDetector.IsQuery(requestType))
            return "GET";

        // Commands follow naming conventions
        string typeName = requestType.Name;

        return typeName switch
        {
            _ when typeName.StartsWith("Create") => "POST",
            _ when typeName.StartsWith("Update") => "PUT",
            _ when typeName.StartsWith("Delete") => "DELETE",
            _ when typeName.StartsWith("Patch") => "PATCH",
            _ when typeName.StartsWith("Archive") => "PUT",
            _ => "POST" // Default for commands
        };
    }

    /// <summary>
    /// Determines the route pattern based on the request type.
    /// </summary>
    internal static string DetermineRoutePattern(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        
        // Check for explicit Route attribute first
        var routeAttribute = requestType.GetCustomAttribute<Crisp.Metadata.RouteAttribute>();
        if (routeAttribute != null)
            return routeAttribute.Pattern;
        
        string typeName = requestType.Name;

        // Remove common suffixes
        typeName = typeName.Replace("Command", "").Replace("Query", "").Replace("Request", "");

        // Extract resource name and action
        (string action, string resource) = ExtractActionAndResource(typeName);

        // Build route pattern in resource/action format
        string actionLower = action.ToLowerInvariant();
        string resourceLower = resource.ToLowerInvariant();
        
        return action switch
        {
            "Get" => $"/api/{resourceLower}/{actionLower}/{{id}}",
            "Update" => $"/api/{resourceLower}/{actionLower}/{{id}}",
            "Delete" => $"/api/{resourceLower}/{actionLower}/{{id}}",
            "Patch" => $"/api/{resourceLower}/{actionLower}/{{id}}",
            "Create" => $"/api/{resourceLower}/{actionLower}",
            "List" => $"/api/{resourceLower}/{actionLower}",
            _ when string.IsNullOrEmpty(action) => $"/api/{resourceLower}",
            _ => $"/api/{resourceLower}/{actionLower}"
        };
    }


    /// <summary>
    /// Extracts the action and resource from a type name.
    /// Example: "CreateTodoCommand" → ("Create", "Todo")
    /// </summary>
    internal static (string action, string resource) ExtractActionAndResource(string typeName)
    {
        // Common action prefixes
        string[] actions = ["Get", "List", "Create", "Update", "Delete", "Patch", "Find", "Search", "Archive"];

        foreach (string action in actions)
        {
            if (typeName.StartsWith(action))
            {
                string resource = typeName[action.Length..];
                return (action, resource);
            }
        }

        // No recognized action, treat the whole name as resource
        return ("", typeName);
    }

    /// <summary>
    /// Converts PascalCase to kebab-case.
    /// Example: "TodoItem" → "todo-item"
    /// </summary>
    internal static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        System.Text.StringBuilder result = new();

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(value[i - 1]))
                    result.Append('-');

                result.Append(char.ToLower(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    internal static Task<T> BindFromRoute<T>(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        // Use the same binding logic as BindFromRouteAndQuery
        T instance = BindFromRouteAndQuery<T>(context);
        return Task.FromResult(instance);
    }

    internal static T BindFromRouteAndQuery<T>(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        // Create binding context with new type conversion system
        var bindingContext = new ParameterBindingContext
        {
            HttpContext = context,
            TypeConverter = new CompositeTypeConverter(),
            TargetType = typeof(T)
        };

        // Use the new parameter binding system
        var binder = new RouteAndQueryParameterBinder();
        return (T)binder.Bind(bindingContext);
    }


    /// <summary>
    /// Provides a summary for a given request type.
    /// </summary>
    internal static string GetSummary(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        
        string name = requestType.Name;

        // Remove suffixes
        name = name.Replace("Command", "").Replace("Query", "");

        // Extract action and resource
        (string action, string resource) = ExtractActionAndResource(name);

        // Format as "Action Resource"
        if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(resource))
        {
            return $"{action} {resource}";
        }

        return name;
    }

    internal static string ExtractTag(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        
        // Extract the resource name for OpenAPI grouping
        string name = type.Name;
        
        // Handle generic types (remove `1, `2, etc.)
        int backtickIndex = name.IndexOf('`');
        if (backtickIndex >= 0)
        {
            name = name[..backtickIndex];
        }
        
        name = name.Replace("Command", "").Replace("Query", "");

        // Find the resource name (e.g., "CreateTodo" → "Todo")
        string[] prefixes = ["Create", "Update", "Delete", "Get", "List"];
        foreach (string prefix in prefixes)
        {
            if (name.StartsWith(prefix))
                return name[prefix.Length..];
        }

        return name;
    }
}