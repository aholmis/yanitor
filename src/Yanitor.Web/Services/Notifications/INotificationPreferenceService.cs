using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Service for managing user notification preferences.
/// </summary>
public interface INotificationPreferenceService
{
    /// <summary>
    /// Gets all notification preferences for the current user.
    /// </summary>
    Task<IReadOnlyList<NotificationPreference>> GetPreferencesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the notification preference for a specific method for the current user.
    /// </summary>
    Task<NotificationPreference?> GetPreferenceAsync(NotificationMethod method, CancellationToken ct = default);

    /// <summary>
    /// Saves or updates a notification preference for the current user.
    /// </summary>
    Task SavePreferenceAsync(NotificationPreference preference, CancellationToken ct = default);
}
