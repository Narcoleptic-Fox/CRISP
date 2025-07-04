namespace Crisp.Exceptions;
/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : CrispException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException() : base("One or more validation failures have occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified validation result.
    /// </summary>
    /// <param name="validationResult">The validation result containing validation errors.</param>
    public ValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValidationException(string message) : base(message) { }
}
