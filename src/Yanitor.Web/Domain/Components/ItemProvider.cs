namespace Yanitor.Web.Domain.Components;

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
    /// Gets house items grouped by room.
    /// </summary>
    /// <returns>A dictionary where the key is the room name and the value is a list of house items in that room.</returns>
    Dictionary<string, List<HouseItem>> GetItemsByRoom();
}

/// <summary>
/// Default implementation of IItemProvider that provides sample house items.
/// </summary>
public class ItemProvider(ITaskProvider taskProvider) : IItemProvider
{
    private readonly List<HouseItem> _items = InitializeItems(taskProvider);

    private static List<HouseItem> InitializeItems(ITaskProvider taskProvider)
    {
        return new List<HouseItem>
        {
            new HouseItem
            {
                Name = "Main HVAC System",
                Type = "HVAC",
                Room = "Garage",
                RoomType = "Garage",
                Tasks = taskProvider.GetTasksForItemType("HVAC").ToList()
            },
            new HouseItem
            {
                Name = "Water Heater",
                Type = "Plumbing",
                Room = "Garage",
                RoomType = "Garage",
                Tasks = taskProvider.GetTasksForItemType("Plumbing").ToList()
            },
            new HouseItem
            {
                Name = "Front Door",
                Type = "Door",
                Room = "Hall",
                RoomType = "Hall",
                Tasks = taskProvider.GetTasksForItemType("Door").ToList()
            },
            new HouseItem
            {
                Name = "Living Room Window",
                Type = "Window",
                Room = "Living Room",
                RoomType = "Living Room",
                Tasks = taskProvider.GetTasksForItemType("Window").ToList()
            },
            new HouseItem
            {
                Name = "Kitchen Sink",
                Type = "Plumbing",
                Room = "Kitchen",
                RoomType = "Kitchen",
                Tasks = taskProvider.GetTasksForItemType("Plumbing").ToList()
            },
            new HouseItem
            {
                Name = "Garage Door Opener",
                Type = "Garage",
                Room = "Garage",
                RoomType = "Garage",
                Tasks = taskProvider.GetTasksForItemType("Garage").ToList()
            },
            new HouseItem
            {
                Name = "Smoke Detector - Hallway",
                Type = "Safety",
                Room = "Hall",
                RoomType = "Hall",
                Tasks = taskProvider.GetTasksForItemType("Safety").ToList()
            },
            new HouseItem
            {
                Name = "Roof Shingles",
                Type = "Roof",
                Room = "Outdoor",
                RoomType = "Outdoor",
                Tasks = taskProvider.GetTasksForItemType("Roof").ToList()
            },
            new HouseItem
            {
                Name = "Gutter System",
                Type = "Exterior",
                Room = "Outdoor",
                RoomType = "Outdoor",
                Tasks = taskProvider.GetTasksForItemType("Exterior").ToList()
            },
            new HouseItem
            {
                Name = "Air Filter",
                Type = "HVAC",
                Room = "Garage",
                RoomType = "Garage",
                Tasks = taskProvider.GetTasksForItemType("HVAC").ToList()
            },
            new HouseItem
            {
                Name = "Sump Pump",
                Type = "Plumbing",
                Room = "Basement",
                RoomType = "Basement",
                Tasks = taskProvider.GetTasksForItemType("Plumbing").ToList()
            },
            new HouseItem
            {
                Name = "Attic Insulation",
                Type = "Insulation",
                Room = "Attic",
                RoomType = "Attic",
                Tasks = taskProvider.GetTasksForItemType("Insulation").ToList()
            },
            new HouseItem
            {
                Name = "Master Bathroom Shower",
                Type = "Plumbing",
                Room = "Master Bathroom",
                RoomType = "Bathroom",
                Tasks = taskProvider.GetTasksForItemType("Plumbing").ToList()
            },
            new HouseItem
            {
                Name = "Guest Bathroom Toilet",
                Type = "Plumbing",
                Room = "Guest Bathroom",
                RoomType = "Bathroom",
                Tasks = taskProvider.GetTasksForItemType("Plumbing").ToList()
            },
            new HouseItem
            {
                Name = "Bedroom Window",
                Type = "Window",
                Room = "Master Bedroom",
                RoomType = "Bedroom",
                Tasks = taskProvider.GetTasksForItemType("Window").ToList()
            }
        };
    }

    public IEnumerable<HouseItem> GetAllItems() => _items;

    public Dictionary<string, List<HouseItem>> GetItemsByRoom()
    {
        return _items
            .GroupBy(p => p.Room)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}