using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yanitor.Web.Data;

namespace Yanitor.Web.Services;

public class UserContext(YanitorDbContext db, UserCircuitHandler circuitHandler, IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly YanitorDbContext _db = db;
    private readonly UserCircuitHandler _circuitHandler = circuitHandler;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Task<Guid?> GetCurrentUserIdAsync()
    {
        // First check circuit handler state
        if (_circuitHandler.UserId.HasValue)
        {
            return Task.FromResult(_circuitHandler.UserId);
        }

        // Fallback to HttpContext claims if circuit handler hasn't been initialized yet
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                // Also update circuit handler state for consistency
                _circuitHandler.UserId = userId;
                return Task.FromResult<Guid?>(userId);
            }
        }

        return Task.FromResult<Guid?>(null);
    }

    public Task<string?> GetCurrentUserEmailAsync()
    {
        // First check circuit handler state
        if (!string.IsNullOrEmpty(_circuitHandler.Email))
        {
            return Task.FromResult(_circuitHandler.Email);
        }

        // Fallback to HttpContext claims if circuit handler hasn't been initialized yet
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                // Also update circuit handler state for consistency
                _circuitHandler.Email = emailClaim.Value;
                return Task.FromResult<string?>(emailClaim.Value);
            }
        }

        return Task.FromResult<string?>(null);
    }

    public Task SetCurrentUserAsync(Guid userId, string email)
    {
        // Only set circuit state; cookies are managed by API endpoints
        _circuitHandler.UserId = userId;
        _circuitHandler.Email = email;
        return Task.CompletedTask;
    }

    public Task ClearCurrentUserAsync()
    {
        // Only clear circuit state; cookies are managed by API endpoints
        _circuitHandler.UserId = null;
        _circuitHandler.Email = null;
        return Task.CompletedTask;
    }
}
