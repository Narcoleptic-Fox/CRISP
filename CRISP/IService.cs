namespace CRISP
{
    /// <summary>
    /// Base interface for all services in the CRISP architecture.
    /// </summary>
    public interface IService
    {
    }

    public interface IRequestService<TRequest> : IService
        where TRequest : IRequest
    {
        /// <summary>
        /// Sends a request to the service.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A value task representing the operation.</returns>
        ValueTask Send(TRequest request, CancellationToken cancellationToken = default);
    }

    public interface IRequestService<in TRequest, TResult> : IService
        where TRequest : IRequest<TResult>
    {
        /// <summary>
        /// Sends a request to the service.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A value task representing the operation with the result.</returns>
        ValueTask<TResult> Send(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for command services that handle commands of type <typeparamref name="TCommand"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public interface ICommandService<in TCommand> : IService
        where TCommand : Command
    {
        /// <summary>
        /// Sends a command to the service.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A value task representing the operation.</returns>
        ValueTask<Response> Send(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for command services that handle commands of type <typeparamref name="TCommand"/>
    /// and return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ICommandService<in TCommand, TResult> : IService
        where TCommand : Command<TResult>
    {
        /// <summary>
        /// Sends a command to the service.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A value task representing the operation with the result.</returns>
        ValueTask<Response<TResult>> Send(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for services that create entities.
    /// </summary>
    /// <typeparam name="TRequest">The type of the create request.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the created entity.</typeparam>
    public interface ICreateService<in TRequest, TId> : ICommandService<TRequest, TId>
        where TRequest : CreateCommand<TId>
        where TId : IComparable<TId>
    {
    }

    /// <summary>
    /// Interface for query services that handle queries of type <typeparamref name="TQuery"/>
    /// and return a response of type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IQueryService<in TQuery, TResponse> : IService
        where TQuery : Query<TResponse>
    {
        /// <summary>
        /// Sends a query to the service.
        /// </summary>
        /// <param name="query">The query to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A value task representing the operation with the result.</returns>
        ValueTask<Response<TResponse>> Send(TQuery query, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for filtered query services that handle queries of type <typeparamref name="TQuery"/>
    /// and return a paged result of type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of the filtered query.</typeparam>
    /// <typeparam name="TFilter">The type of the filter the query has.</typeparam>
    /// <typeparam name="TResponse">The type of the response items in the paged result.</typeparam>
    public interface IFilteredQueryService<in TQuery, TFilter, TResponse> : IQueryService<TQuery, PagedResult<TResponse>>
        where TQuery : FilteredQuery<TFilter, TResponse>
        where TFilter : FilterBase
    {
    }
}