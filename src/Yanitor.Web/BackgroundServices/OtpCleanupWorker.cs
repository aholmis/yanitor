using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;

namespace Yanitor.Web.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up expired OTP records from the database.
/// Runs daily to remove OTPs older than 24 hours to prevent database bloat.
/// </summary>
public class OtpCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OtpCleanupWorker> logger) : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OtpCleanupWorker starting");

        // Wait a bit on startup to allow migrations and other initialization
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredOtpsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OtpCleanupWorker");
            }

            // Wait for next cleanup interval
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        logger.LogInformation("OtpCleanupWorker stopping");
    }

    private async Task CleanupExpiredOtpsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YanitorDbContext>();

        var cutoffTime = DateTime.UtcNow.AddHours(-24);

        // Delete OTPs that expired more than 24 hours ago
        var expiredOtps = await dbContext.OneTimePasswords
            .Where(otp => otp.ExpiresAt < cutoffTime)
            .ToListAsync(ct);

        if (expiredOtps.Any())
        {
            dbContext.OneTimePasswords.RemoveRange(expiredOtps);
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Cleaned up {Count} expired OTP records", expiredOtps.Count);
        }
        else
        {
            logger.LogDebug("No expired OTP records to clean up");
        }
    }
}
