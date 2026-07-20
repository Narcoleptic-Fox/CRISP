using Microsoft.Extensions.Options;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Email;

/// <summary>
/// Provides dependency injection registration helpers for email infrastructure.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Registers the configured email sender implementation and validates email options on startup.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing email settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddEmailInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(ValidateEmailOptions, "Email configuration is invalid.")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
            return options.Provider switch
            {
                EmailProviders.None => serviceProvider.GetRequiredService<NullEmailSender>(),
                EmailProviders.Smtp => serviceProvider.GetRequiredService<SmtpEmailSender>(),
                EmailProviders.Ses => serviceProvider.GetRequiredService<SesEmailSender>(),
                _ => throw new InvalidOperationException($"Unknown email provider '{options.Provider}'.")
            };
        });

        services.AddSingleton<NullEmailSender>();
        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton<SesEmailSender>();

        return services;
    }

    private static bool ValidateEmailOptions(EmailOptions options)
    {
        if (options.Provider == EmailProviders.None)
            return true;

        if (string.IsNullOrWhiteSpace(options.From))
            return false;

        if (options.Provider == EmailProviders.Smtp)
            return !string.IsNullOrWhiteSpace(options.Smtp.Host) && options.Smtp.Port is > 0 and <= 65535;

        if (options.Provider == EmailProviders.Ses)
            return !string.IsNullOrWhiteSpace(options.Region);

        return false;
    }
}
