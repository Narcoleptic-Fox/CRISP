using Microsoft.Extensions.Options;
using CRISP.Server.Common;
using System.Net;
using System.Net.Mail;

namespace CRISP.Server.Infrastructure.Email;

/// <summary>
/// Sends application email through an SMTP server.
/// </summary>
internal sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    /// <summary>
    /// Sends an email message through the configured SMTP server.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>A task that completes when the SMTP server accepts the message.</returns>
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false,
        };
        mailMessage.To.Add(message.To);

        using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = _options.Smtp.EnableSsl,
        };

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        await client.SendMailAsync(mailMessage, cancellationToken);
        logger.LogInformation("Sent email to {Recipient} with subject {Subject}.", message.To, message.Subject);
    }
}
