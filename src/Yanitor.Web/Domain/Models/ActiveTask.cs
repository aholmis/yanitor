namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents an active maintenance task with scheduling information.
/// </summary>
public record ActiveTask
{
    /// <summary>
    /// Gets the unique identifier for the active task instance.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the underlying maintenance task definition.
    /// </summary>
    public required MaintenanceTask Task { get; init; }

    /// <summary>
    /// Gets the house item this task is for.
    /// </summary>
    public required HouseItem Item { get; init; }

    /// <summary>
    /// Gets the date when this task was last completed.
    /// If null, the task has never been completed.
    /// </summary>
    public DateTime? LastCompletedAt { get; init; }

    /// <summary>
    /// Gets the date when this task is next due.
    /// </summary>
    public DateTime NextDueDate { get; init; }

    /// <summary>
    /// Gets the number of days until this task is due.
    /// Negative values indicate overdue tasks.
    /// </summary>
    public int DaysUntilDue => (NextDueDate.Date - DateTime.UtcNow.Date).Days;

    /// <summary>
    /// Gets whether this task is overdue.
    /// </summary>
    public bool IsOverdue => DaysUntilDue < 0;

    /// <summary>
    /// Gets whether this task is due soon (within 7 days).
    /// </summary>
    public bool IsDueSoon => DaysUntilDue >= 0 && DaysUntilDue <= 7;

    /// <summary>
    /// Creates a new active task with updated completion information.
    /// </summary>
    public ActiveTask MarkAsCompleted(DateTime completedAt)
    {
        return this with
        {
            LastCompletedAt = completedAt,
            NextDueDate = completedAt.AddDays(Task.IntervalDays)
        };
    }
}
