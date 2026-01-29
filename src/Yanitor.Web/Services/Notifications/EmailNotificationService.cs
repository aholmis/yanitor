using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;
using Yanitor.Web.Domain.Services;
using Yanitor.Web.Resources;

namespace Yanitor.Web.Services.Notifications;

/// <summary>
/// Email-based notification service for task reminders.
/// </summary>
public class EmailNotificationService(
    YanitorDbContext dbContext,
    IEmailSender emailSender,
    IStringLocalizer<SharedResources> localizer,
    ILogger<EmailNotificationService> logger) : INotificationService
{
    public async Task SendTaskReminderAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for task reminder", userId);
            return;
        }

        var task = await dbContext.ActiveTasks.FindAsync([taskId], cancellationToken);
        if (task == null)
        {
            logger.LogWarning("Task {TaskId} not found for reminder", taskId);
            return;
        }

        var preference = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Method == (int)NotificationMethod.Email && p.IsEnabled, cancellationToken);

        if (preference == null)
        {
            logger.LogInformation("Email notifications not enabled for user {UserId}", userId);
            return;
        }

        try
        {
            var subject = $"Task Reminder: {task.TaskName}";
            var daysUntilDue = (task.NextDueDate - DateTime.UtcNow).Days;
            
            var body = $@"
                <html>
                <body>
                    <h2>Task Reminder</h2>
                    <p>Hello {user.DisplayName ?? user.Email},</p>
                    <p>This is a reminder that the following task is due soon:</p>
                    <ul>
                        <li><strong>Item:</strong> {task.ItemName}</li>
                        <li><strong>Task:</strong> {task.TaskName}</li>
                        <li><strong>Due Date:</strong> {task.NextDueDate:yyyy-MM-dd}</li>
                        <li><strong>Days Until Due:</strong> {daysUntilDue}</li>
                    </ul>
                    <p>Please complete this task to keep your home maintenance on track.</p>
                    <p>Best regards,<br/>Yanitor</p>
                </body>
                </html>";

            await emailSender.SendEmailAsync(user.Email, subject, body, cancellationToken);

            // Log successful notification
            var log = new NotificationLogRow
            {
                UserId = userId,
                TaskId = taskId,
                Method = (int)NotificationMethod.Email,
                SentAt = DateTime.UtcNow,
                Success = true,
                Recipient = user.Email
            };
            dbContext.NotificationLogs.Add(log);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Task reminder sent to {Email} for task {TaskId}", user.Email, taskId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send task reminder to {Email} for task {TaskId}", user.Email, taskId);

            // Log failed notification
            var log = new NotificationLogRow
            {
                UserId = userId,
                TaskId = taskId,
                Method = (int)NotificationMethod.Email,
                SentAt = DateTime.UtcNow,
                Success = false,
                Recipient = user.Email,
                ErrorMessage = ex.Message
            };
            dbContext.NotificationLogs.Add(log);
            await dbContext.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    public async Task<IEnumerable<NotificationPreference>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await dbContext.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        return preferences.Select(p => new NotificationPreference
        {
            Id = 0, // Domain model doesn't use Guid, could refactor
            UserId = 0, // Would need to map Guid to int or refactor domain model
            Method = (NotificationMethod)p.Method,
            IsEnabled = p.IsEnabled,
            PreferredTime = p.PreferredTime,
            ReminderDaysBeforeDue = p.ReminderDaysBeforeDue
        });
    }

    public async Task UpdatePreferenceAsync(
        Guid userId,
        NotificationMethod method,
        bool isEnabled,
        TimeOnly? preferredTime = null,
        int? reminderDaysBeforeDue = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Method == (int)method, cancellationToken);

        if (existing != null)
        {
            existing.IsEnabled = isEnabled;
            existing.PreferredTime = preferredTime;
            existing.ReminderDaysBeforeDue = reminderDaysBeforeDue;
        }
        else
        {
            var newPreference = new NotificationPreferenceRow
            {
                UserId = userId,
                Method = (int)method,
                IsEnabled = isEnabled,
                PreferredTime = preferredTime,
                ReminderDaysBeforeDue = reminderDaysBeforeDue
            };
            dbContext.NotificationPreferences.Add(newPreference);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated notification preference for user {UserId}, method {Method}", userId, method);
    }
}
