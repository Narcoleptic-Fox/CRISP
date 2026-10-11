using Microsoft.Extensions.Options;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Storage;

/// <summary>
/// Provides dependency injection registration helpers for object storage infrastructure.
/// </summary>
public static class ObjectStorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers the configured object storage implementation and validates options on startup.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing object storage settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddObjectStorageInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ObjectStorageOptions>()
            .Bind(configuration.GetSection(ObjectStorageOptions.SectionName))
            .Validate(ValidateObjectStorageOptions, "Object storage configuration is invalid.")
            .ValidateOnStart();

        services.AddSingleton<IObjectStorage>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ObjectStorageOptions>>().Value;
            return options.Provider switch
            {
                ObjectStorageProviders.None => serviceProvider.GetRequiredService<NullObjectStorage>(),
                ObjectStorageProviders.S3 => serviceProvider.GetRequiredService<S3ObjectStorage>(),
                _ => throw new InvalidOperationException($"Unknown object storage provider '{options.Provider}'.")
            };
        });

        services.AddSingleton<NullObjectStorage>();
        services.AddSingleton<S3ObjectStorage>();

        return services;
    }

    private static bool ValidateObjectStorageOptions(ObjectStorageOptions options)
    {
        if (options.Provider == ObjectStorageProviders.None)
            return true;

        if (options.Provider != ObjectStorageProviders.S3)
            return false;

        if (string.IsNullOrWhiteSpace(options.BucketName) || string.IsNullOrWhiteSpace(options.Region))
            return false;

        var hasAccessKey = !string.IsNullOrWhiteSpace(options.AccessKey);
        var hasSecretKey = !string.IsNullOrWhiteSpace(options.SecretKey);

        return hasAccessKey == hasSecretKey && options.DefaultPresignedUrlExpirationMinutes is >= 1 and <= 60;
    }
}