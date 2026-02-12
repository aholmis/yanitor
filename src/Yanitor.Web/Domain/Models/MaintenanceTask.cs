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
    /// Localization key for the detailed task description (optional).
    /// </summary>
    public string? DetailedDescriptionKey { get; init; }

    /// <summary>
    /// URL to an instructional video for this task (optional).
    /// </summary>
    public string? VideoUrl { get; init; }

    /// <summary>
    /// List of product links relevant to this task (optional).
    /// </summary>
    public IReadOnlyList<ProductLink>? ProductLinks { get; init; }

    /// <summary>
    /// Gets the recommended interval in days between task executions.
    /// </summary>
    public required int IntervalDays { get; init; }
}

/// <summary>
/// Represents a product link related to a maintenance task.
/// </summary>
public record ProductLink
{
    /// <summary>
    /// Localization key for the product name.
    /// </summary>
    public required string NameKey { get; init; }

    /// <summary>
    /// URL to the product page.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Optional localization key for additional description.
    /// </summary>
    public string? DescriptionKey { get; init; }
}
