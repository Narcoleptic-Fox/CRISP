namespace Crisp.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs during resource modification.
/// Maps to HTTP 409 Conflict status code.
/// </summary>
public class ConflictException : CrispException
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}