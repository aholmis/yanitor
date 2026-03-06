using Azure.Security.KeyVault.Secrets;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Options;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Email sender implementation using the Mailjet transactional email API.
/// API credentials are fetched lazily from Azure Key Vault on first use
/// and cached for the lifetime of the singleton.
/// </summary>
public sealed class MailjetEmailSender : IEmailSender, IAsyncDisposable
{
    private readonly SecretClient _secretClient;
    private readonly MailjetSettings _settings;
    private readonly ILogger<MailjetEmailSender> _logger;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private string? _apiKey;
    private string? _apiSecret;

    public MailjetEmailSender(
        SecretClient secretClient,
        IOptions<MailjetSettings> settings,
        ILogger<MailjetEmailSender> logger)
    {
        _secretClient = secretClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        await EnsureCredentialsLoadedAsync(ct);

        var client = new MailjetClient(_apiKey, _apiSecret);

        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact(_settings.FromEmail, _settings.FromName))
            .WithSubject(subject)
            .WithHtmlPart(body)
            .WithTo(new SendContact(to))
            .Build();

        _logger.LogDebug("Sending email via Mailjet to {To} with subject {Subject}", to, subject);

        var response = await client.SendTransactionalEmailAsync(email);

        if (response.Messages is { Length: > 0 })
        {
            _logger.LogInformation(
                "Email sent via Mailjet to {To}. Status: {Status}",
                to,
                response.Messages[0].Status);
        }
        else
        {
            _logger.LogWarning("Mailjet response contained no message details for recipient {To}", to);
        }
    }

    /// <summary>
    /// Fetches API credentials from Key Vault the first time an email is sent,
    /// then caches them for subsequent calls.
    /// </summary>
    private async Task EnsureCredentialsLoadedAsync(CancellationToken ct)
    {
        if (_apiKey is not null && _apiSecret is not null)
            return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_apiKey is not null && _apiSecret is not null)
                return;

            _logger.LogInformation("Fetching Mailjet credentials from Azure Key Vault");

            var keyResponse = await _secretClient.GetSecretAsync("mailjet-primary-apikey", cancellationToken: ct);
            var secretResponse = await _secretClient.GetSecretAsync("mailjet-primary-secretkey", cancellationToken: ct);

            _apiKey = keyResponse.Value.Value;
            _apiSecret = secretResponse.Value.Value;

            _logger.LogInformation("Mailjet credentials loaded from Key Vault");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Mailjet credentials from Azure Key Vault");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _initLock.Dispose();
        return ValueTask.CompletedTask;
    }
}
