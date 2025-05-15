using CRISP;
using CRISP.Behaviors;
using CRISP.Events;
using CRISP.Options;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring CRISP services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CRISP core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrispCore(this IServiceCollection services)
    {
        // Add default options
        services.AddOptions();
        services.AddSingleton(CreateDefaultEventOptions());
        services.AddSingleton(CreateDefaultMediatorOptions());
        services.AddSingleton(CreateDefaultChannelEventOptions());

        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

        // Register the event dispatcher
        services.AddScoped<IEventDispatcher>(sp =>
        {
            EventOptions eventOptions = sp.GetRequiredService<EventOptions>();

            // Use channel-based dispatcher if configured
            if (eventOptions.UseChannels)
            {
                return new ChannelEventDispatcher(
                    sp,
                    sp.GetRequiredService<ILogger<ChannelEventDispatcher>>(),
                    eventOptions,
                    sp.GetRequiredService<ChannelEventOptions>());
            }

            // Otherwise use the standard dispatcher
            return new EventDispatcher(
                sp,
                sp.GetRequiredService<ILogger<EventDispatcher>>(),
                eventOptions);
        });

        return services;
    }

    /// <summary>
    /// Adds CRISP core services with custom options.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrispCore(
        this IServiceCollection services,
        Action<CrispOptionsBuilder> configureOptions)
    {
        // Add default options first
        services.AddCrispCore();

        // Then allow customization
        CrispOptionsBuilder optionsBuilder = new(services);
        configureOptions(optionsBuilder);

        return services;
    }

    /// <summary>
    /// Registers all request handlers in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRequestHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var handlerTypes = from assembly in assemblies
                           from type in assembly.GetTypes()
                           from @interface in type.GetInterfaces()
                           where @interface.IsGenericType &&
                                 (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                                  @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                           select new { HandlerType = type, InterfaceType = @interface };

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
        }

        return services;
    }

    /// <summary>
    /// Registers all pipeline behaviors in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for behaviors.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPipelineBehaviors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var behaviorTypes = from assembly in assemblies
                            from type in assembly.GetTypes()
                            from @interface in type.GetInterfaces()
                            where @interface.IsGenericType &&
                                  @interface.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)
                            select new { BehaviorType = type, InterfaceType = @interface };

        foreach (var behavior in behaviorTypes)
        {
            services.AddTransient(behavior.InterfaceType, behavior.BehaviorType);
        }

        return services;
    }

    /// <summary>
    /// Registers all modules in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for modules.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        IEnumerable<Type> moduleTypes = from assembly in assemblies
                                        from type in assembly.GetTypes()
                                        where typeof(IModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                                        select type;

        foreach (Type? moduleType in moduleTypes)
        {
            // Create module instance
            if (Activator.CreateInstance(moduleType) is IModule module)
            {
                // Register module services
                module.RegisterServices(services);

                // Register module itself
                services.AddSingleton(typeof(IModule), module);
            }
        }

        return services;
    }

    /// <summary>
    /// Registers all event handlers in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for event handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var handlerTypes = from assembly in assemblies
                           from type in assembly.GetTypes()
                           from @interface in type.GetInterfaces()
                           where @interface.IsGenericType &&
                                 @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>)
                           select new { HandlerType = type, InterfaceType = @interface };

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
        }

        return services;
    }

    private static EventOptions CreateDefaultEventOptions() => new();
    private static MediatorOptions CreateDefaultMediatorOptions() => new();
    private static ChannelEventOptions CreateDefaultChannelEventOptions() => new();
}

/// <summary>
/// Builder for configuring CRISP options.
/// </summary>
public class CrispOptionsBuilder
{
    public readonly IServiceCollection services;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrispOptionsBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public CrispOptionsBuilder(IServiceCollection services) => services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Configures event options.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The options builder for chaining.</returns>
    public CrispOptionsBuilder ConfigureEvents(Action<EventOptions> configure)
    {
        EventOptions options = new();
        configure(options);
        services.ReplaceSingleton(options);
        return this;
    }

    /// <summary>
    /// Configures channel-based event processing options.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The options builder for chaining.</returns>
    public CrispOptionsBuilder ConfigureChannelEvents(Action<ChannelEventOptions> configure)
    {
        ChannelEventOptions options = new();
        configure(options);
        services.ReplaceSingleton(options);
        return this;
    }

    /// <summary>
    /// Enables channel-based event processing with optional configuration.
    /// </summary>
    /// <param name="configureChannel">Optional configuration for channel options.</param>
    /// <returns>The options builder for chaining.</returns>
    public CrispOptionsBuilder UseChannelEventProcessing(Action<ChannelEventOptions>? configureChannel = null)
    {
        // Configure event options to use channels
        EventOptions eventOptions = new() { UseChannels = true };
        services.ReplaceSingleton(eventOptions);

        // Configure channel options if provided
        if (configureChannel != null)
        {
            ChannelEventOptions channelOptions = new();
            configureChannel(channelOptions);
            services.ReplaceSingleton(channelOptions);
        }

        return this;
    }

    /// <summary>
    /// Configures mediator options.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The options builder for chaining.</returns>
    public CrispOptionsBuilder ConfigureMediator(Action<MediatorOptions> configure)
    {
        MediatorOptions options = new();
        configure(options);
        services.ReplaceSingleton(options);
        return this;
    }


}

/// <summary>
/// Extension methods for the service collection.
/// </summary>
public static class ServiceCollectionExtensions2
{
    /// <summary>
    /// Replaces a singleton registration in the service collection.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="instance">The instance to register.</param>
    public static void ReplaceSingleton<T>(this IServiceCollection services, T instance) where T : class
    {
        // Remove existing registration if any
        ServiceDescriptor? existingRegistration = services.FirstOrDefault(sd => sd.ServiceType == typeof(T));
        if (existingRegistration != null)
            services.Remove(existingRegistration);

        // Add new registration
        services.AddSingleton(instance);
    }
}