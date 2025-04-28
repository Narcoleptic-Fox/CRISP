namespace CRISP.Core.Responses;

/// <summary>
/// Base class for all responses in the CRISP architecture.
/// Provides a standardized structure for API responses.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class Response<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the response.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data returned by the operation.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during the operation.
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }

    /// <summary>
    /// Creates a successful response with the specified data.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A successful response containing the specified data.</returns>
    public static Response<T> Success(T data, string message = "Operation completed successfully") => new()
    {
        IsSuccess = true,
        Message = message,
        Data = data
    };

    /// <summary>
    /// Creates a failure response with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">Optional collection of specific errors.</param>
    /// <returns>A failure response with the specified error details.</returns>
    public static Response<T> Failure(string message, IEnumerable<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}

/// <summary>
/// Represents a response with no data payload.
/// </summary>
public class Response : Response<object>
{
    /// <summary>
    /// Creates a successful response with no data.
    /// </summary>
    /// <param name="message">An optional success message.</param>
    /// <returns>A successful response with no data.</returns>
    public static Response Success(string message = "Operation completed successfully") => new()
    {
        IsSuccess = true,
        Message = message
    };

    /// <summary>
    /// Creates a failure response with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">Optional collection of specific errors.</param>
    /// <returns>A failure response with the specified error details.</returns>
    public static new Response Failure(string message, IEnumerable<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}