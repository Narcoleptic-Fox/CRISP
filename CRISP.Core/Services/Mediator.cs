using CRISP.Core.Interfaces;
using CRISP.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRISP.Core.Services;

/// <summary>
/// Default implementation of the <see cref="IMediator"/> interface.
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;
    private readonly MediatorOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The mediator options.</param>
    public Mediator(
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        MediatorOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource? timeoutCts = _options.DefaultTimeoutSeconds > 0
            ? new CancellationTokenSource(TimeSpan.FromSeconds(_options.DefaultTimeoutSeconds))
            : null;

        using CancellationTokenSource linkedCts = timeoutCts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken)
            : new CancellationTokenSource();

        if (cancellationToken != default)
        {
            linkedCts.Token.Register(() => linkedCts.Cancel());
        }

        Type requestType = request.GetType();

        if (_options.EnableDetailedLogging)
        {
            _logger.LogDebug("Processing request {RequestType}", requestType.Name);
        }

        // Start timing if metrics are enabled
        DateTime startTime = _options.TrackRequestMetrics ? DateTime.UtcNow : default;

        try
        {
            // Get pipeline behaviors
            IEnumerable<IPipelineBehavior<IRequest<TResponse>, TResponse>> behaviors = _serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>();

            // Create the request handler execution pipeline
            async ValueTask<TResponse> Pipeline(CancellationToken cancellationToken)
            {
                Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

                IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);

                if (!handlers.Any())
                {
                    throw new InvalidOperationException($"No handler registered for {requestType.Name}");
                }

                if (handlers.Count() > 1 && !_options.AllowMultipleHandlers)
                {
                    throw new InvalidOperationException($"Multiple handlers registered for {requestType.Name}. Consider setting AllowMultipleHandlers to true if this is intended.");
                }

                object? handler = handlers.First();
                System.Reflection.MethodInfo? method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));

                if (method == null)
                {
                    throw new InvalidOperationException($"Handler for {requestType.Name} does not implement Handle method.");
                }

                object? result = method.Invoke(handler, new object[] { request, cancellationToken });

                return result == null
                    ? throw new InvalidOperationException($"Handler for {requestType.Name} returned null.")
                    : await (ValueTask<TResponse>)result;
            }

            // Execute the pipeline with behaviors
            RequestHandlerDelegate<TResponse> pipelineDelegate = behaviors
                .Reverse()
                .Aggregate((RequestHandlerDelegate<TResponse>)Pipeline, (next, behavior) =>
                    (CancellationToken cancellationToken) => behavior.Handle(request, next, cancellationToken));

            TResponse? response = await pipelineDelegate(linkedCts.Token);

            // Log metrics if enabled
            if (_options.TrackRequestMetrics)
            {
                TimeSpan elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Request {RequestType} completed in {ElapsedMilliseconds}ms",
                    requestType.Name, elapsed.TotalMilliseconds);
            }

            return response;
        }
        catch (OperationCanceledException) when (timeoutCts?.Token.IsCancellationRequested == true && !cancellationToken.IsCancellationRequested)
        {
            string message = $"Request {requestType.Name} timed out after {_options.DefaultTimeoutSeconds} seconds";
            _logger.LogWarning(message);
            throw new TimeoutException(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {RequestType}: {ErrorMessage}",
                requestType.Name, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource? timeoutCts = _options.DefaultTimeoutSeconds > 0
            ? new CancellationTokenSource(TimeSpan.FromSeconds(_options.DefaultTimeoutSeconds))
            : null;

        using CancellationTokenSource linkedCts = timeoutCts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken)
            : new CancellationTokenSource();

        if (cancellationToken != default)
        {
            linkedCts.Token.Register(() => linkedCts.Cancel());
        }

        Type requestType = request.GetType();

        if (_options.EnableDetailedLogging)
        {
            _logger.LogDebug("Processing request {RequestType}", requestType.Name);
        }

        // Start timing if metrics are enabled
        DateTime startTime = _options.TrackRequestMetrics ? DateTime.UtcNow : default;

        try
        {
            Type handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);

            if (!handlers.Any())
            {
                throw new InvalidOperationException($"No handler registered for {requestType.Name}");
            }

            if (handlers.Count() > 1 && !_options.AllowMultipleHandlers)
            {
                throw new InvalidOperationException($"Multiple handlers registered for {requestType.Name}. Consider setting AllowMultipleHandlers to true if this is intended.");
            }

            object? handler = handlers.First();
            System.Reflection.MethodInfo? method = handlerType.GetMethod(nameof(IRequestHandler<IRequest>.Handle));

            if (method == null)
            {
                throw new InvalidOperationException($"Handler for {requestType.Name} does not implement Handle method.");
            }

            object? result = method.Invoke(handler, new object[] { request, linkedCts.Token });

            if (result == null)
            {
                throw new InvalidOperationException($"Handler for {requestType.Name} returned null.");
            }

            await (ValueTask)result;

            // Log metrics if enabled
            if (_options.TrackRequestMetrics)
            {
                TimeSpan elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Request {RequestType} completed in {ElapsedMilliseconds}ms",
                    requestType.Name, elapsed.TotalMilliseconds);
            }
        }
        catch (OperationCanceledException) when (timeoutCts?.Token.IsCancellationRequested == true && !cancellationToken.IsCancellationRequested)
        {
            string message = $"Request {requestType.Name} timed out after {_options.DefaultTimeoutSeconds} seconds";
            _logger.LogWarning(message);
            throw new TimeoutException(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {RequestType}: {ErrorMessage}",
                requestType.Name, ex.Message);
            throw;
        }
    }
}