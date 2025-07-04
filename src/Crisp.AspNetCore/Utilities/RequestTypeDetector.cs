using Crisp.Commands;
using Crisp.Queries;

namespace Crisp.Utilities;

/// <summary>
/// Centralizes request type detection logic to eliminate duplication across the framework.
/// </summary>
internal static class RequestTypeDetector
{
    /// <summary>
    /// Checks if a type implements ICommand or ICommand&lt;TResponse&gt;.
    /// </summary>
    public static bool IsCommand(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));
    }

    /// <summary>
    /// Checks if a type implements IQuery&lt;TResponse&gt;.
    /// </summary>
    public static bool IsQuery(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }

    /// <summary>
    /// Checks if a type implements ICommand (void command).
    /// </summary>
    public static bool IsVoidCommand(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces().Any(i => i == typeof(ICommand));
    }

    /// <summary>
    /// Checks if a type implements ICommand&lt;TResponse&gt; (non-void command).
    /// </summary>
    public static bool IsNonVoidCommand(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces().Any(i => 
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    /// <summary>
    /// Gets the command interface for a command type.
    /// </summary>
    public static Type? GetCommandInterface(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    /// <summary>
    /// Gets the query interface for a query type.
    /// </summary>
    public static Type? GetQueryInterface(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }

    /// <summary>
    /// Gets the response type for a command or query.
    /// </summary>
    public static Type? GetResponseType(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        // Check for command with response
        var commandInterface = GetCommandInterface(requestType);
        if (commandInterface != null)
        {
            return commandInterface.GetGenericArguments()[0];
        }

        // Check for query
        var queryInterface = GetQueryInterface(requestType);
        if (queryInterface != null)
        {
            return queryInterface.GetGenericArguments()[0];
        }

        // Void command
        if (IsVoidCommand(requestType))
        {
            return null;
        }

        throw new InvalidOperationException($"Type {requestType.Name} is not a valid command or query");
    }

    /// <summary>
    /// Validates that a type is a valid request type (command or query).
    /// </summary>
    public static void ValidateRequestType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        
        if (!IsCommand(type) && !IsQuery(type))
        {
            throw new InvalidOperationException($"Type {type.Name} does not implement ICommand or IQuery");
        }
    }
}