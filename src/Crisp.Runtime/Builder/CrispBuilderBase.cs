using Crisp.Commands;
using Crisp.Dispatchers;
using Crisp.Pipeline;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Crisp.Builder;

/// <summary>
/// Base builder with common handler discovery logic.
/// Transport libraries extend this for their specific needs.
/// </summary>
public abstract class CrispBuilderBase : ICrispBuilder
{
    private int _commandHandlerCount;
    private int _queryHandlerCount;

    /// <summary>
    /// Collection of assemblies to scan for command and query handlers.
    /// </summary>
    protected readonly List<Assembly> _assemblies = [];

    /// <summary>
    /// Dictionary containing all compiled pipelines (both typed and void).
    /// Key: Request type, Value: Compiled pipeline interface for unified storage.
    /// </summary>
    internal readonly Dictionary<Type, ICompiledPipeline> _compiledPipelines = [];

    /// <summary>
    /// Collection of handlers.
    /// </summary>
    protected List<HandlerRegistration> _handlers = [];

    /// <summary>
    /// The service collection used for dependency injection registration.
    /// </summary>
    protected readonly IServiceCollection _services;

    /// <inheritdoc/>
    public int CommandHandlerCount =>
        _commandHandlerCount;

    /// <inheritdoc/>
    public int CompiledPipelineCount =>
        _compiledPipelines.Count;

    /// <inheritdoc/>
    public int QueryHandlerCount =>
        _queryHandlerCount;

    /// <inheritdoc/>
    public CrispOptions Options { get; protected set; } = new CrispOptions();


    /// <inheritdoc/>
    public ICrispBuilder ConfigureOptions(Action<CrispOptions> configureOptions)
    {
        configureOptions(Options);
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the CrispBuilder with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to register dependencies with.</param>
    public CrispBuilderBase(IServiceCollection services) =>
        _services = services;

    /// <inheritdoc/>
    public ICrispBuilder AddAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    /// <inheritdoc/>
    public ICrispBuilder AddAssemblies(params Assembly[] assembly)
    {
        _assemblies.AddRange(assembly);
        return this;
    }

    /// <inheritdoc/>
    public IReadOnlyList<HandlerRegistration> GetHandlerMappings() =>
        _handlers;

    /// <inheritdoc/>
    public Assembly[] GetAssemblies() =>
        _assemblies.ToArray();

    /// <inheritdoc/>
    public ICrispBuilder RegisterHandlersFromAssemblies(params Assembly[] assembly)
    {
        _assemblies.AddRange(assembly);
        return this;
    }

    /// <inheritdoc/>
    public abstract void Build();

    /// <summary>
    /// Discovers all command and query handlers from the registered assemblies.
    /// Scans for classes implementing ICommandHandler or IQueryHandler interfaces.
    /// </summary>
    /// <returns>A list of handler registrations containing metadata about each discovered handler.</returns>
    protected virtual void DiscoverHandlers()
    {
        ConcurrentBag<HandlerRegistration> registrations = [];

        Parallel.ForEach(_assemblies, assembly =>
        {
            // Find all concrete classes that implement handler interfaces
            IEnumerable<Type> handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => t.GetInterfaces().Any(IsHandlerInterface));

            foreach (Type? handlerType in handlerTypes)
            {
                // Get all handler interfaces implemented by this type
                IEnumerable<Type> interfaces = handlerType.GetInterfaces()
                    .Where(IsHandlerInterface);

                foreach (Type? handlerInterface in interfaces)
                {
                    HandlerRegistration? registration = CreateRegistration(handlerType, handlerInterface);
                    if (registration != null)
                    {
                        registrations.Add(registration);

                        // Update diagnostic counters
                        if (registration.IsCommand)
                            Interlocked.Increment(ref _commandHandlerCount);
                        else
                            Interlocked.Increment(ref _queryHandlerCount);
                    }
                }
            }
        });
        _handlers = registrations.ToList();
    }

    /// <summary>
    /// Creates a handler registration from a handler type and its implemented interface.
    /// Extracts metadata about the request/response types and handler characteristics.
    /// </summary>
    /// <param name="handlerType">The concrete handler class type.</param>
    /// <param name="handlerInterface">The handler interface being implemented.</param>
    /// <returns>A HandlerRegistration containing metadata, or null if the interface is not recognized.</returns>
    protected HandlerRegistration? CreateRegistration(Type handlerType, Type handlerInterface)
    {
        Type genericDef = handlerInterface.GetGenericTypeDefinition();
        Type[] genericArgs = handlerInterface.GetGenericArguments();

        // Command handler with response: ICommandHandler<TCommand, TResponse>
        if (genericDef == typeof(ICommandHandler<,>))
        {
            return new HandlerRegistration
            {
                HandlerType = handlerType,
                HandlerInterface = handlerInterface,
                RequestType = genericArgs[0],
                ResponseType = genericArgs[1],
                IsCommand = true,
                HasResponse = true
            };
        }
        // Void command handler: ICommandHandler<TCommand>
        else if (genericDef == typeof(ICommandHandler<>))
        {
            return new HandlerRegistration
            {
                HandlerType = handlerType,
                HandlerInterface = handlerInterface,
                RequestType = genericArgs[0],
                IsCommand = true,
                HasResponse = false
            };
        }
        // Query handler: IQueryHandler<TQuery, TResponse>
        else if (genericDef == typeof(IQueryHandler<,>))
        {
            return new HandlerRegistration
            {
                HandlerType = handlerType,
                HandlerInterface = handlerInterface,
                RequestType = genericArgs[0],
                ResponseType = genericArgs[1],
                IsCommand = false,
                HasResponse = true
            };
        }

        return null;
    }

    /// <summary>
    /// Determines if a type is a valid handler interface (ICommandHandler or IQueryHandler).
    /// </summary>
    /// <param name="genericType">The type to check.</param>
    /// <returns>True if the type is a handler interface, false otherwise.</returns>
    protected bool IsHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(ICommandHandler<,>) ||
               genericDef == typeof(ICommandHandler<>) ||
               genericDef == typeof(IQueryHandler<,>);
    }

    /// <summary>
    /// Registers all discovered handlers with the dependency injection container.
    /// Each handler is registered as scoped to ensure proper lifetime management.
    /// </summary>
    protected void RegisterHandlers()
    {
        foreach (HandlerRegistration handler in _handlers)
        {
            _services.AddScoped(handler.HandlerInterface, handler.HandlerType);
        }
    }

    /// <summary>
    /// Compiles optimized execution pipelines for all discovered handlers.
    /// Uses expression trees to generate fast delegates that avoid reflection at runtime.
    /// </summary>
    protected void CompilePipelines()
    {
        foreach (HandlerRegistration handler in _handlers)
        {
            if (handler.HasResponse)
            {
                CompilePipelineWithResponse(handler);
            }
            else
            {
                CompileVoidPipeline(handler);
            }
        }
    }

    /// <summary>
    /// Compiles an optimized pipeline for handlers that return a response (commands with results and queries).
    /// Creates a compiled delegate using expression trees for maximum performance.
    /// </summary>
    /// <param name="registration">The handler registration to compile a pipeline for.</param>
    private void CompilePipelineWithResponse(HandlerRegistration registration)
    {
        // Use reflection to call the generic helper method
        MethodInfo compileMethod = typeof(CrispBuilderBase)
            .GetMethod(nameof(CompilePipelineWithResponseGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(registration.RequestType, registration.ResponseType);

        compileMethod.Invoke(this, [registration]);
    }

    /// <summary>
    /// Generic helper method to compile a typed pipeline with response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="registration">The handler registration.</param>
    private void CompilePipelineWithResponseGeneric<TRequest, TResponse>(HandlerRegistration registration)
    {
        // Parameters for the compiled expression
        ParameterExpression cmdParam = Expression.Parameter(typeof(object), "cmd");
        ParameterExpression spParam = Expression.Parameter(typeof(IServiceProvider), "sp");
        ParameterExpression ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        // Get the appropriate generic method to call (command or query pipeline)
        MethodInfo executeMethod = registration.IsCommand
            ? typeof(PipelineExecutor).GetMethod(nameof(PipelineExecutor.ExecuteCommandPipeline))!
            : typeof(PipelineExecutor).GetMethod(nameof(PipelineExecutor.ExecuteQueryPipeline))!;

        // Make the method generic with the specific request and response types
        MethodInfo genericMethod = executeMethod.MakeGenericMethod(typeof(TRequest), typeof(TResponse));

        // Build the expression: PipelineExecutor.ExecuteXxxPipeline<TRequest, TResponse>(cmd, sp, ct)
        MethodCallExpression callExpr = Expression.Call(null, genericMethod, cmdParam, spParam, ctParam);

        // Compile to a strongly-typed delegate
        Expression<Func<object, IServiceProvider, CancellationToken, Task<TResponse>>> lambda =
            Expression.Lambda<Func<object, IServiceProvider, CancellationToken, Task<TResponse>>>(
                callExpr, cmdParam, spParam, ctParam);

        // Create the compiled pipeline with type safety
        CompiledPipeline<TResponse> compiled = new()
        {
            RequestType = registration.RequestType,
            ResponseType = registration.ResponseType!,
            HandlerType = registration.HandlerType,
            Executor = lambda.Compile()
        };

        _compiledPipelines[registration.RequestType] = compiled;
    }

    /// <summary>
    /// Compiles an optimized pipeline for void commands (commands that don't return a response).
    /// Creates a compiled delegate using expression trees for maximum performance.
    /// </summary>
    /// <param name="registration">The handler registration to compile a pipeline for.</param>
    private void CompileVoidPipeline(HandlerRegistration registration)
    {
        ParameterExpression cmdParam = Expression.Parameter(typeof(object), "cmd");
        ParameterExpression spParam = Expression.Parameter(typeof(IServiceProvider), "sp");
        ParameterExpression ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        MethodInfo executeMethod = typeof(PipelineExecutor)
            .GetMethod(nameof(PipelineExecutor.ExecuteVoidCommandPipeline))
            !.MakeGenericMethod(registration.RequestType);

        MethodCallExpression callExpr = Expression.Call(null, executeMethod, cmdParam, spParam, ctParam);

        Expression<Func<object, IServiceProvider, CancellationToken, Task>> lambda = Expression.Lambda<Func<object, IServiceProvider, CancellationToken, Task>>(
            callExpr, cmdParam, spParam, ctParam);

        CompiledVoidPipeline compiled = new()
        {
            RequestType = registration.RequestType,
            HandlerType = registration.HandlerType,
            Executor = lambda.Compile()
        };

        _compiledPipelines[registration.RequestType] = compiled;
    }

    /// <summary>
    /// Registers the pre-compiled dispatchers with the dependency injection container.
    /// Uses the unified pipeline storage for both command and query dispatchers.
    /// </summary>
    protected void RegisterDispatchers()
    {
        _services.AddScoped<ICommandDispatcher>(sp =>
            new PreCompiledCommandDispatcher(
                new Dictionary<Type, ICompiledPipeline>(_compiledPipelines),
                sp));

        _services.AddScoped<IQueryDispatcher>(sp =>
            new PreCompiledQueryDispatcher(
                new Dictionary<Type, ICompiledPipeline>(_compiledPipelines),
                sp));
    }

    /// <summary>
    /// Registers default pipeline behaviors based on configuration.
    /// </summary>
    protected void RegisterDefaultPipelineBehaviors()
    {
        // Register CRISP options as singleton
        _services.AddSingleton(Options);

        // Register error handling behavior if enabled
        if (Options.Pipeline.EnableErrorHandling)
        {
            _services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
        }

        // Register logging behavior if enabled
        if (Options.Pipeline.EnableLogging)
        {
            _services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
    }
}