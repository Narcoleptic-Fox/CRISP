namespace CRISP.Server.Common;

/// <summary>
/// Defines a service that sends application email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message through the configured provider.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>A task that completes when the provider has accepted the message.</returns>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an outbound email message to send through the configured email provider.
/// </summary>
/// <param name="To">The recipient email address.</param>
/// <param name="Subject">The message subject.</param>
/// <param name="Body">The plain-text message body.</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body);
