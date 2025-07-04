namespace Crisp.Metadata;

/// <summary>
/// Specifies a custom route pattern for a command or query.
/// Overrides the default convention-based routing.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RouteAttribute : Attribute
{
    /// <summary>
    /// Gets the route pattern.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteAttribute"/> class with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern to associate with the command or query.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    public RouteAttribute(string pattern) => Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
}
