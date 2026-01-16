namespace Yanitor.Web.Services;

public interface IUserContext
{
    Task<Guid?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserEmailAsync();
    Task<Yanitor.Web.Data.User?> GetCurrentUserAsync();
    Task SetCurrentUserAsync(Guid userId, string email);
    Task ClearCurrentUserAsync();
}
