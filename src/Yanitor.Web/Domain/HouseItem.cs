namespace Yanitor.Web.Domain;

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
    /// Gets the room where this item is located.
    /// </summary>
    public required string Room { get; init; }

    /// <summary>
    /// Gets the general type of room (e.g., Bedroom, Kitchen, Bathroom).
    /// </summary>
    public required string RoomType { get; init; }
}
