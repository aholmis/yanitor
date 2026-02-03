using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System.Security.Claims;
using Yanitor.Web.Models;
using YanitorAuth = Yanitor.Web.Services.IAuthenticationService;

namespace Yanitor.Web.Extensions
{
    internal static class YanitorEndpointExtensions
    {
        public static void MapAuthenticationEndpoints(this WebApplication app)
        {
            // Sign-in endpoint using ASP.NET Core Authentication (form POST)
            app.MapPost("/auth/sign-in", async (HttpContext httpContext, YanitorAuth authService) =>
            {
                var form = await httpContext.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var culture = form["culture"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return Results.Redirect($"{(string.IsNullOrWhiteSpace(culture) ? "" : $"/{culture}")}/sign-in?error=email-required");
                }

                var result = await authService.SignInWithEmailAsync(email);

                if (!result.Success)
                {
                    return Results.Redirect($"{(string.IsNullOrWhiteSpace(culture) ? "" : $"/{culture}")}/sign-in?error=signin-failed");
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

            // Keep the JSON API endpoint for programmatic access
            app.MapPost("/api/auth/sign-in", async (SignInRequest request, YanitorAuth authService, HttpContext httpContext) =>
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Results.BadRequest(new { error = "Email is required" });
                }

                var result = await authService.SignInWithEmailAsync(request.Email);

                if (!result.Success)
                {
                    return Results.BadRequest(new { error = result.ErrorMessage ?? "Sign-in failed" });
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

                var redirectUrl = string.IsNullOrWhiteSpace(request.Culture)
                    ? "/my-house"
                    : $"/{request.Culture}/my-house";

                return Results.Ok(new { redirectUrl });
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
