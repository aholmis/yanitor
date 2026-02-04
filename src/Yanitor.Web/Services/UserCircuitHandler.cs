using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Security.Claims;

namespace Yanitor.Web.Services;

public class UserCircuitHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserCircuitHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId { get; set; }
    public string? Email { get; set; }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken ct)
    {
        // Load user from authenticated claims
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                UserId = userId;
            }

            var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                Email = emailClaim.Value;
            }
        }

        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken ct)
    {
        UserId = null;
        Email = null;
        return Task.CompletedTask;
    }
}
