using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Data;

public class YanitorDbContext(DbContextOptions<YanitorDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<SelectedItemType> SelectedItemTypes => Set<SelectedItemType>();
    public DbSet<ActiveTaskRow> ActiveTasks => Set<ActiveTaskRow>();
    public DbSet<NotificationPreferenceRow> NotificationPreferences => Set<NotificationPreferenceRow>();
    public DbSet<NotificationLogRow> NotificationLogs => Set<NotificationLogRow>();
    public DbSet<OneTimePassword> OneTimePasswords => Set<OneTimePassword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.CreatedAt).IsRequired();
            b.HasMany(x => x.Houses)
                .WithOne(x => x.Owner)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<House>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<SelectedItemType>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.HouseId, x.Type }).IsUnique();
            b.Property(x => x.Type).HasMaxLength(200).IsRequired();
            b.HasOne<House>()
                .WithMany(x => x.SelectedItemTypes)
                .HasForeignKey(x => x.HouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActiveTaskRow>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            b.Property(x => x.TaskName).HasMaxLength(200).IsRequired();
            b.Property(x => x.TaskType).HasMaxLength(200).IsRequired();
            b.Property(x => x.RoomType).IsRequired();
            b.Property(x => x.IntervalDays).IsRequired();
            b.Property(x => x.NextDueDate).IsRequired();
            b.HasIndex(x => x.NextDueDate);
            b.HasIndex(x => x.HouseId);
            b.HasOne<House>()
                .WithMany()
                .HasForeignKey(x => x.HouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationPreferenceRow>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.Method }).IsUnique();
            b.Property(x => x.Method).IsRequired();
            b.Property(x => x.IsEnabled).IsRequired();
            b.Property(x => x.ReminderDaysBeforeDue).HasDefaultValue(1);
            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationLogRow>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Method).IsRequired();
            b.Property(x => x.SentAt).IsRequired();
            b.Property(x => x.Success).IsRequired();
            b.Property(x => x.Recipient).HasMaxLength(320);
            b.Property(x => x.ErrorMessage).HasMaxLength(1000);
            b.HasIndex(x => new { x.UserId, x.SentAt });
            b.HasIndex(x => x.TaskId);
            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OneTimePassword>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.Code).HasMaxLength(6).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
            b.Property(x => x.IsUsed).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(45); // IPv6 max length
            b.HasIndex(x => new { x.Email, x.CreatedAt });
            b.HasIndex(x => x.ExpiresAt);
        });
    }
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; } = false;
    
    public ICollection<House> Houses { get; set; } = new List<House>();
}

public class House
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SelectedItemType> SelectedItemTypes { get; set; } = new List<SelectedItemType>();
}

public class SelectedItemType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HouseId { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class ActiveTaskRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HouseId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty; // item Type key used to match SelectedItemTypes
    public RoomType RoomType { get; set; } = RoomType.Other;
    public DateTime? LastCompletedAt { get; set; }
    public DateTime NextDueDate { get; set; }
    public int IntervalDays { get; set; }
}

public class NotificationPreferenceRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int Method { get; set; } // NotificationMethod enum as int
    public bool IsEnabled { get; set; } = true;
    public TimeOnly? PreferredTime { get; set; }
    public int? ReminderDaysBeforeDue { get; set; } = 1;
}

public class NotificationLogRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid TaskId { get; set; }
    public int Method { get; set; } // NotificationMethod enum as int
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Recipient { get; set; }
}
public class OneTimePassword
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string? IpAddress { get; set; }
}