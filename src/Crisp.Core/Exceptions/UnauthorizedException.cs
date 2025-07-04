namespace Crisp.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an operation.
/// </summary>
public class UnauthorizedException : CrispException
{    /// <summary>
     /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
     /// </summary>
    public UnauthorizedException() : base("Access denied. You do not have sufficient permissions to perform this operation. Please contact your administrator or verify your authentication status.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnauthorizedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class for a specific operation.
    /// </summary>
    /// <param name="operation">The name of the operation that was not authorized.</param>
    /// <param name="resourceName">The name of the resource that access was denied to.</param>
    public UnauthorizedException(string operation, string resourceName)
        : base($"You are not authorized to {operation} the {resourceName}. Please ensure you have the required permissions or contact your administrator for access.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class for a specific required role or permission.
    /// </summary>
    /// <param name="operation">The name of the operation that was not authorized.</param>
    /// <param name="resourceName">The name of the resource that access was denied to.</param>
    /// <param name="requiredPermission">The specific permission or role required.</param>
    public UnauthorizedException(string operation, string resourceName, string requiredPermission)
        : base($"You are not authorized to {operation} the {resourceName}. This operation requires '{requiredPermission}' permission. Please contact your administrator to request access.")
    {
    }
}
