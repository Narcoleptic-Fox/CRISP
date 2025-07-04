namespace Crisp.Exceptions;

/// <summary>
/// Utility class for building clear, actionable error messages.
/// Provides consistent error messaging across the CRISP framework.
/// </summary>
public static class ErrorMessageBuilder
{
    /// <summary>
    /// Creates a descriptive "not found" error message with actionable guidance.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="identifier">The identifier that was searched for.</param>
    /// <param name="suggestions">Optional additional suggestions.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string NotFound(string entityName, object identifier, string? suggestions = null)
    {
        var message = $"The {entityName} with identifier '{identifier}' was not found.";
        
        var standardSuggestions = new List<string>
        {
            $"Verify the {entityName.ToLower()} ID is correct",
            $"Check if the {entityName.ToLower()} still exists",
            "Ensure you have permission to access this resource"
        };

        if (!string.IsNullOrWhiteSpace(suggestions))
        {
            standardSuggestions.Insert(0, suggestions);
        }

        message += "\n\nSuggestions:";
        message += string.Join("\n", standardSuggestions.Select(s => $"• {s}"));

        return message;
    }

    /// <summary>
    /// Creates a descriptive authorization error message with specific guidance.
    /// </summary>
    /// <param name="operation">The operation that was attempted.</param>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="requiredPermission">The specific permission required.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string Unauthorized(string operation, string resource, string? requiredPermission = null)
    {
        var message = $"You are not authorized to {operation} {resource}.";

        var suggestions = new List<string>
        {
            "Verify you are logged in with the correct account",
            "Check that your session has not expired"
        };

        if (!string.IsNullOrWhiteSpace(requiredPermission))
        {
            suggestions.Insert(0, $"Ensure you have the '{requiredPermission}' permission");
        }
        else
        {
            suggestions.Insert(0, "Verify you have the necessary permissions for this operation");
        }

        suggestions.Add("Contact your administrator if you believe you should have access");

        message += "\n\nSuggestions:";
        message += string.Join("\n", suggestions.Select(s => $"• {s}"));

        return message;
    }

    /// <summary>
    /// Creates a descriptive validation error message with guidance for fixing issues.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="value">The value that was provided.</param>
    /// <param name="constraint">The constraint that was violated.</param>
    /// <param name="suggestion">Specific suggestion for fixing the issue.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string ValidationError(string fieldName, object? value, string constraint, string? suggestion = null)
    {
        var message = $"The field '{fieldName}' has an invalid value";
        
        if (value != null)
        {
            message += $" '{value}'";
        }

        message += $". {constraint}";

        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            message += $"\n\nSuggestion: {suggestion}";
        }

        return message;
    }

    /// <summary>
    /// Creates a descriptive configuration error message with setup guidance.
    /// </summary>
    /// <param name="component">The component that is misconfigured.</param>
    /// <param name="issue">The specific configuration issue.</param>
    /// <param name="solution">The recommended solution.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string ConfigurationError(string component, string issue, string solution)
    {
        var message = $"Configuration error in {component}: {issue}";
        
        var suggestions = new List<string>
        {
            solution,
            "Check your application configuration files",
            "Verify all required dependencies are registered",
            "Review the CRISP documentation for setup guidance"
        };

        message += "\n\nTo resolve this issue:";
        message += string.Join("\n", suggestions.Select(s => $"• {s}"));

        return message;
    }

    /// <summary>
    /// Creates a descriptive timeout error message with retry guidance.
    /// </summary>
    /// <param name="operation">The operation that timed out.</param>
    /// <param name="timeoutSeconds">The timeout duration in seconds.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string TimeoutError(string operation, int timeoutSeconds)
    {
        var message = $"The operation '{operation}' timed out after {timeoutSeconds} seconds.";
        
        var suggestions = new List<string>
        {
            "Try the operation again",
            "Check your network connection",
            "Verify the target service is responsive",
            "Consider increasing the timeout configuration if this issue persists"
        };

        message += "\n\nSuggestions:";
        message += string.Join("\n", suggestions.Select(s => $"• {s}"));

        return message;
    }

    /// <summary>
    /// Creates a descriptive dependency error message with resolution guidance.
    /// </summary>
    /// <param name="missingService">The missing service or dependency.</param>
    /// <param name="requiredFor">What the dependency is required for.</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string DependencyError(string missingService, string requiredFor)
    {
        var message = $"Required service '{missingService}' is not registered and is needed for {requiredFor}.";
        
        var suggestions = new List<string>
        {
            $"Register the {missingService} service in your DI container",
            "Check your service registration configuration",
            "Verify all required NuGet packages are installed",
            "Review the CRISP setup documentation for required dependencies"
        };

        message += "\n\nTo resolve this issue:";
        message += string.Join("\n", suggestions.Select(s => $"• {s}"));

        return message;
    }

    /// <summary>
    /// Creates a descriptive rate limiting error message with retry guidance.
    /// </summary>
    /// <param name="operation">The operation that was rate limited.</param>
    /// <param name="retryAfterSeconds">When the user can retry (in seconds).</param>
    /// <returns>A clear, actionable error message.</returns>
    public static string RateLimitError(string operation, int retryAfterSeconds)
    {
        var message = $"Rate limit exceeded for '{operation}'. Too many requests have been made.";
        
        var suggestions = new List<string>
        {
            $"Wait {retryAfterSeconds} seconds before trying again",
            "Reduce the frequency of your requests",
            "Consider implementing request batching if applicable",
            "Contact support if you need higher rate limits"
        };

        message += "\n\nSuggestions:";
        message += string.Join("\n", suggestions.Select(s => $"• {s}"));

        return message;
    }
}