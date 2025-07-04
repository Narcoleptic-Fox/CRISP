namespace Crisp.Exceptions;

/// <summary>
/// Exception thrown when access to a resource is forbidden.
/// Maps to HTTP 403 Forbidden status code.
/// </summary>
public class ForbiddenException : CrispException
{
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}