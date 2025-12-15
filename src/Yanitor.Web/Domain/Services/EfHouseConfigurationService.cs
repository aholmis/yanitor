using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Yanitor.Web.Domain.Services;

public class EfHouseConfigurationService
    : IHouseConfigurationService
{
    private readonly YanitorDbContext _db;
    private readonly IServiceProvider _services;

    public EfHouseConfigurationService(YanitorDbContext db, IServiceProvider services)
    {
        _db = db;
        _services = services;
    }

    public async Task<HouseConfiguration?> GetConfigurationAsync()
    {
        var user = await EnsureDefaultUserAsync(_db);
        var house = await _db.Houses.Include(h => h.SelectedItemTypes)
            .FirstOrDefaultAsync(h => h.OwnerId == user.Id);
        if (house == null) return null;
        var config = new HouseConfiguration();
        foreach (var t in house.SelectedItemTypes.Select(s => s.Type))
        {
            config.SelectedItemTypes.Add(t);
        }
        return config;
    }

    public async Task SaveConfigurationAsync(HouseConfiguration configuration)
    {
        var user = await EnsureDefaultUserAsync(_db);
        var house = await _db.Houses.Include(h => h.SelectedItemTypes)
            .FirstOrDefaultAsync(h => h.OwnerId == user.Id);

        // Create the house if it does not exist and save immediately to get a generated Id
        if (house == null)
        {
            house = new House { OwnerId = user.Id };
            _db.Houses.Add(house);
            await _db.SaveChangesAsync();
            // Ensure navigation collection is initialized after creation
            _db.Entry(house).Collection(h => h.SelectedItemTypes).Load();
        }

        var desired = configuration.SelectedItemTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Remove deselected item types
        foreach (var existing in house.SelectedItemTypes.ToList())
        {
            if (!desired.Contains(existing.Type))
            {
                _db.SelectedItemTypes.Remove(existing);
            }
        }

        // Add newly selected item types
        var existingSet = house.SelectedItemTypes.Select(s => s.Type)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var type in desired)
        {
            if (!existingSet.Contains(type))
            {
                // Add via navigation collection; EF will set the FK (HouseId) automatically
                _db.SelectedItemTypes.Add(new SelectedItemType { Type = type, HouseId = house.Id });
            }
        }

        await _db.SaveChangesAsync();

        // Ensure tasks exist for the newly saved configuration
        var taskService = _services.GetRequiredService<IActiveTaskService>();
        await taskService.SyncActiveTasksAsync();
    }

    public async Task<bool> HasConfigurationAsync()
    {
        var user = await EnsureDefaultUserAsync(_db);
        var has = await _db.Houses.Include(h => h.SelectedItemTypes)
            .AnyAsync(h => h.OwnerId == user.Id && h.SelectedItemTypes.Any());
        return has;
    }

    public async Task SetSelectedItemTypesAsync(IEnumerable<string> itemTypes)
    {
        var config = new HouseConfiguration();
        foreach (var t in itemTypes)
        {
            config.SelectedItemTypes.Add(t);
        }
        await SaveConfigurationAsync(config);
    }

    internal static async Task<User> EnsureDefaultUserAsync(YanitorDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Name == "Anders");
        if (user == null)
        {
            user = new User { Name = "Anders" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        return user;
    }
}
