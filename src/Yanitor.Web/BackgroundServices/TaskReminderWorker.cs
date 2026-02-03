using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;
using Yanitor.Web.Services.Notifications;
using Yanitor.Web.Services.Reminders;

namespace Yanitor.Web.BackgroundServices;

/// <summary>
/// Background service that checks for tasks due for reminders and sends notifications.
/// Runs periodically to check active tasks and notify users based on their preferences.
/// </summary>
public class TaskReminderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TaskReminderWorker> logger,
    TaskReminderCalculator calculator) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TaskReminderWorker starting");

        // Wait a bit on startup to allow migrations and other initialization
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in TaskReminderWorker");
            }

            // Wait for next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        logger.LogInformation("TaskReminderWorker stopping");
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YanitorDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;

        // Get all users with enabled email notifications
        var usersWithPreferences = await dbContext.NotificationPreferences
            .Where(p => p.Method == (int)NotificationMethod.Email && p.IsEnabled)
            .ToListAsync(ct);

        logger.LogInformation("Checking reminders for {Count} users with email preferences", usersWithPreferences.Count);

        foreach (var preference in usersWithPreferences)
        {
            try
            {
                var tasksDueForReminder = await calculator.GetTasksDueForReminderAsync(dbContext, now, preference, ct);
          
                foreach (var task in tasksDueForReminder)
                {
                    // Check if we've already sent a reminder for this task recently (within last 24 hours)
                    var recentLog = await dbContext.NotificationLogs
                        .Where(l => l.UserId == preference.UserId
                            && l.TaskId == task.Id
                            && l.SentAt >= now.AddHours(-24))
                        .AnyAsync(ct);

                    if (recentLog)
                    {
                        logger.LogDebug("Skipping task {TaskId} - reminder already sent in last 24 hours", task.Id);
                        continue;
                    }

                    // Send reminder
                    logger.LogInformation("Sending reminder for task {TaskId} to user {UserId}", task.Id, preference.UserId);
                    await notificationService.SendTaskReminderAsync(preference.UserId, task.Id, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing reminders for user {UserId}", preference.UserId);
            }
        }

        logger.LogInformation("Reminder check completed");
    }
}
