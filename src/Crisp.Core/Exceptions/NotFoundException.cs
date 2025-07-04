namespace Crisp.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : CrispException
{
    /// <summary>
    /// Gets or sets the type of the resource that was not found.
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the resource that was not found.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException() : base("The requested resource was not found. Please verify the resource identifier and try again.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class for a specific entity and identifier.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="id">The identifier that was searched for.</param>
    public NotFoundException(object id, string entityName)
        : base($"The {entityName} with identifier '{id}' was not found. Please verify the {entityName.ToLower()} ID exists and try again.")
    {
        ResourceType = entityName;
        ResourceId = id?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class for a specific entity and identifier with suggestions.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="id">The identifier that was searched for.</param>
    /// <param name="suggestions">Additional suggestions for resolving the issue.</param>
    public NotFoundException(object id, string entityName, string suggestions)
        : base($"The {entityName} with identifier '{id}' was not found. {suggestions}")
    {
        ResourceType = entityName;
        ResourceId = id?.ToString() ?? string.Empty;
    }
    public NotFoundException(object id, string entityName, string suggestions, Exception? inner = null)
        : base($"The {entityName} with identifier '{id}' was not found. {suggestions}", inner)
    {
        ResourceType = entityName;
        ResourceId = id?.ToString() ?? string.Empty;
    }

}
