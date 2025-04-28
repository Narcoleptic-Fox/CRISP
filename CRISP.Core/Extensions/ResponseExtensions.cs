using CRISP.Core.Responses;

namespace CRISP;

/// <summary>
/// Extension methods for working with CRISP responses.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Ensures that the response is successful, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="response">The response to check.</param>
    /// <param name="errorMessage">Optional custom error message to use if the response is not successful.</param>
    /// <returns>The data from the response if successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the response is not successful.</exception>
    public static T EnsureSuccess<T>(this Response<T> response, string? errorMessage = null)
    {
        if (!response.IsSuccess)
        {
            string message = errorMessage ?? response.Message;
            if (response.Errors != null && response.Errors.Any())
            {
                message = $"{message}: {string.Join(", ", response.Errors)}";
            }
            throw new InvalidOperationException(message);
        }

        return response.Data!;
    }

    /// <summary>
    /// Ensures that the response is successful, otherwise throws an exception.
    /// </summary>
    /// <param name="response">The response to check.</param>
    /// <param name="errorMessage">Optional custom error message to use if the response is not successful.</param>
    /// <exception cref="InvalidOperationException">Thrown when the response is not successful.</exception>
    public static void EnsureSuccess(this Response response, string? errorMessage = null)
    {
        if (!response.IsSuccess)
        {
            string message = errorMessage ?? response.Message;
            if (response.Errors != null && response.Errors.Any())
            {
                message = $"{message}: {string.Join(", ", response.Errors)}";
            }
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Maps a response to a new type using a mapping function.
    /// </summary>
    /// <typeparam name="TSource">The source type of the response data.</typeparam>
    /// <typeparam name="TDestination">The destination type for the response data.</typeparam>
    /// <param name="response">The response to map.</param>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new response with mapped data.</returns>
    public static Response<TDestination> Map<TSource, TDestination>(
        this Response<TSource> response,
        Func<TSource, TDestination> mapper) => !response.IsSuccess
            ? new Response<TDestination>
            {
                IsSuccess = false,
                Message = response.Message,
                Errors = response.Errors
            }
            : new Response<TDestination>
            {
                IsSuccess = true,
                Message = response.Message,
                Data = response.Data != null ? mapper(response.Data) : default
            };
}