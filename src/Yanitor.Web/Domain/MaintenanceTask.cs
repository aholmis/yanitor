namespace Yanitor.Web.Domain;

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
    /// Gets the name of the maintenance task.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the detailed description of the task.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the recommended interval in days between task executions.
    /// </summary>
    public required int IntervalDays { get; init; }
}
