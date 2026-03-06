namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Configuration settings for the Mailjet email sender.
/// API key and secret are read from Azure Key Vault at runtime.
/// </summary>
public class MailjetSettings
{
    /// <summary>The verified sender email address.</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>The display name shown as the sender.</summary>
    public string FromName { get; set; } = "Yanitor";
}
