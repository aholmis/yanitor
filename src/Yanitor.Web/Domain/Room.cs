namespace Yanitor.Web.Domain;

/// <summary>
/// Represents a room in a house configuration.
/// </summary>
public record Room
{
    /// <summary>
    /// Gets the unique identifier for the room.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the custom name of the room (e.g., "Master Bedroom", "Kitchen").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the predefined type of the room.
    /// </summary>
    public required RoomType Type { get; init; }

    /// <summary>
    /// Gets when this room was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
