using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;
using Yanitor.Web.Services.Reminders;

namespace Yanitor.Web.Tests;

public class TaskReminderCalculatorTests
{
    private static YanitorDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<YanitorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new YanitorDbContext(options);
    }

    [Fact]
    public async Task Returns_NoTasks_When_UserHasNoHouses()
    {
        using var db = CreateInMemoryDb();
        var calc = new TaskReminderCalculator();

        var pref = new NotificationPreferenceRow
        {
            UserId = Guid.NewGuid(),
            Method = (int) NotificationMethod.Email,
            IsEnabled = true
        };

        var result = await calc.GetTasksDueForReminderAsync(db,
            DateTime.UtcNow,
            pref, TestContext.Current.CancellationToken);
        Assert.Empty(result);
    }

    // Rule: A task is considered due for reminder when its NextDueDate is between 'now' (inclusive)
    // and 'now + ReminderDaysBeforeDue' (inclusive). If ReminderDaysBeforeDue is null it defaults to 1.
    [Theory]
    [InlineData(1, 2, true)]   // due in 1 day, reminder set to 2 days -> included
    [InlineData(10, 2, false)] // due in 10 days, reminder set to 2 days -> excluded
    [InlineData(0, 1, true)]   // due today, reminder 1 day -> included
    [InlineData(-1, 1, false)] // already past due -> excluded
    public async Task Returns_Tasks_Due_BasedOnReminderDays(int nextDueOffsetDays, int? reminderDaysBeforeDue, bool shouldBeIncluded)
    {
        using var db = CreateInMemoryDb();
        var userId = Guid.NewGuid();
        var house = new House { OwnerId = userId };
        db.Users.Add(new User { Id = userId, Email = "test@example.com" });
        db.Houses.Add(house);

        var now = DateTime.UtcNow;
        var targetTask = new ActiveTaskRow
        {
            HouseId = house.Id,
            NextDueDate = now.AddDays(nextDueOffsetDays),
            IntervalDays = 10,
            ItemName = "Item",
            TaskName = "T",
            TaskType = "Ventilation"
        };

        var later = new ActiveTaskRow
        {
            HouseId = house.Id,
            NextDueDate = now.AddDays(100),
            IntervalDays = 10,
            ItemName = "Item",
            TaskName = "Later",
            TaskType = "Ventilation"
        };

        db.ActiveTasks.AddRange(targetTask, later);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var calc = new TaskReminderCalculator();
        var pref = new NotificationPreferenceRow
        {
            UserId = userId,
            Method = (int) NotificationMethod.Email,
            IsEnabled = true,
            ReminderDaysBeforeDue = reminderDaysBeforeDue
        };

        var tasks = await calc.GetTasksDueForReminderAsync(db, now, pref, TestContext.Current.CancellationToken);

        if (shouldBeIncluded)
        {
            Assert.Contains(tasks, t => t.Id == targetTask.Id);
        }
        else
        {
            Assert.DoesNotContain(tasks, t => t.Id == targetTask.Id);
        }
        Assert.DoesNotContain(tasks, t => t.Id == later.Id);
    }

    // Rule: If the user has a PreferredTime set, reminders are only sent when the current time
    // is within +/- 30 minutes of that time. Otherwise no reminders are sent for that user.
    [Theory]
    [InlineData(10, true)]   // within +/-30min -> send
    [InlineData(30, true)]   // exactly 30min -> send
    [InlineData(31, false)]  // just outside 30min -> do not send
    [InlineData(-31, false)] // just outside 30min the other way -> do not send
    public async Task Respects_PreferredTime_Window(int preferredOffsetMinutes, bool shouldSend)
    {
        using var db = CreateInMemoryDb();
        var userId = Guid.NewGuid();
        var house = new House { OwnerId = userId };
        db.Users.Add(new User { Id = userId, Email = "test@example.com" });
        db.Houses.Add(house);

        var now = DateTime.UtcNow;
        var dueSoon = new ActiveTaskRow { HouseId = house.Id, NextDueDate = now.AddDays(1), IntervalDays = 10, ItemName = "Item", TaskName = "T", TaskType = "Ventilation" };
        db.ActiveTasks.Add(dueSoon);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var calc = new TaskReminderCalculator();
        var pref = new NotificationPreferenceRow
        {
            UserId = userId,
            Method = (int) NotificationMethod.Email,
            IsEnabled = true,
            PreferredTime = TimeOnly.FromDateTime(now.AddMinutes(preferredOffsetMinutes))
        };

        var tasks = await calc.GetTasksDueForReminderAsync(db, now, pref, TestContext.Current.CancellationToken);

        if (shouldSend)
        {
            Assert.Contains(tasks, t => t.Id == dueSoon.Id);
        }
        else
        {
            Assert.Empty(tasks);
        }
    }
}