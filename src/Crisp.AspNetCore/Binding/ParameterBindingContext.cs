using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Provides context information for parameter binding operations.
/// </summary>
internal class ParameterBindingContext
{
    /// <summary>
    /// The HTTP context containing request information.
    /// </summary>
    public required HttpContext HttpContext { get; init; }

    /// <summary>
    /// The type converter to use for value conversions.
    /// </summary>
    public required ITypeConverter TypeConverter { get; init; }

    /// <summary>
    /// The target type to bind to.
    /// </summary>
    public required Type TargetType { get; init; }

    /// <summary>
    /// Gets route values from the HTTP context.
    /// </summary>
    public IReadOnlyDictionary<string, object?> RouteValues => 
        HttpContext.Request.RouteValues.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value, 
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets query parameters from the HTTP context.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> QueryParameters =>
        HttpContext.Request.Query.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all available parameter values (route + query).
    /// </summary>
    public IReadOnlyDictionary<string, object?> AllValues
    {
        get
        {
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            // Add route values first
            foreach (var kvp in RouteValues)
            {
                values[kvp.Key] = kvp.Value;
            }

            // Add query values (query takes precedence over route)
            foreach (var kvp in QueryParameters)
            {
                values[kvp.Key] = kvp.Value.Length == 1 ? kvp.Value[0] : kvp.Value;
            }

            return values;
        }
    }
}