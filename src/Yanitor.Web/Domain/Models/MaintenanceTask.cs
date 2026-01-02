namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents a maintenance task for a house item.
/// </summary>
public record MaintenanceTask
{
    /// <summary>
    /// Gets the unique identifier for the task.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Localization key for the task name.
    /// </summary>
    public required string NameKey { get; init; }

    /// <summary>
    /// Localization key for the task description.
    /// </summary>
    public required string DescriptionKey { get; init; }

    /// <summary>
    /// Gets the recommended interval in days between task executions.
    /// </summary>
    public required int IntervalDays { get; init; }
}
