using Crisp.Commands;
using Crisp.Common;
using Crisp.Queries;

namespace Crisp.Pipeline;

/// <summary>
/// Defines a behavior to be applied to a request before or after the handler is invoked.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Pipeline handler. Handles a request and returns a response.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a behavior to be applied to a request before or after the handler is invoked.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
public interface IPipelineBehavior<TRequest>
   where TRequest : IRequest
{
    /// <summary>
    /// Pipeline handler. Handles a request and returns a response.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task.</returns>
    Task Handle(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Command-only behaviors that are only applied to commands that return a response.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Command-only behaviors that are only applied to commands that don't return a response.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
public interface ICommandPipelineBehavior<TCommand> : IPipelineBehavior<TCommand>
    where TCommand : ICommand
{
}

/// <summary>
/// Query-only behaviors that are only applied to queries.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IQueryPipelineBehavior<in TQuery, TResponse> : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
