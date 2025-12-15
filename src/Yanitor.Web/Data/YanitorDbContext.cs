using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Data;

public class YanitorDbContext(DbContextOptions<YanitorDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<SelectedItemType> SelectedItemTypes => Set<SelectedItemType>();
    public DbSet<ActiveTaskRow> ActiveTasks => Set<ActiveTaskRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasOne(x => x.House)
                .WithOne(x => x.Owner)
                .HasForeignKey<House>(x => x.OwnerId)
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
    }
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid? HouseId { get; set; }
    public House? House { get; set; }
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
