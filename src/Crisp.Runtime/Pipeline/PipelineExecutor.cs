using Crisp.Commands;
using Crisp.Common;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Pipeline;

/// <summary>
/// Internal static class responsible for executing command and query pipelines.
/// This class builds and executes the behavior pipeline for each request,
/// including resolving handlers and applying cross-cutting concerns (behaviors).
/// </summary>
internal static class PipelineExecutor
{
    /// <summary>
    /// Executes a command pipeline for commands that return a response.
    /// Resolves the handler, builds the behavior pipeline, and executes the command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute.</typeparam>
    /// <typeparam name="TResponse">The type of response the command returns.</typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The command response as an object.</returns>
    public static async Task<TResponse> ExecuteCommandPipeline<TCommand, TResponse>(
        object command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        TCommand typedCommand = (TCommand)command;

        // Get the specific handler for this command type
        ICommandHandler<TCommand, TResponse> handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();

        // Resolve all applicable behaviors for this command
        IEnumerable<IPipelineBehavior<TCommand, TResponse>> behaviors = ResolveBehaviors<TCommand, TResponse>(serviceProvider, isCommand: true);

        // Define the core handler execution
        Task<TResponse> HandlerDelegate(CancellationToken cancellationToken)
        {
            return handler.Handle(typedCommand, cancellationToken);
        }

        // Build the behavior pipeline by wrapping the handler with behaviors in reverse order
        // This creates an onion-like structure where behaviors can execute before and after the handler
        RequestHandlerDelegate<TResponse> pipeline = behaviors
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>)HandlerDelegate,
                (next, behavior) => (ct) => behavior.Handle(typedCommand, next, ct));

        TResponse result = await pipeline(cancellationToken);
        return result!;
    }

    /// <summary>
    /// Executes a query pipeline for queries that return a response.
    /// Resolves the handler, builds the behavior pipeline, and executes the query.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to execute.</typeparam>
    /// <typeparam name="TResponse">The type of response the query returns.</typeparam>
    /// <param name="query">The query instance to execute.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The query response as an object.</returns>
    public static async Task<TResponse> ExecuteQueryPipeline<TQuery, TResponse>(
        object query,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        TQuery typedQuery = (TQuery)query;

        // Get the specific handler for this query type
        IQueryHandler<TQuery, TResponse> handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();

        // Resolve all applicable behaviors for this query
        IEnumerable<IPipelineBehavior<TQuery, TResponse>> behaviors = ResolveBehaviors<TQuery, TResponse>(serviceProvider, isCommand: false);

        // Define the core handler execution
        Task<TResponse> HandlerDelegate(CancellationToken cancellationToken)
        {
            return handler.Handle(typedQuery, cancellationToken);
        }

        // Build the behavior pipeline by wrapping the handler with behaviors in reverse order
        RequestHandlerDelegate<TResponse> pipeline = behaviors
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>)HandlerDelegate,
                (next, behavior) => (ct) => behavior.Handle(typedQuery, next, ct));

        TResponse result = await pipeline(cancellationToken);
        return result!;
    }

    /// <summary>
    /// Executes a void command pipeline for commands that don't return a response.
    /// Resolves the handler, builds the behavior pipeline, and executes the command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute.</typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteVoidCommandPipeline<TCommand>(
        object command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        TCommand typedCommand = (TCommand)command;

        // Get the specific handler for this command type
        ICommandHandler<TCommand> handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        // Resolve all applicable behaviors for this void command
        IEnumerable<IPipelineBehavior<TCommand>> behaviors = ResolveBehaviors<TCommand>(serviceProvider);

        // Define the core handler execution
        Task HandlerDelegate(CancellationToken cancellationToken)
        {
            return handler.Handle(typedCommand, cancellationToken);
        }

        // Build the behavior pipeline by wrapping the handler with behaviors in reverse order
        RequestHandlerDelegate pipeline = behaviors
            .Reverse()
            .Aggregate((RequestHandlerDelegate)HandlerDelegate,
                (next, behavior) => (ct) => behavior.Handle(typedCommand, next, ct));

        await pipeline(cancellationToken);
    }

    /// <summary>
    /// Resolves all applicable pipeline behaviors for a request that returns a response.
    /// Combines global behaviors, command/query-specific behaviors, and orders them by priority.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="isCommand">True if this is a command, false if it's a query.</param>
    /// <returns>An ordered list of behaviors to apply to the request.</returns>
    private static List<IPipelineBehavior<TRequest, TResponse>> ResolveBehaviors<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        bool isCommand)
        where TRequest : IRequest<TResponse>
    {
        List<IPipelineBehavior<TRequest, TResponse>> behaviors =
        [
            // 1. Global behaviors that apply to all requests
            .. serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>(),
        ];

        // 2. Command/Query specific behaviors
        // 2. Command/Query specific behaviors
        if (isCommand)
        {
            // Only add command-specific behaviors when TRequest actually implements ICommand<TResponse>
            if (typeof(ICommand<TResponse>).IsAssignableFrom(typeof(TRequest)))
            {
                Type commandBehaviorType = typeof(ICommandPipelineBehavior<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
                IEnumerable<object?> commandBehaviors = serviceProvider.GetServices(commandBehaviorType);

                foreach (object? behavior in commandBehaviors)
                {
                    behaviors.Add((IPipelineBehavior<TRequest, TResponse>)behavior!);
                }
            }
        }
        else
        {
            // Only add query-specific behaviors when TRequest actually implements IQuery<TResponse>
            if (typeof(IQuery<TResponse>).IsAssignableFrom(typeof(TRequest)))
            {
                Type queryBehaviorType = typeof(IQueryPipelineBehavior<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
                IEnumerable<object?> queryBehaviors = serviceProvider.GetServices(queryBehaviorType);

                foreach (object? behavior in queryBehaviors)
                {
                    behaviors.Add((IPipelineBehavior<TRequest, TResponse>)behavior!);
                }
            }
        }

        // 3. Type-specific behaviors are already included in global behaviors

        // Order behaviors by their execution priority
        return behaviors.OrderBy(GetBehaviorPriority).ToList();
    }

    /// <summary>
    /// Resolves all applicable pipeline behaviors for void commands (commands without responses).
    /// Combines global behaviors and command-specific behaviors, then orders them by priority.
    /// </summary>
    /// <typeparam name="TRequest">The type of void command request.</typeparam>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An ordered list of behaviors to apply to the void command.</returns>
    private static List<IPipelineBehavior<TRequest>> ResolveBehaviors<TRequest>(
        IServiceProvider serviceProvider)
        where TRequest : ICommand
    {
        List<IPipelineBehavior<TRequest>> behaviors =
        [
            // Add global behaviors for void commands
            .. serviceProvider.GetServices<IPipelineBehavior<TRequest>>(),
            // Add command-specific behaviors for void commands
            .. serviceProvider.GetServices<ICommandPipelineBehavior<TRequest>>(),
        ];

        // Order behaviors by their execution priority
        return behaviors.OrderBy(GetBehaviorPriority).ToList();
    }

    /// <summary>
    /// Determines the execution priority of a behavior based on its type name.
    /// Lower numbers execute first, creating a predictable behavior execution order.
    /// </summary>
    /// <param name="behavior">The behavior instance to get priority for.</param>
    /// <returns>
    /// Priority number where:
    /// 1 = Logging (executes first),
    /// 2 = Validation,
    /// 3 = Authorization,
    /// 4 = Transaction,
    /// 5 = Caching,
    /// 99 = Default (executes last)
    /// </returns>
    private static int GetBehaviorPriority(object behavior) =>
        // Define execution order based on behavior type
        behavior switch
        {
            Type t when t.IsAssignableTo(typeof(LoggingBehavior<,>)) => 1,        // Log everything first
            _ => 99                                 // Unknown behaviors execute last
        };
}
