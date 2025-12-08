namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents a component or item of a house that requires maintenance or tracking.
/// </summary>
public record HouseItem
{
    /// <summary>
    /// Gets the name of the house item.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the type/category of the house item (e.g., HVAC, Plumbing, Door).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the general type of room (e.g., Bedroom, Kitchen, Bathroom).
    /// </summary>
    public required RoomType RoomType { get; init; }

    /// <summary>
    /// Gets the predefined maintenance tasks for this house item.
    /// </summary>
    public IReadOnlyList<MaintenanceTask> Tasks { get; init; } = Array.Empty<MaintenanceTask>();
}
