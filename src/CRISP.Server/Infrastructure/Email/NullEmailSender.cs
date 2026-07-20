using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Email;

/// <summary>
/// Email sender used when outbound email delivery is disabled.
/// </summary>
internal sealed class NullEmailSender(ILogger<NullEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Logs that the message was skipped because email is disabled.
    /// </summary>
    /// <param name="message">The message that would have been sent.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A completed task.</returns>
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email provider is disabled. Skipped message to {Recipient} with subject {Subject}.",
            message.To,
            message.Subject);

        return Task.CompletedTask;
    }
}
