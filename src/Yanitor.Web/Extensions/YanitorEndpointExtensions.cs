using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Yanitor.Web.Models;
using Yanitor.Web.Resources;
using Yanitor.Web.Services;
using Yanitor.Web.Services.Notifications;
using YanitorAuth = Yanitor.Web.Services.IAuthenticationService;

namespace Yanitor.Web.Extensions
{
    internal static class YanitorEndpointExtensions
    {
        public static void MapAuthenticationEndpoints(this WebApplication app)
        {
            // Sign-in endpoint: Request OTP (form POST)
            app.MapPost("/auth/sign-in", async (
                HttpContext httpContext,
                IOtpService otpService,
                IEmailSender emailSender,
                IStringLocalizer<SharedResources> localizer) =>
            {
                var form = await httpContext.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var culture = form["culture"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return Results.Redirect($"{(string.IsNullOrWhiteSpace(culture) ? "" : $"/{culture}")}/sign-in?error=email-required");
                }

                // Check rate limit
                if (!await otpService.CanRequestOtpAsync(email))
                {
                    return Results.Redirect($"{(string.IsNullOrWhiteSpace(culture) ? "" : $"/{culture}")}/sign-in?error=rate-limit");
                }

                // Generate OTP
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var code = await otpService.GenerateOtpAsync(email, ipAddress);

                // Send email with OTP
                var subject = EmailTemplates.GetOtpEmailSubject(localizer);
                var body = EmailTemplates.GetOtpEmailHtml(localizer, code, 15);

                try
                {
                    await emailSender.SendEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                    // Log error but don't expose to user
                    app.Logger.LogError(ex, "Failed to send OTP email to {Email}", email);
                    return Results.Redirect($"{(string.IsNullOrWhiteSpace(culture) ? "" : $"/{culture}")}/sign-in?error=email-failed");
                }

                // Redirect back to sign-in page with success message
                var redirectPath = string.IsNullOrWhiteSpace(culture) ? "/sign-in" : $"/{culture}/sign-in";
                var queryParams = $"?step=verify&email={Uri.EscapeDataString(email)}";
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    queryParams += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
                }
                return Results.Redirect(redirectPath + queryParams);
            })
            .DisableAntiforgery();

            // Verify OTP endpoint (form POST)
            app.MapPost("/auth/verify-otp", async (
                HttpContext httpContext,
                IOtpService otpService,
                YanitorAuth authService) =>
            {
                var form = await httpContext.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var code = form["code"].ToString();
                var culture = form["culture"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                {
                    var redirectPath = string.IsNullOrWhiteSpace(culture) ? "/sign-in" : $"/{culture}/sign-in";
                    return Results.Redirect($"{redirectPath}?step=verify&email={Uri.EscapeDataString(email)}&error=invalid-code");
                }

                // Validate OTP
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var isValid = await otpService.ValidateOtpAsync(email, code, ipAddress);

                if (!isValid)
                {
                    var redirectPath = string.IsNullOrWhiteSpace(culture) ? "/sign-in" : $"/{culture}/sign-in";
                    return Results.Redirect($"{redirectPath}?step=verify&email={Uri.EscapeDataString(email)}&error=invalid-code");
                }

                // OTP is valid - sign in the user
                var result = await authService.SignInWithEmailAsync(email, setEmailVerified: true);

                if (!result.Success)
                {
                    var redirectPath = string.IsNullOrWhiteSpace(culture) ? "/sign-in" : $"/{culture}/sign-in";
                    return Results.Redirect($"{redirectPath}?error=signin-failed");
                }

                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.User!.Id.ToString()),
                    new(ClaimTypes.Email, result.User.Email),
                    new(ClaimTypes.Name, result.User.DisplayName ?? result.User.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in using ASP.NET Core Authentication
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                        AllowRefresh = true
                    });

                var redirectUrl = string.IsNullOrWhiteSpace(returnUrl)
                    ? (string.IsNullOrWhiteSpace(culture) ? "/my-house" : $"/{culture}/my-house")
                    : returnUrl;

                return Results.Redirect(redirectUrl);
            })
            .DisableAntiforgery();

            // Sign-out endpoint using ASP.NET Core Authentication
            app.MapPost("/api/auth/sign-out", async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var path = httpContext.Request.PathBase + httpContext.Request.Path;
                var culture = httpContext.Request.RouteValues.TryGetValue("culture", out var cultureValue)
                    ? cultureValue?.ToString()
                    : null;

                // If culture isn't a route value (this endpoint isn't culture-routed), fall back to referer path parsing
                if (string.IsNullOrWhiteSpace(culture) && httpContext.Request.Headers.TryGetValue("Referer", out var refererValues))
                {
                    if (Uri.TryCreate(refererValues.FirstOrDefault(), UriKind.Absolute, out var refererUri))
                    {
                        var segments = refererUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (segments.Length > 0)
                        {
                            culture = segments[0];
                        }
                    }
                }

                var redirectUrl = string.IsNullOrWhiteSpace(culture) ? "/sign-in" : $"/{culture}/sign-in";
                return Results.Redirect(redirectUrl);
            });
        }

        public static void MapCultureEndpoints(this WebApplication app)
        {
            // Endpoint to persist culture via cookie and redirect
            app.MapGet("/set-culture", (string culture, string? returnUrl, HttpContext ctx) =>
            {
                if (string.IsNullOrWhiteSpace(culture)) return Results.BadRequest("culture required");
                var requestCulture = new RequestCulture(culture);
                ctx.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(requestCulture));
                var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                return Results.Redirect(target);
            });
        }
    }
}
