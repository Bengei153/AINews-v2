using AINews.Application.Common.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AINews.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromName { get; set; } = "AI Brief";
    public string FromEmail { get; set; } = default!;
    public bool UseSsl { get; set; } = true;
}

/// <summary>
/// Sends transactional email (welcome, password reset) and, later, newsletter
/// campaigns via SMTP using MimeKit/MailKit. Swap FromName/FromEmail via
/// configuration for a provider like Brevo, as noted in the product plan.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't let a flaky SMTP provider take down a request; log and move on.
            // Callers that need guaranteed delivery should queue via Hangfire instead.
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
        }
    }
}
