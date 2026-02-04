namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (supports HTML).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}
