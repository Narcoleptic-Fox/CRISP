namespace Crisp.Metadata;

/// <summary>
/// Specifies the HTTP method for a command.
/// Overrides the default convention-based HTTP method.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpMethodAttribute : Attribute
{
    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMethodAttribute"/> class with the specified HTTP method.
    /// </summary>
    /// <param name="method">The HTTP method to associate with the command.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
    public HttpMethodAttribute(string method) => Method = method ?? throw new ArgumentNullException(nameof(method));
}
