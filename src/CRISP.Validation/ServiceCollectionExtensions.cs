using CRISP.Validation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring CRISP services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all validators in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for validators.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var validatorTypes = from assembly in assemblies
                             from type in assembly.GetTypes()
                             from @interface in type.GetInterfaces()
                             where @interface.IsGenericType &&
                                   @interface.GetGenericTypeDefinition() == typeof(IValidator<>)
                             select new { ValidatorType = type, InterfaceType = @interface };

        foreach (var validator in validatorTypes)
        {
            services.AddTransient(validator.InterfaceType, validator.ValidatorType);
        }

        services.AddSingleton(CreateDefaultValidationOptions());
        return services;
    }


    private static ValidationOptions CreateDefaultValidationOptions() => new();

    /// <summary>
    /// Configures validation options.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The options builder for chaining.</returns>
    public static CrispOptionsBuilder ConfigureValidation(this CrispOptionsBuilder builder, Action<ValidationOptions> configure)
    {
        ValidationOptions options = new();
        configure(options);
        builder.services.ReplaceSingleton(options);
        return builder;
    }
}