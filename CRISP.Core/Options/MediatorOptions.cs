namespace CRISP.Core.Options;

/// <summary>
/// Options for configuring the mediator behavior.
/// </summary>
public class MediatorOptions
{
    /// <summary>
    /// Gets or sets whether to allow multiple handlers for a single request.
    /// Default is false which means an exception is thrown if multiple handlers are registered for the same request type.
    /// </summary>
    public bool AllowMultipleHandlers { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically apply validation to requests.
    /// Default is true which means all requests will be validated if a validator is registered.
    /// </summary>
    public bool AutoValidateRequests { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log detailed information about request processing.
    /// Default is true.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// Default is 60 seconds. A value of 0 or negative means no timeout.
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Gets or sets a value indicating whether to track request metrics.
    /// Default is true.
    /// </summary>
    public bool TrackRequestMetrics { get; set; } = true;
}