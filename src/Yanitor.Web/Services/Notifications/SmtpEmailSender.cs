using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Email sender implementation using SMTP.
/// </summary>
public class SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message, ct);
            
            logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
