using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;

namespace Yanitor.Web.Services;

/// <summary>
/// Implements OTP generation, validation, and rate limiting for email authentication.
/// </summary>
public class OtpService(YanitorDbContext dbContext) : IOtpService
{
    private const int OtpLength = 6;
    private const int OtpExpirationMinutes = 15;
    private const int MaxOtpRequestsPerHour = 5;

    public async Task<string> GenerateOtpAsync(string email, string? ipAddress = null)
    {
        // Generate a secure random 6-digit code
        var code = GenerateSecureCode();

        var otp = new OneTimePassword
        {
            Email = email.ToLowerInvariant(),
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes),
            IsUsed = false,
            IpAddress = ipAddress
        };

        dbContext.OneTimePasswords.Add(otp);
        await dbContext.SaveChangesAsync();

        return code;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, string? ipAddress = null)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var now = DateTime.UtcNow;

        // Find the most recent unused, non-expired OTP for this email with matching code
        var otp = await dbContext.OneTimePasswords
            .Where(o => o.Email == normalizedEmail
                     && o.Code == code
                     && !o.IsUsed
                     && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return false;
        }

        // Mark as used
        otp.IsUsed = true;
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CanRequestOtpAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        // Count OTPs requested in the last hour
        var recentOtpCount = await dbContext.OneTimePasswords
            .Where(o => o.Email == normalizedEmail && o.CreatedAt > oneHourAgo)
            .CountAsync();

        return recentOtpCount < MaxOtpRequestsPerHour;
    }

    /// <summary>
    /// Generates a cryptographically secure 6-digit numeric code.
    /// </summary>
    private static string GenerateSecureCode()
    {
        // Generate a random number between 100 and 999,999
        var randomNumber = RandomNumberGenerator.GetInt32(100, 1_000_000);
        
        // Format as 6-digit string with leading zeros
        return randomNumber.ToString("D6");
    }
}
