using CRISP.Resilience;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring CRISP services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds resilience strategies to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddResilienceStrategies(this IServiceCollection services)
    {
        // Register the resilience strategies
        services.AddTransient(sp =>
        {
            ResilienceOptions options = sp.GetRequiredService<ResilienceOptions>();
            return new RetryStrategy(
                sp.GetRequiredService<ILogger<RetryStrategy>>(),
                maxRetryAttempts: options.Retry.MaxRetryAttempts,
                initialDelay: TimeSpan.FromSeconds(options.Retry.InitialDelaySeconds),
                backoffFactor: options.Retry.BackoffFactor);
        });

        services.AddTransient(sp =>
        {
            ResilienceOptions options = sp.GetRequiredService<ResilienceOptions>();
            return new CircuitBreakerStrategy(
                sp.GetRequiredService<ILogger<CircuitBreakerStrategy>>(),
                failureThreshold: options.CircuitBreaker.FailureThreshold,
                resetTimeout: TimeSpan.FromSeconds(options.CircuitBreaker.DurationOfBreakSeconds));
        });

        services.AddTransient(sp =>
        {
            ResilienceOptions options = sp.GetRequiredService<ResilienceOptions>();
            return new TimeoutStrategy(
                sp.GetRequiredService<ILogger<TimeoutStrategy>>(),
                timeout: TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds));
        });

        // Register a factory to create composite strategies
        services.AddTransient<Func<IEnumerable<IResilienceStrategy>, IResilienceStrategy>>(sp =>
            strategies => new CompositeResilienceStrategy(strategies));

        // Register the common resilience strategy combinations
        services.AddTransient<IResilienceStrategy>(sp => new CompositeResilienceStrategy(
            sp.GetRequiredService<TimeoutStrategy>(),
            sp.GetRequiredService<RetryStrategy>(),
            sp.GetRequiredService<CircuitBreakerStrategy>()));

        services.AddSingleton(CreateDefaultResilienceOptions);
        return services;
    }

    private static ResilienceOptions CreateDefaultResilienceOptions() => new();

    /// <summary>
    /// Configures resilience options.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The options builder for chaining.</returns>
    public static CrispOptionsBuilder ConfigureResilience(this CrispOptionsBuilder builder, Action<ResilienceOptions> configure)
    {
        ResilienceOptions options = new();
        configure(options);
        builder.services.ReplaceSingleton(options);
        return builder;
    }
}