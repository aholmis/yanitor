namespace Yanitor.Web.Services;

/// <summary>
/// Service for managing one-time password (OTP) authentication codes.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP for the specified email address.
    /// </summary>
    /// <param name="email">The email address to generate the OTP for.</param>
    /// <param name="ipAddress">The IP address of the requester (optional).</param>
    /// <returns>The generated 6-digit OTP code.</returns>
    Task<string> GenerateOtpAsync(string email, string? ipAddress = null);

    /// <summary>
    /// Validates an OTP code for the specified email address.
    /// </summary>
    /// <param name="email">The email address associated with the OTP.</param>
    /// <param name="code">The OTP code to validate.</param>
    /// <param name="ipAddress">The IP address of the requester (optional).</param>
    /// <returns>True if the OTP is valid and not expired; otherwise false.</returns>
    Task<bool> ValidateOtpAsync(string email, string code, string? ipAddress = null);

    /// <summary>
    /// Checks if the specified email address can request a new OTP based on rate limiting rules.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if a new OTP can be requested; otherwise false.</returns>
    Task<bool> CanRequestOtpAsync(string email);
}
