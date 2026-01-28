using Yanitor.Web.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Services;

namespace Yanitor.Web.Domain.Services;

/// <summary>
/// Service for managing active maintenance tasks based on house configuration.
/// </summary>
public interface IActiveTaskService
{
    Task<IEnumerable<ActiveTask>> GetActiveTasksAsync();
    Task<IEnumerable<ActiveTask>> GetTasksByRoomTypeAsync(RoomType roomType);
    Task<ActiveTask?> GetNextTaskAsync();
    Task<IEnumerable<ActiveTask>> GetOverdueTasksAsync();
    Task<IEnumerable<ActiveTask>> GetTasksDueWithinAsync(int days);
    Task<ActiveTask?> CompleteTaskAsync(Guid taskId, DateTime? completedAt = null);
    Task<int> GetTaskCountAsync();
    /// <summary>
    /// Ensures that active tasks exist for the current house configuration.
    /// Creates missing tasks for all selected items and their predefined tasks.
    /// </summary>
    Task SyncActiveTasksAsync();
}

/// <summary>
/// Active task service backed by EF Core persistence.
/// </summary>
public class ActiveTaskService : IActiveTaskService
{
    private readonly YanitorDbContext _db;
    private readonly IHouseConfigurationService _houseConfigService;
    private readonly IItemProvider _itemProvider;
    private readonly IUserContext _userContext;

    public ActiveTaskService(
        YanitorDbContext db,
        IHouseConfigurationService houseConfigService,
        IItemProvider itemProvider,
        IUserContext userContext)
    {
        _db = db;
        _houseConfigService = houseConfigService;
        _itemProvider = itemProvider;
        _userContext = userContext;
    }

    public async Task<IEnumerable<ActiveTask>> GetActiveTasksAsync()
    {
        var config = await _houseConfigService.GetConfigurationAsync();
        var houseId = await GetCurrentUserHouseIdAsync();

        if (config == null || config.SelectedItemTypes.Count == 0)
        {
            return Enumerable.Empty<ActiveTask>();
        }

        var types = config.SelectedItemTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var rows = await _db.ActiveTasks
            .Where(r => r.HouseId == houseId && types.Contains(r.TaskType))
            .OrderBy(r => r.NextDueDate)
            .AsNoTracking()
            .ToListAsync();

        return rows.Select(MapToDomain);
    }

    public async Task<IEnumerable<ActiveTask>> GetTasksByRoomTypeAsync(RoomType roomType)
    {
        var all = await GetActiveTasksAsync();
        return all.Where(t => t.Item.RoomType == roomType);
    }

    public async Task<ActiveTask?> GetNextTaskAsync()
    {
        var all = await GetActiveTasksAsync();
        return all.OrderBy(t => t.NextDueDate).FirstOrDefault();
    }

    public async Task<IEnumerable<ActiveTask>> GetOverdueTasksAsync()
    {
        var all = await GetActiveTasksAsync();
        return all.Where(t => t.IsOverdue).OrderBy(t => t.NextDueDate);
    }

    public async Task<IEnumerable<ActiveTask>> GetTasksDueWithinAsync(int days)
    {
        var all = await GetActiveTasksAsync();
        return all.Where(t => t.DaysUntilDue >= 0 && t.DaysUntilDue <= days)
            .OrderBy(t => t.NextDueDate);
    }

    public async Task<ActiveTask?> CompleteTaskAsync(Guid taskId, DateTime? completedAt = null)
    {
        var row = await _db.ActiveTasks.FirstOrDefaultAsync(r => r.Id == taskId);
        if (row == null) return null;
        var completionDate = completedAt ?? DateTime.UtcNow;
        row.LastCompletedAt = completionDate;
        row.NextDueDate = completionDate.AddDays(row.IntervalDays);
        await _db.SaveChangesAsync();
        return MapToDomain(row);
    }

    public async Task<int> GetTaskCountAsync()
    {
        var config = await _houseConfigService.GetConfigurationAsync();
        var houseId = await GetCurrentUserHouseIdAsync();

        if (config == null || config.SelectedItemTypes.Count == 0)
        {
            return 0;
        }

        var types = config.SelectedItemTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return await _db.ActiveTasks.CountAsync(r => r.HouseId == houseId && types.Contains(r.TaskType));
    }

    public async Task SyncActiveTasksAsync()
    {
        var config = await _houseConfigService.GetConfigurationAsync();
        var houseId = await GetCurrentUserHouseIdAsync();

        if (config == null || config.SelectedItemTypes.Count == 0)
        {
            return;
        }

        var selectedTypes = config.SelectedItemTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var items = (await _itemProvider.GetItemsForCurrentConfigurationAsync())
            .Where(i => selectedTypes.Contains(i.ItemType.ToString()))
            .ToList();

        // Existing tasks to avoid duplicates
        var existing = await _db.ActiveTasks
            .Where(r => r.HouseId == houseId)
            .Select(r => new { r.ItemName, r.TaskName })
            .ToListAsync();
        var existingSet = existing.Select(e => (e.ItemName, e.TaskName))
            .ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            foreach (var task in item.Tasks)
            {
                // Use resource key as canonical TaskName for persistence
                var taskKey = task.NameKey;
                var key = (item.Name, taskKey);
                if (existingSet.Contains(key))
                {
                    continue;
                }

                var row = new ActiveTaskRow
                {
                    HouseId = houseId,
                    ItemName = item.Name,
                    TaskName = taskKey,
                    TaskType = item.ItemType.ToString(),
                    RoomType = item.RoomType,
                    LastCompletedAt = null,
                    IntervalDays = task.IntervalDays,
                    NextDueDate = now.AddDays(task.IntervalDays)
                };
                _db.ActiveTasks.Add(row);
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task<Guid> GetCurrentUserHouseIdAsync()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        if (userId == null)
            throw new InvalidOperationException("No authenticated user");

        var house = await _db.Houses
            .Where(h => h.OwnerId == userId.Value)
            .Select(h => new { h.Id })
            .FirstOrDefaultAsync();

        if (house == null)
            throw new InvalidOperationException("User has no house configured");

        return house.Id;
    }

    private static ActiveTask MapToDomain(ActiveTaskRow r)
    {
        // TaskName stores the localized name key (e.g., "HVAC_ChangeAirFilter_Name").
        // Derive the description key using the naming convention when possible.
        var descriptionKey = r.TaskName.EndsWith("_Name", StringComparison.Ordinal)
            ? r.TaskName.Replace("_Name", "_Description", StringComparison.Ordinal)
            : r.TaskName;

        var mt = new MaintenanceTask
        {
            NameKey = r.TaskName,
            DescriptionKey = descriptionKey,
            IntervalDays = r.IntervalDays
        };

        // Build a lightweight HouseItem instance based on persisted data
        var itemType = Enum.TryParse<HouseItemType>(r.TaskType, true, out var parsed)
            ? parsed
            : HouseItemType.Ventilation;
        var item = new HouseItem
        {
            Name = r.ItemName,
            ItemType = itemType,
            RoomType = r.RoomType,
            Tasks = Array.Empty<MaintenanceTask>()
        };

        return new ActiveTask
        {
            Id = r.Id,
            Task = mt,
            Item = item,
            LastCompletedAt = r.LastCompletedAt,
            NextDueDate = r.NextDueDate
        };
    }
}
