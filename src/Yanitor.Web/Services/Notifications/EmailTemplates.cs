using Microsoft.Extensions.Localization;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Provides HTML email templates with localization support.
/// </summary>
public static class EmailTemplates
{
    /// <summary>
    /// Generates an HTML email body for OTP authentication.
    /// </summary>
    /// <param name="localizer">String localizer for translations.</param>
    /// <param name="code">The 6-digit OTP code.</param>
    /// <param name="expirationMinutes">OTP expiration time in minutes.</param>
    /// <returns>HTML email body.</returns>
    public static string GetOtpEmailHtml(IStringLocalizer localizer, string code, int expirationMinutes)
    {
        var subject = localizer["Otp_Email_Subject"];
        var header = localizer["Otp_Email_Header"];
        var body = localizer["Otp_Email_Body"];
        var codeLabel = localizer["Otp_Email_Code"];
        var expiresIn = localizer["Otp_Email_ExpiresIn", expirationMinutes];
        var footer = localizer["Otp_Email_Footer"];

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{subject}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 40px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        h1 {{
            color: #1a1a1a;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .otp-code {{
            background-color: #f0f0f0;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 8px;
            text-align: center;
            padding: 20px;
            margin: 30px 0;
            color: #1a1a1a;
        }}
        .expiration {{
            color: #666;
            font-size: 14px;
            text-align: center;
            margin-top: -10px;
            margin-bottom: 30px;
        }}
        .footer {{
            color: #999;
            font-size: 12px;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>{header}</h1>
        <p>{body}</p>
        <div class=""otp-code"">{code}</div>
        <p class=""expiration"">{expiresIn}</p>
        <div class=""footer"">
            <p>{footer}</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gets the email subject for OTP emails.
    /// </summary>
    public static string GetOtpEmailSubject(IStringLocalizer localizer)
    {
        return localizer["Otp_Email_Subject"];
    }
}
