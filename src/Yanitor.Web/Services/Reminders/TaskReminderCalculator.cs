using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Services.Reminders
{
    /// <summary>
    /// Encapsulates the calculation logic for which tasks are due for reminders.
    /// Extracted from the background worker so it can be unit tested in isolation.
    /// </summary>
    public class TaskReminderCalculator
    {
        /// <summary>
        /// Returns whether reminders should be sent for the given preference and the list of tasks to remind for.
        /// The boolean indicates if processing should continue for the user (true) or be skipped (false).
        /// </summary>
        public async Task<IEnumerable<ActiveTaskRow>> GetTasksDueForReminderAsync(
            YanitorDbContext dbContext,
            DateTime now,
            NotificationPreferenceRow preference,
            CancellationToken ct)
        {
            // Derive current time-of-day from the provided 'now'
            var currentTime = TimeOnly.FromDateTime(now);

            // Check if it's the right time to send notifications for this user
            if (preference.PreferredTime.HasValue)
            {
                var preferredTime = preference.PreferredTime.Value;
                var timeDifference = Math.Abs((currentTime - preferredTime).TotalMinutes);

                // Only send if within 30 minutes of preferred time
                if (timeDifference > 30)
                {
                    return Enumerable.Empty<ActiveTaskRow>();
                }
            }

            var reminderDays = preference.ReminderDaysBeforeDue ?? 1;
            var reminderThreshold = now.AddDays(reminderDays);

            // Get user's houses
            var userHouses = await dbContext.Houses
                .Where(h => h.OwnerId == preference.UserId)
                .Select(h => h.Id)
                .ToListAsync(ct);

            if (!userHouses.Any())
            {
                return Enumerable.Empty<ActiveTaskRow>();
            }

            // Find tasks that are due within the reminder threshold
            var tasksDueForReminder = await dbContext.ActiveTasks
                .Where(t => userHouses.Contains(t.HouseId)
                    && t.NextDueDate <= reminderThreshold
                    && t.NextDueDate >= now)
                .ToListAsync(ct);

            return tasksDueForReminder;
        }
    }
}
