using System.Collections.Concurrent;
using System.Reflection;

namespace CRISP.Server.Middleware;

/// <summary>
/// Copies identifiers from the route onto a bound command before it is validated.
/// </summary>
/// <remarks>
/// <para>
/// Minimal APIs bind the body and the route separately, so a command posted to
/// <c>/Items/{id}</c> arrives with its <c>Id</c> still empty — the handler only fills it
/// afterwards, with <c>command with { Id = id }</c>. Validation runs in between, so a caller that
/// did not also repeat the id in the body was rejected for omitting something the URL already
/// said. Every client had to send it twice, and forgetting produced a confusing 400.
/// </para>
/// <para>
/// The route wins outright rather than only filling a blank. Handlers already overwrite from the
/// route, so the route was always the authority; matching that here means the value validated is
/// the value used, instead of a body id passing validation and then being silently replaced.
/// </para>
/// <para>
/// Restricted to <see cref="Guid"/> properties whose name matches a route value. That covers every
/// identifier in the app while leaving alone things like an <c>int</c> quantity, which is real
/// data rather than addressing.
/// </para>
/// </remarks>
public static class RouteIdBinder
{
    // Reflection per request would be wasteful on a filter that runs for every endpoint.
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> IdProperties = new();

    /// <summary>
    /// Fills any Guid property of <paramref name="argument"/> that shares its name with a route value.
    /// </summary>
    /// <param name="argument">The bound command or query to populate.</param>
    /// <param name="routeValues">The current request's route values.</param>
    public static void Bind(object argument, IReadOnlyDictionary<string, object?> routeValues)
    {
        if (routeValues.Count == 0)
            return;

        foreach (var property in IdProperties.GetOrAdd(argument.GetType(), FindIdProperties))
        {
            if (!TryGetRouteValue(routeValues, property.Name, out var raw))
                continue;

            if (!Guid.TryParse(raw, out var id) || id == Guid.Empty)
                continue;

            // Records declare these init-only, which is a compile-time restriction: reflection can
            // still set them, so the freshly bound instance is populated in place rather than
            // rebuilt (a `with` expression would need the concrete type at compile time).
            property.SetValue(argument, property.PropertyType == typeof(Guid?) ? id : (object)id);
        }
    }

    private static bool TryGetRouteValue(
        IReadOnlyDictionary<string, object?> routeValues,
        string name,
        out string? value)
    {
        // RouteValueDictionary is case-insensitive, but the parameter is typed as the read-only
        // interface so a plain dictionary in a test behaves the same way.
        foreach (var (key, raw) in routeValues)
        {
            if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                continue;

            value = raw?.ToString();
            return value is not null;
        }

        value = null;
        return false;
    }

    private static PropertyInfo[] FindIdProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property =>
                property.CanWrite &&
                (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?)))
            .ToArray();
}
