namespace Crisp;

/// <summary>
/// Core configuration options for CRISP framework.
/// Lean and focused on essential patterns only.
/// </summary>
public class CrispOptions
{
    /// <summary>
    /// Endpoint configuration options.
    /// </summary>
    public EndpointOptions Endpoints { get; set; } = new();

    /// <summary>
    /// Pipeline configuration options.
    /// </summary>
    public PipelineOptions Pipeline { get; set; } = new();

    /// <summary>
    /// Basic serialization options.
    /// </summary>
    public SerializationOptions Serialization { get; set; } = new();
}

/// <summary>
/// Endpoint-specific options.
/// </summary>
public class EndpointOptions
{
    /// <summary>
    /// The base route prefix for all endpoints. Default is "/api".
    /// </summary>
    public string RoutePrefix { get; set; } = "/api";

    /// <summary>
    /// Whether to use kebab-case routes. Default is true.
    /// </summary>
    public bool UseKebabCase { get; set; } = true;

    /// <summary>
    /// Whether to automatically discover and map endpoints. Default is true.
    /// </summary>
    public bool AutoDiscoverEndpoints { get; set; } = true;

    /// <summary>
    /// Whether to generate OpenAPI documentation. Default is true.
    /// </summary>
    public bool EnableOpenApi { get; set; } = true;

    /// <summary>
    /// Alias for EnableOpenApi for backward compatibility.
    /// </summary>
    public bool EnableSwagger
    {
        get => EnableOpenApi;
        set => EnableOpenApi = value;
    }

    /// <summary>
    /// Whether to require authorization by default. Default is false.
    /// </summary>
    public bool RequireAuthorization { get; set; } = false;
}

/// <summary>
/// Pipeline-specific options.
/// </summary>
public class PipelineOptions
{
    /// <summary>
    /// Whether to enable logging behavior. Default is true.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable error handling behavior. Default is true.
    /// </summary>
    public bool EnableErrorHandling { get; set; } = true;
}

/// <summary>
/// Basic serialization options.
/// </summary>
public class SerializationOptions
{
    /// <summary>
    /// Whether to use camel case property names. Default is true.
    /// </summary>
    public bool UseCamelCase { get; set; } = true;

    /// <summary>
    /// Whether to write indented JSON. Default is false.
    /// </summary>
    public bool WriteIndented { get; set; } = false;

    /// <summary>
    /// Whether to ignore null values. Default is true.
    /// </summary>
    public bool IgnoreNullValues { get; set; } = true;
}
