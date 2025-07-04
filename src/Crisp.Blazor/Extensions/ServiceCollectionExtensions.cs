using Blazor.Concurrency;
using Crisp.Blazor.Concurrency.Handlers;
using Crisp.Blazor.Notifications;
using Crisp.Builder;
using Crisp.Events;
using Crisp.Notifications;
using Crisp.Pipeline;
using Crisp.Runtime.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Crisp.Blazor.Extensions;

/// <summary>
/// Extension methods for configuring CRISP in Blazor WASM applications.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CRISP services optimized for Blazor WASM.
    /// </summary>
    public static IServiceCollection AddCrispBlazor(
        this IServiceCollection services,
        Action<BlazorCrispBuilder>? configureBuilder = null)
    {
        // Step 1: Configure builder/options
        BlazorCrispBuilder builder = new(services);
        configureBuilder?.Invoke(builder);
        services.AddSingleton(builder.Options);

        // Step 2: Register core services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IEventPublisher, ChannelEventPublisher>();

        // Step 3: Register Blazor.Concurrency.Core
        services.AddBlazorConcurrencyCore();

        // Step 4: Register concurrency handlers
        services.AddScoped(typeof(ExecuteBackgroundTaskCommandHandler<>));
        services.AddScoped<GetCacheItemQueryHandler>();
        services.AddScoped<SetCacheItemCommandHandler>();

        // Step 5: Register lean pipeline behaviors
        if (builder.Options.Pipeline.EnableLogging)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }

        if (builder.Options.Pipeline.EnableErrorHandling)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
        }

        // Step 6: Configure JSON for WASM
        ConfigureJsonSerialization(services, builder.Options);

        // Step 7: Build and compile CRISP
        builder.Build();

        return services;
    }

    private static void ConfigureJsonSerialization(IServiceCollection services, CrispBlazorOptions options)
    {
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = options.Serialization.UseCamelCase
                ? JsonNamingPolicy.CamelCase
                : null,
            WriteIndented = options.Serialization.WriteIndented,
            DefaultIgnoreCondition = options.Serialization.IgnoreNullValues
                ? System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                : System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        services.AddSingleton(jsonOptions);
    }
}
