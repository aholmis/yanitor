namespace Yanitor.Web.Services;

public interface IUserContext
{
    Task<Guid?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserEmailAsync();
    Task SetCurrentUserAsync(Guid userId, string email);
    Task ClearCurrentUserAsync();
}
