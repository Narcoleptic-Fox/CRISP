using Crisp;
using Crisp.Builder;
using Crisp.Endpoints;
using Crisp.OpenApi;
using Crisp.Pipeline;
using Crisp.Events;
using Crisp.Runtime.Events;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for IServiceCollection to configure CRISP framework services.
/// </summary>
public static class CrispServiceCollectionExtensions
{
    /// <summary>
    /// Adds CRISP framework services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddCrisp(
        this IServiceCollection services,
        Action<NetCoreCrispBuilder>? configureBuilder = null)
    {
        // Step 1: Configure builder/options
        NetCoreCrispBuilder builder = new(services);
        
        // Register any assemblies that were added via AddHandlersFromAssembly
        var storedAssemblies = TestAssemblyStorage.GetAssemblies();
        if (storedAssemblies.Length > 0)
        {
            builder.RegisterHandlersFromAssemblies(storedAssemblies);
            TestAssemblyStorage.Clear(); // Clear after use
        }
        
        configureBuilder?.Invoke(builder);
        services.AddSingleton(builder.Options);

        // Step 2: Register core services
        services.AddSingleton<EndpointMapper>(provider => 
        {
            var options = provider.GetRequiredService<CrispOptions>();
            var mapper = new EndpointMapper(options);
            
            // Register assemblies from the builder
            var assemblies = builder.GetAssemblies();
            if (assemblies.Length > 0)
            {
                mapper.DiscoverEndpoints(assemblies);
            }
            
            return mapper;
        });
        services.AddSingleton<IEventPublisher, ChannelEventPublisher>();

        // Step 3: Register basic pipeline behaviors
        if (builder.Options.Pipeline.EnableLogging)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }

        if (builder.Options.Pipeline.EnableErrorHandling)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
        }

        // Step 4: Configure JSON options
        services.Configure<JsonOptions>(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy =
                builder.Options.Serialization.UseCamelCase ? JsonNamingPolicy.CamelCase : null;
            jsonOptions.JsonSerializerOptions.WriteIndented = builder.Options.Serialization.WriteIndented;
            jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition =
                builder.Options.Serialization.IgnoreNullValues
                    ? System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    : System.Text.Json.Serialization.JsonIgnoreCondition.Never;
        });

        // Step 5: Build and compile CRISP
        builder.Build();

        // Step 6: Add OpenAPI if enabled
        if (builder.Options.Endpoints.EnableOpenApi)
        {
            services.AddCrispSwagger();
        }

        return services;
    }
}

/// <summary>
/// Extension methods for fluent configuration of CRISP builders.
/// </summary>
public static class CrispBuilderExtensions
{
    /// <summary>
    /// Adds handlers from the specified assembly to the CRISP builder.
    /// This method stores assemblies in a static collection to be used during CRISP configuration.
    /// </summary>
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        // Store the assembly for later use during CRISP configuration
        TestAssemblyStorage.AddAssembly(assembly);
        return services;
    }
}

/// <summary>
/// Storage for test assemblies to be registered during CRISP configuration.
/// </summary>
internal static class TestAssemblyStorage
{
    private static readonly List<Assembly> _assemblies = new();

    public static void AddAssembly(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
    }

    public static Assembly[] GetAssemblies()
    {
        return _assemblies.ToArray();
    }

    public static void Clear()
    {
        _assemblies.Clear();
    }
}
