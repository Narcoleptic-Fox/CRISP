using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Events;

/// <summary>
/// Provides dependency injection registration helpers for the in-process event bus.
/// </summary>
public static class EventServiceCollectionExtensions
{
    /// <summary>
    /// Registers the channel-backed event publisher and the background worker
    /// that drains and dispatches published events.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing event settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EventWorkerOptions>()
            .Bind(configuration.GetSection(EventWorkerOptions.SectionName))
            .Validate(options => options.MaxAttempts >= 1, "Event worker MaxAttempts must be at least 1.")
            .Validate(options => options.RetryBaseDelay > TimeSpan.Zero, "Event worker RetryBaseDelay must be greater than zero.")
            .ValidateOnStart();

        // EventPublisher owns the channel, so it must be a singleton shared
        // between publishers (request scope) and the single worker.
        services.AddSingleton<EventPublisher>();
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<EventPublisher>());
        services.AddHostedService<EventWorker>();

        return services;
    }
}
