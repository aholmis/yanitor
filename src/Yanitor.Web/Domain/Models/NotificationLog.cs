namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents a record of a sent notification for audit and tracking purposes.
/// </summary>
public record NotificationLog
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int TaskId { get; init; }
    public NotificationMethod Method { get; init; }
    public DateTime SentAt { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Recipient { get; init; }

    /// <summary>
    /// Creates a new notification log entry for a successful send.
    /// </summary>
    public static NotificationLog CreateSuccess(int userId, int taskId, NotificationMethod method, string recipient)
    {
        return new NotificationLog
        {
            UserId = userId,
            TaskId = taskId,
            Method = method,
            SentAt = DateTime.UtcNow,
            Success = true,
            Recipient = recipient
        };
    }

    /// <summary>
    /// Creates a new notification log entry for a failed send.
    /// </summary>
    public static NotificationLog CreateFailure(int userId, int taskId, NotificationMethod method, string recipient, string errorMessage)
    {
        return new NotificationLog
        {
            UserId = userId,
            TaskId = taskId,
            Method = method,
            SentAt = DateTime.UtcNow,
            Success = false,
            Recipient = recipient,
            ErrorMessage = errorMessage
        };
    }
}
