using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Yanitor.Web.Data;

namespace Yanitor.Web.Services;

public class AuthenticationService(
    YanitorDbContext db,
    IUserContext userContext,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public async Task<SignInResult> SignInWithEmailAsync(string email, bool setEmailVerified = false)
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
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var now = DateTime.UtcNow;

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    CreatedAt = now,
                    LastLoginAt = now,
                    EmailVerified = setEmailVerified
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                logger.LogInformation("New user created with ID {UserId}", user.Id);
            }
            else
            {
                // Update last login time and optionally set email verified
                user.LastLoginAt = now;
                if (setEmailVerified && !user.EmailVerified)
                {
                    user.EmailVerified = true;
                    logger.LogInformation("Email verified for user {UserId}", user.Id);
                }
                await db.SaveChangesAsync();

                logger.LogInformation("User {UserId} signed in", user.Id);
            }

            // Set user context
            await userContext.SetCurrentUserAsync(user.Id, user.Email);

            return new SignInResult(true, null, user);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error during sign-in");
            return new SignInResult(false, "An error occurred. Please try again.", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-in");
            return new SignInResult(false, "An error occurred. Please try again.", null);
        }
    }

    public async Task SignOutAsync()
    {
        var userId = await userContext.GetCurrentUserIdAsync();
        if (userId.HasValue)
        {
            logger.LogInformation("User {UserId} signed out", userId.Value);
        }

        await userContext.ClearCurrentUserAsync();
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
