using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Yanitor.Web.Services;

namespace Yanitor.Web.Domain.Services;

public class EfHouseConfigurationService
    : IHouseConfigurationService
{
    private readonly YanitorDbContext _db;
    private readonly IServiceProvider _services;
    private readonly IUserContext _userContext;

    public EfHouseConfigurationService(YanitorDbContext db, IServiceProvider services, IUserContext userContext)
    {
        _db = db;
        _services = services;
        _userContext = userContext;
    }

    public async Task<HouseConfiguration?> GetConfigurationAsync()
    {
        var house = await GetCurrentUserHouseAsync();
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
        var house = await GetCurrentUserHouseAsync();

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
        var userId = await _userContext.GetCurrentUserIdAsync();
        if (userId == null) return false;
        
        var has = await _db.Houses.Include(h => h.SelectedItemTypes)
            .AnyAsync(h => h.OwnerId == userId.Value && h.SelectedItemTypes.Any());
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

    private async Task<House> GetCurrentUserHouseAsync()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        if (userId == null)
            throw new InvalidOperationException("No authenticated user");

        // Ensure a corresponding User row exists before creating a House.
        // If a House is created with an OwnerId that doesn't exist in Users, the FK constraint will fail.
        
        var house = await _db.Houses
            .Include(h => h.SelectedItemTypes)
            .FirstOrDefaultAsync(h => h.OwnerId == userId.Value);

        if (house == null)
        {
            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
            {
                // Try to obtain an email from the user context if available.
                var email = await _userContext.GetCurrentUserEmailAsync() ?? string.Empty;
                user = new User { Id = userId.Value, Email = email, CreatedAt = DateTime.UtcNow };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            house = new House { OwnerId = userId.Value };
            _db.Houses.Add(house);
            await _db.SaveChangesAsync();
            _db.Entry(house).Collection(h => h.SelectedItemTypes).Load();
        }

        return house;
    }
}
