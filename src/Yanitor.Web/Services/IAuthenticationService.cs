using Yanitor.Web.Data;

namespace Yanitor.Web.Services;

public record SignInResult(bool Success, string? ErrorMessage, User? User);

public interface IAuthenticationService
{
    Task<SignInResult> SignInWithEmailAsync(string email, bool setEmailVerified = false);
    Task SignOutAsync();
}
