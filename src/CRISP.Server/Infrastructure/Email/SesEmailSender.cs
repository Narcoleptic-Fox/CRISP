using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Options;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Email;

/// <summary>
/// Sends application email through Amazon Simple Email Service.
/// </summary>
internal sealed class SesEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SesEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    /// <summary>
    /// Sends an email message through Amazon SES.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>A task that completes when SES accepts the message.</returns>
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(_options.Region));
        var request = new SendEmailRequest
        {
            FromEmailAddress = _options.From,
            Destination = new Destination
            {
                ToAddresses = [message.To]
            },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Data = message.Subject,
                        Charset = "UTF-8"
                    },
                    Body = new Body
                    {
                        Text = new Content
                        {
                            Data = message.Body,
                            Charset = "UTF-8"
                        }
                    }
                }
            }
        };

        await client.SendEmailAsync(request, cancellationToken);
        logger.LogInformation("Sent SES email to {Recipient} with subject {Subject}.", message.To, message.Subject);
    }
}
