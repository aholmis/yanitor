using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Domain.Services;

/// <summary>
/// Provides access to house items/components for the application.
/// </summary>
public interface IItemProvider
{
    /// <summary>
    /// Gets all house items available in the system.
    /// </summary>
    /// <returns>A collection of house items.</returns>
    IEnumerable<HouseItem> GetAllItems();

    /// <summary>
    /// Gets house items selected in the current house configuration.
    /// Falls back to empty if no configuration or no selections.
    /// </summary>
    Task<IEnumerable<HouseItem>> GetItemsForCurrentConfigurationAsync();
}

/// <summary>
/// Default implementation of IItemProvider that provides sample house items.
/// </summary>
public class ItemProvider(ITaskProvider taskProvider, IHouseConfigurationService houseConfigurationService) : IItemProvider
{
    private readonly List<HouseItem> _items = InitializeItems(taskProvider);
    private readonly IHouseConfigurationService _houseConfigService = houseConfigurationService;

    private static List<HouseItem> InitializeItems(ITaskProvider taskProvider)
    {
        return new List<HouseItem>
        {
            new HouseItem
            {
                Name = "Ventilation System",
                Type = "Ventilation",
                RoomType = RoomType.Other,
                Tasks = taskProvider.GetTasksForItemType("Ventilation").ToList()
            },
            new HouseItem
            {
                Name = "Master Bathroom Shower",
                Type = "Shower",
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType("Shower").ToList()
            },
            new HouseItem
            {
                Name = "Washing Machine",
                Type = "WashingMachine",
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType("WashingMachine").ToList()
            },
            new HouseItem
            {
                Name = "Dishwasher",
                Type = "Dishwasher",
                RoomType = RoomType.Kitchen,
                Tasks = taskProvider.GetTasksForItemType("Dishwasher").ToList()
            }
        };
    }

    public IEnumerable<HouseItem> GetAllItems() => _items;

    public async Task<IEnumerable<HouseItem>> GetItemsForCurrentConfigurationAsync()
    {
        var config = await _houseConfigService.GetConfigurationAsync();
        if (config == null || config.SelectedItemTypes.Count == 0)
        {
            return Enumerable.Empty<HouseItem>();
        }

        var selected = config.SelectedItemTypes;
        return _items.Where(i => selected.Contains(i.Type));
    }
}