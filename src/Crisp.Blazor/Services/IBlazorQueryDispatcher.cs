using Crisp.Queries;

namespace Crisp.Services;

/// <summary>
/// Query dispatcher for Blazor applications with caching support.
/// </summary>
public interface IBlazorQueryDispatcher : IQueryDispatcher
{
    /// <summary>
    /// Sends a query with loading state management.
    /// </summary>
    Task<TResponse> SendWithLoadingState<TResponse>(
        IQuery<TResponse> query,
        string? loadingKey = null,
        CancellationToken cancellationToken = default);
}