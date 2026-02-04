using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// EF Core implementation of notification preference management.
/// </summary>
public class NotificationPreferenceService(
    YanitorDbContext db,
    IUserContext userContext,
    ILogger<NotificationPreferenceService> logger) : INotificationPreferenceService
{
    public async Task<IReadOnlyList<NotificationPreference>> GetPreferencesAsync(CancellationToken ct = default)
    {
        var userId = await userContext.GetCurrentUserIdAsync()
            ?? throw new InvalidOperationException("No authenticated user");

        var rows = await db.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        // Return existing preferences, ensuring we have entries for all methods
        var preferences = new List<NotificationPreference>();
        foreach (var method in Enum.GetValues<NotificationMethod>())
        {
            var existing = rows.FirstOrDefault(r => r.Method == (int)method);
            if (existing != null)
            {
                preferences.Add(MapToDomain(existing));
            }
            else
            {
                // Return default preference for methods that haven't been configured
                preferences.Add(new NotificationPreference
                {
                    UserId = (int)userId.GetHashCode(),
                    Method = method,
                    IsEnabled = false,
                    ReminderDaysBeforeDue = 1
                });
            }
        }

        return preferences;
    }

    public async Task<NotificationPreference?> GetPreferenceAsync(NotificationMethod method, CancellationToken ct = default)
    {
        var userId = await userContext.GetCurrentUserIdAsync()
            ?? throw new InvalidOperationException("No authenticated user");

        var row = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Method == (int)method, ct);

        return row != null ? MapToDomain(row) : null;
    }

    public async Task SavePreferenceAsync(NotificationPreference preference, CancellationToken ct = default)
    {
        var userId = await userContext.GetCurrentUserIdAsync()
            ?? throw new InvalidOperationException("No authenticated user");

        var existingRow = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Method == (int)preference.Method, ct);

        if (existingRow != null)
        {
            existingRow.IsEnabled = preference.IsEnabled;
            existingRow.PreferredTime = preference.PreferredTime;
            existingRow.ReminderDaysBeforeDue = preference.ReminderDaysBeforeDue;
            logger.LogInformation("Updated notification preference for user {UserId}, method {Method}", userId, preference.Method);
        }
        else
        {
            var newRow = new NotificationPreferenceRow
            {
                UserId = userId,
                Method = (int)preference.Method,
                IsEnabled = preference.IsEnabled,
                PreferredTime = preference.PreferredTime,
                ReminderDaysBeforeDue = preference.ReminderDaysBeforeDue
            };
            db.NotificationPreferences.Add(newRow);
            logger.LogInformation("Created notification preference for user {UserId}, method {Method}", userId, preference.Method);
        }

        await db.SaveChangesAsync(ct);
    }

    private static NotificationPreference MapToDomain(NotificationPreferenceRow row) => new()
    {
        Id = (int)row.Id.GetHashCode(),
        UserId = (int)row.UserId.GetHashCode(),
        Method = (NotificationMethod)row.Method,
        IsEnabled = row.IsEnabled,
        PreferredTime = row.PreferredTime,
        ReminderDaysBeforeDue = row.ReminderDaysBeforeDue
    };
}
