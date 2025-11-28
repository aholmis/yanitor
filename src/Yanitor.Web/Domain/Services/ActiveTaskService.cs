namespace Yanitor.Web.Domain.Services;

using Yanitor.Web.Domain.Components;

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
    /// <returns>The updated active task.</returns>
    Task<ActiveTask?> CompleteTaskAsync(Guid taskId);

    /// <summary>
    /// Gets the total count of active tasks.
    /// </summary>
    /// <returns>The number of active tasks.</returns>
    Task<int> GetTaskCountAsync();
}

/// <summary>
/// Default implementation of IActiveTaskService.
/// Generates active tasks based on house configuration and items.
/// </summary>
public class ActiveTaskService(
    IHouseConfigurationService houseConfigService,
    IItemProvider itemProvider) : IActiveTaskService
{
    private readonly Dictionary<Guid, ActiveTask> _completedTasks = new();

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

    public Task<ActiveTask?> CompleteTaskAsync(Guid taskId)
    {
        if (_completedTasks.TryGetValue(taskId, out var task))
        {
            var completedTask = task.MarkAsCompleted(DateTime.UtcNow);
            _completedTasks[taskId] = completedTask;
            return Task.FromResult<ActiveTask?>(completedTask);
        }

        return Task.FromResult<ActiveTask?>(null);
    }

    public async Task<int> GetTaskCountAsync()
    {
        var tasks = await GetActiveTasksAsync();
        return tasks.Count();
    }

    private ActiveTask CreateActiveTask(MaintenanceTask task, HouseItem item, Room room)
    {
        // For demo purposes, create tasks with staggered due dates
        // In production, this would be based on actual completion history
        var baseDate = DateTime.UtcNow;
        var taskHash = task.Id.GetHashCode();
        var daysOffset = Math.Abs(taskHash % task.IntervalDays);

        return new ActiveTask
        {
            Task = task,
            ItemName = item.Name,
            RoomName = room.Name,
            RoomType = room.Type,
            LastCompletedAt = null,
            NextDueDate = baseDate.AddDays(daysOffset)
        };
    }
}
