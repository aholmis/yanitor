using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Service for sending notifications to users about task reminders.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a task reminder notification to a user.
    /// </summary>
    /// <param name="userId">The ID of the user to notify.</param>
    /// <param name="taskId">The ID of the task to remind about.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendTaskReminderAsync(Guid userId, Guid taskId, CancellationToken ct = default);

    /// <summary>
    /// Gets notification preferences for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<NotificationPreference>> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates or creates a notification preference for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="method">The notification method.</param>
    /// <param name="isEnabled">Whether the method is enabled.</param>
    /// <param name="preferredTime">Optional preferred time for notifications.</param>
    /// <param name="reminderDaysBeforeDue">Days before due date to send reminder.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdatePreferenceAsync(
        Guid userId, 
        NotificationMethod method, 
        bool isEnabled, 
        TimeOnly? preferredTime = null, 
        int? reminderDaysBeforeDue = null,
        CancellationToken ct = default);
}
