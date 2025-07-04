namespace Crisp.Queries;

/// <summary>
/// Dispatches queries to their respective handlers.
/// </summary>
public interface IQueryDispatcher
{    /// <summary>
     /// Dispatches a query to its handler and returns a response.
     /// </summary>
     /// <typeparam name="TResponse">The type of the response.</typeparam>
     /// <param name="query">The query to dispatch.</param>
     /// <param name="cancellationToken">Cancellation token</param>
     /// <returns>The response from the query handler.</returns>
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
