namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents the complete configuration of a house, including all rooms.
/// </summary>
public record HouseConfiguration
{
    /// <summary>
    /// Gets the unique identifier for this house configuration.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the collection of rooms in this house.
    /// </summary>
    public List<Room> Rooms { get; init; } = new();

    /// <summary>
    /// Gets when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets when this configuration was last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new house configuration with updated rooms.
    /// </summary>
    public HouseConfiguration WithRooms(List<Room> rooms)
    {
        return this with { Rooms = rooms, LastModifiedAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Adds a room to the house configuration.
    /// </summary>
    public HouseConfiguration AddRoom(Room room)
    {
        var updatedRooms = new List<Room>(Rooms) { room };
        return this with { Rooms = updatedRooms, LastModifiedAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Removes a room from the house configuration.
    /// </summary>
    public HouseConfiguration RemoveRoom(Guid roomId)
    {
        var updatedRooms = Rooms.Where(r => r.Id != roomId).ToList();
        return this with { Rooms = updatedRooms, LastModifiedAt = DateTime.UtcNow };
    }
}
