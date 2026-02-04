namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents a user's notification preferences including delivery method and preferred times.
/// </summary>
public record NotificationPreference
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public NotificationMethod Method { get; init; }
    public bool IsEnabled { get; init; }
    public TimeOnly? PreferredTime { get; init; }
    public int? ReminderDaysBeforeDue { get; init; }

    /// <summary>
    /// Creates a new notification preference with updated enabled status.
    /// </summary>
    public NotificationPreference WithEnabled(bool enabled) => this with { IsEnabled = enabled };

    /// <summary>
    /// Creates a new notification preference with updated preferred time.
    /// </summary>
    public NotificationPreference WithPreferredTime(TimeOnly? time) => this with { PreferredTime = time };
}
