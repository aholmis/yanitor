using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Yanitor.Web.Data;

namespace Yanitor.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly YanitorDbContext _db;
    private readonly IUserContext _userContext;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        YanitorDbContext db,
        IUserContext userContext,
        ILogger<AuthenticationService> logger)
    {
        _db = db;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<SignInResult> SignInWithEmailAsync(string email)
    {
        try
        {
            // Validate and normalize email
            if (string.IsNullOrWhiteSpace(email))
            {
                return new SignInResult(false, "Email is required", null);
            }

            email = email.Trim().ToLowerInvariant();

            // Validate email format
            if (!IsValidEmail(email))
            {
                return new SignInResult(false, "Please enter a valid email address", null);
            }

            // Check if user exists
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var now = DateTime.UtcNow;

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    CreatedAt = now,
                    LastLoginAt = now,
                    EmailVerified = false
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("New user created with ID {UserId}", user.Id);
            }
            else
            {
                // Update last login time
                user.LastLoginAt = now;
                await _db.SaveChangesAsync();

                _logger.LogInformation("User {UserId} signed in", user.Id);
            }

            // Set user context
            await _userContext.SetCurrentUserAsync(user.Id, user.Email);

            return new SignInResult(true, null, user);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during sign-in");
            return new SignInResult(false, "An error occurred. Please try again.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-in");
            return new SignInResult(false, "An error occurred. Please try again.", null);
        }
    }

    public async Task SignOutAsync()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        if (userId.HasValue)
        {
            _logger.LogInformation("User {UserId} signed out", userId.Value);
        }

        await _userContext.ClearCurrentUserAsync();
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return await _userContext.GetCurrentUserAsync();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
