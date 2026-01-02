using Yanitor.Web.Domain.Models;

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
    /// Checks if a house configuration exists.
    /// </summary>
    /// <returns>True if a configuration exists, false otherwise.</returns>
    Task<bool> HasConfigurationAsync();

    /// <summary>
    /// Sets the selected item types present in the house.
    /// </summary>
    Task SetSelectedItemTypesAsync(IEnumerable<string> itemTypes);
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

    public async Task<bool> HasConfigurationAsync()
    {
        var config = await GetConfigurationAsync();
        return config != null && config.SelectedItemTypes.Count > 0;
    }

    public async Task SetSelectedItemTypesAsync(IEnumerable<string> itemTypes)
    {
        var config = await GetConfigurationAsync() ?? new HouseConfiguration();

        // return if no changes
        if (config.SelectedItemTypes.SetEquals(itemTypes))
        {
            return;
        }

        config.SelectedItemTypes.Clear();
        foreach (var t in itemTypes)
        {
            config.SelectedItemTypes.Add(t);
        }
        _configuration = config with { LastModifiedAt = DateTime.UtcNow };
    }
}
