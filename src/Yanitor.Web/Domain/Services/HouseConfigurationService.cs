namespace Yanitor.Web.Domain.Services;

/// <summary>
/// Service for managing house configurations.
/// </summary>
public interface IHouseConfigurationService
{
    /// <summary>
    /// Gets the current house configuration.
    /// </summary>
    /// <returns>The house configuration, or null if none exists.</returns>
    Task<HouseConfiguration?> GetConfigurationAsync();

    /// <summary>
    /// Saves the house configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    Task SaveConfigurationAsync(HouseConfiguration configuration);

    /// <summary>
    /// Adds a room to the current house configuration.
    /// </summary>
    /// <param name="room">The room to add.</param>
    Task AddRoomAsync(Room room);

    /// <summary>
    /// Removes a room from the current house configuration.
    /// </summary>
    /// <param name="roomId">The ID of the room to remove.</param>
    Task RemoveRoomAsync(Guid roomId);

    /// <summary>
    /// Checks if a house configuration exists.
    /// </summary>
    /// <returns>True if a configuration exists, false otherwise.</returns>
    Task<bool> HasConfigurationAsync();
}

/// <summary>
/// In-memory implementation of IHouseConfigurationService.
/// TODO: Replace with persistent storage implementation.
/// </summary>
public class HouseConfigurationService : IHouseConfigurationService
{
    private HouseConfiguration? _configuration;

    public Task<HouseConfiguration?> GetConfigurationAsync()
    {
        return Task.FromResult(_configuration);
    }

    public Task SaveConfigurationAsync(HouseConfiguration configuration)
    {
        _configuration = configuration;
        return Task.CompletedTask;
    }

    public async Task AddRoomAsync(Room room)
    {
        var config = await GetConfigurationAsync();
        if (config == null)
        {
            config = new HouseConfiguration();
        }

        _configuration = config.AddRoom(room);
    }

    public async Task RemoveRoomAsync(Guid roomId)
    {
        var config = await GetConfigurationAsync();
        if (config == null)
        {
            return;
        }

        _configuration = config.RemoveRoom(roomId);
    }

    public async Task<bool> HasConfigurationAsync()
    {
        var config = await GetConfigurationAsync();
        return config != null && config.Rooms.Count > 0;
    }
}
