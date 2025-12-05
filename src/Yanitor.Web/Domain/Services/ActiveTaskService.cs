using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Domain.Services;
/// <summary>
/// Service for managing active maintenance tasks based on house configuration.
/// </summary>
public interface IActiveTaskService
{
    /// <summary>
    /// Gets all active tasks for the current house configuration.
    /// </summary>
    /// <returns>A collection of active tasks.</returns>
    Task<IEnumerable<ActiveTask>> GetActiveTasksAsync();

    /// <summary>
    /// Gets active tasks filtered by room type.
    /// </summary>
    /// <param name="roomType">The type of room to filter by.</param>
    /// <returns>A collection of active tasks for the specified room type.</returns>
    Task<IEnumerable<ActiveTask>> GetTasksByRoomTypeAsync(RoomType roomType);

    /// <summary>
    /// Gets the next upcoming task.
    /// </summary>
    /// <returns>The next task due, or null if no tasks exist.</returns>
    Task<ActiveTask?> GetNextTaskAsync();

    /// <summary>
    /// Gets all overdue tasks.
    /// </summary>
    /// <returns>A collection of overdue tasks.</returns>
    Task<IEnumerable<ActiveTask>> GetOverdueTasksAsync();

    /// <summary>
    /// Gets tasks due within the specified number of days.
    /// </summary>
    /// <param name="days">The number of days to look ahead.</param>
    /// <returns>A collection of tasks due within the specified period.</returns>
    Task<IEnumerable<ActiveTask>> GetTasksDueWithinAsync(int days);

    /// <summary>
    /// Marks a task as completed.
    /// </summary>
    /// <param name="taskId">The ID of the task to complete.</param>
    /// <param name="completedAt">The date when the task was completed. If null, uses current UTC time.</param>
    /// <returns>The updated active task.</returns>
    Task<ActiveTask?> CompleteTaskAsync(Guid taskId, DateTime? completedAt = null);

    /// <summary>
    /// Gets the total count of active tasks.
    /// </summary>
    /// <returns>The number of active tasks.</returns>
    Task<int> GetTaskCountAsync();
}

/// <summary>
/// Default implementation of IActiveTaskService.
/// Generates active tasks based on house configuration and items.
/// Uses raw user-entered names for room and item display.
/// </summary>
public class ActiveTaskService(
    IHouseConfigurationService houseConfigService,
    IItemProvider itemProvider) : IActiveTaskService
{
    // Store completion data by a composite key (item name + task id)
    private readonly Dictionary<string, (DateTime completedAt, DateTime nextDueDate)> _completionHistory = new();

    public async Task<IEnumerable<ActiveTask>> GetActiveTasksAsync()
    {
        var config = await houseConfigService.GetConfigurationAsync();
        if (config == null || config.Rooms.Count == 0)
        {
            return Enumerable.Empty<ActiveTask>();
        }

        var items = itemProvider.GetAllItems();
        var activeTasks = new List<ActiveTask>();

        // Generate tasks for items that match rooms in the configuration
        foreach (var item in items)
        {
            var matchingRoom = config.Rooms.FirstOrDefault(r =>
                r.Type.ToString().Equals(item.RoomType, StringComparison.OrdinalIgnoreCase));

            if (matchingRoom != null)
            {
                foreach (var task in item.Tasks)
                {
                    var activeTask = CreateActiveTask(task, item, matchingRoom);
                    activeTasks.Add(activeTask);
                }
            }
        }

        return activeTasks.OrderBy(t => t.NextDueDate);
    }

    public async Task<IEnumerable<ActiveTask>> GetTasksByRoomTypeAsync(RoomType roomType)
    {
        var allTasks = await GetActiveTasksAsync();
        return allTasks.Where(t => t.RoomType == roomType);
    }

    public async Task<ActiveTask?> GetNextTaskAsync()
    {
        var allTasks = await GetActiveTasksAsync();
        return allTasks.OrderBy(t => t.NextDueDate).FirstOrDefault();
    }

    public async Task<IEnumerable<ActiveTask>> GetOverdueTasksAsync()
    {
        var allTasks = await GetActiveTasksAsync();
        return allTasks.Where(t => t.IsOverdue).OrderBy(t => t.NextDueDate);
    }

    public async Task<IEnumerable<ActiveTask>> GetTasksDueWithinAsync(int days)
    {
        var allTasks = await GetActiveTasksAsync();
        return allTasks
            .Where(t => t.DaysUntilDue >= 0 && t.DaysUntilDue <= days)
            .OrderBy(t => t.NextDueDate);
    }

    public async Task<ActiveTask?> CompleteTaskAsync(Guid taskId, DateTime? completedAt = null)
    {
        // Find the task by ID
        var allTasks = await GetActiveTasksAsync();
        var task = allTasks.FirstOrDefault(t => t.Id == taskId);
        
        if (task != null)
        {
            var completionDate = completedAt ?? DateTime.UtcNow;
            
            // Store the completion history using a composite key
            var key = GetTaskKey(task.ItemName, task.Task.Id);
            var nextDue = completionDate.AddDays(task.Task.IntervalDays);
            _completionHistory[key] = (completionDate, nextDue);
            
            // Return the updated task
            var completedTask = task.MarkAsCompleted(completionDate);
            return completedTask;
        }

        return null;
    }

    public async Task<int> GetTaskCountAsync()
    {
        var tasks = await GetActiveTasksAsync();
        return tasks.Count();
    }

    private ActiveTask CreateActiveTask(MaintenanceTask task, HouseItem item, Room room)
    {
        var key = GetTaskKey(item.Name, task.Id);
        var itemName = item.Name; // use raw item name
        var roomName = room.Name; // use user-entered room name
        
        // Check if we have completion history for this task
        if (_completionHistory.TryGetValue(key, out var history))
        {
            // Use the stored completion history
            var taskId = GenerateTaskId(item.Name, task.Id, room.Name);
            return new ActiveTask
            {
                Id = taskId,
                Task = task,
                ItemName = itemName,
                RoomName = roomName,
                RoomType = room.Type,
                LastCompletedAt = history.completedAt,
                NextDueDate = history.nextDueDate
            };
        }
        
        // For demo purposes, create tasks with staggered due dates
        // In production, this would be based on actual completion history
        var baseDate = DateTime.UtcNow;
        var taskHash = task.Id.GetHashCode();
        var daysOffset = Math.Abs(taskHash % task.IntervalDays);
        var taskId2 = GenerateTaskId(item.Name, task.Id, room.Name);

        return new ActiveTask
        {
            Id = taskId2,
            Task = task,
            ItemName = itemName,
            RoomName = roomName,
            RoomType = room.Type,
            LastCompletedAt = null,
            NextDueDate = baseDate.AddDays(daysOffset)
        };
    }

    private static string GetTaskKey(string itemName, Guid taskId) => $"{itemName}_{taskId}";

    private static Guid GenerateTaskId(string itemName, Guid taskId, string roomName)
    {
        var combined = $"{itemName}_{taskId}_{roomName}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        return new Guid(bytes);
    }
}
