using FluentValidation;
using FluentValidation.Results;

namespace CRISP.Server.Middleware;

/// <summary>
/// Generic endpoint filter that resolves an IValidator&lt;T&gt; from DI
/// and validates the request body before the endpoint executes.
/// Returns 400 ProblemDetails with structured validation errors on failure.
/// </summary>
public sealed class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    /// <summary>
    /// Validates the matching endpoint argument before invoking the endpoint delegate.
    /// </summary>
    /// <param name="context">The endpoint invocation context.</param>
    /// <param name="next">The next endpoint filter delegate.</param>
    /// <returns>The endpoint result, or a validation problem result when validation fails.</returns>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
            return await next(context);

        // Find the argument of type T in the endpoint parameters
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return await next(context);

        var result = await validator.ValidateAsync(argument);
        if (result.IsValid)
            return await next(context);

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}

/// <summary>
/// Validates all endpoint arguments that have registered FluentValidation validators.
/// </summary>
public sealed class ValidationEndpointFilter : IEndpointFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEndpointFilter"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve validators.</param>
    /// <param name="logger">The logger for validation filter diagnostics.</param>
    public ValidationEndpointFilter(IServiceProvider serviceProvider, ILogger<ValidationEndpointFilter> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates endpoint arguments before invoking the endpoint delegate.
    /// </summary>
    /// <param name="context">The endpoint invocation context.</param>
    /// <param name="next">The next endpoint filter delegate.</param>
    /// <returns>The endpoint result, or a validation problem result when validation fails.</returns>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments is null || context.Arguments.Count == 0)
            return await next(context);

        var validators = context.Arguments.SelectMany(a =>
        {
            if (a is null)
                return [];
            Type validatorGenericType = typeof(IValidator<>).MakeGenericType(a.GetType());
            var validators = _serviceProvider.GetServices(validatorGenericType) as IEnumerable<IValidator>;
            if (validators is null)
                return [];
            Type contextGenericType = typeof(ValidationContext<>).MakeGenericType(a.GetType());
            var validationContext = Activator.CreateInstance(contextGenericType, a) as IValidationContext;
            return validators.Select<IValidator, (IValidator Validator, IValidationContext? Context)>(v => new() { Validator = v, Context = validationContext });
        }) ?? [];

        if (!validators.Any())
            return await next(context);

        ValidationResult[] results = await Task.WhenAll(validators.Select(v => v.Validator.ValidateAsync(v.Context, context.HttpContext.RequestAborted)));
        if (results.All(r => r.IsValid))
            return await next(context);

        var errors = results.SelectMany(r => r.Errors)
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}

/// <summary>
/// Extension to fluently add validation to minimal API endpoints.
/// Usage: .AddEndpointFilter&lt;ValidationEndpointFilter&lt;MyRequest&gt;&gt;()
/// Or:    .WithValidation&lt;MyRequest&gt;()
/// </summary>
public static class ValidationEndpointFilterExtensions
{
    /// <summary>
    /// Adds validation for a specific endpoint argument type.
    /// </summary>
    /// <typeparam name="T">The endpoint argument type to validate.</typeparam>
    /// <param name="builder">The route handler builder to configure.</param>
    /// <returns>The route handler builder.</returns>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
        => builder.AddEndpointFilter<ValidationEndpointFilter<T>>();

    /// <summary>
    /// Adds validation for all endpoint arguments with registered validators.
    /// </summary>
    /// <param name="builder">The route group builder to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder WithValidation(this RouteGroupBuilder builder)
        => builder.AddEndpointFilter<ValidationEndpointFilter>();
}
