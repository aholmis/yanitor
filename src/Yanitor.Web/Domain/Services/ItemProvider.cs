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
                ItemType = HouseItemType.Ventilation,
                RoomType = RoomType.Other,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.Ventilation).ToList()
            },
            new HouseItem
            {
                Name = "Master Bathroom Shower",
                ItemType = HouseItemType.Shower,
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.Shower).ToList()
            },
            new HouseItem
            {
                Name = "Washing Machine",
                ItemType = HouseItemType.WashingMachine,
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.WashingMachine).ToList()
            },
            new HouseItem
            {
                Name = "Dishwasher",
                ItemType = HouseItemType.Dishwasher,
                RoomType = RoomType.Kitchen,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.Dishwasher).ToList()
            },
            new HouseItem
            {
                Name = "Bathroom Sink",
                ItemType = HouseItemType.BathroomSink,
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.BathroomSink).ToList()
            },
            new HouseItem
            {
                Name = "Bathtub Drain",
                ItemType = HouseItemType.BathtubDrain,
                RoomType = RoomType.Bathroom,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.BathtubDrain).ToList()
            },
            new HouseItem
            {
                Name = "Interior Doors",
                ItemType = HouseItemType.InteriorDoor,
                RoomType = RoomType.Hall,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.InteriorDoor).ToList()
            },
            new HouseItem
            {
                Name = "Smoke Detectors",
                ItemType = HouseItemType.SmokeDetector,
                RoomType = RoomType.Hall,
                Tasks = taskProvider.GetTasksForItemType(HouseItemType.SmokeDetector).ToList()
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
        return _items.Where(i => selected.Contains(i.ItemType.ToString()));
    }
}