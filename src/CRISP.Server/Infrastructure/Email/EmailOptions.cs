using System.ComponentModel.DataAnnotations;

namespace CRISP.Server.Infrastructure.Email;

/// <summary>
/// Configures the application's outbound email provider.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// The configuration section name for email options.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Gets the configured email provider name.
    /// </summary>
    public string Provider { get; init; } = EmailProviders.None;

    /// <summary>
    /// Gets the sender address used for outbound messages.
    /// </summary>
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// Gets the AWS region used by the SES provider.
    /// </summary>
    public string Region { get; init; } = "us-east-1";

    /// <summary>
    /// Gets the SMTP-specific provider options.
    /// </summary>
    public SmtpEmailOptions Smtp { get; init; } = new();
}

/// <summary>
/// Configures SMTP provider connection settings.
/// </summary>
public sealed class SmtpEmailOptions
{
    /// <summary>
    /// Gets the SMTP server host name.
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SMTP server port.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 25;

    /// <summary>
    /// Gets a value indicating whether SSL is enabled for SMTP connections.
    /// </summary>
    public bool EnableSsl { get; init; }

    /// <summary>
    /// Gets the optional SMTP username.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets the optional SMTP password.
    /// </summary>
    public string? Password { get; init; }
}

/// <summary>
/// Provides supported email provider names.
/// </summary>
public static class EmailProviders
{
    /// <summary>
    /// Disables outbound email delivery.
    /// </summary>
    public const string None = "None";

    /// <summary>
    /// Sends email through an SMTP server.
    /// </summary>
    public const string Smtp = "Smtp";

    /// <summary>
    /// Sends email through Amazon Simple Email Service.
    /// </summary>
    public const string Ses = "Ses";
}
