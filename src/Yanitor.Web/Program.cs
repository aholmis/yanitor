using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Yanitor.Web.Components;
using Yanitor.Web.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Yanitor.Web.Data;
using Yanitor.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// Add Authentication with Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Yanitor.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.SameAsRequest 
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/sign-in";
        options.LogoutPath = "/api/auth/sign-out";
        options.AccessDeniedPath = "/sign-in";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // For Blazor, we want to handle redirects differently
                // Don't redirect API calls, just return 401
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// HttpClient for server-side API calls
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// EF Core with SQLite
builder.Services.AddDbContext<YanitorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Yanitor") ?? "Data Source=yanitor.db"));

// Register circuit handler for user state management
builder.Services.AddScoped<UserCircuitHandler>();
builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<UserCircuitHandler>());

// Register authentication and user context services
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<Yanitor.Web.Services.IAuthenticationService, Yanitor.Web.Services.AuthenticationService>();

// Register domain services
builder.Services.AddSingleton<ITaskProvider, TaskProvider>();
builder.Services.AddScoped<IItemProvider, ItemProvider>();
// Replace in-memory with EF-backed implementation
builder.Services.AddScoped<IHouseConfigurationService, EfHouseConfigurationService>();
builder.Services.AddScoped<IActiveTaskService, ActiveTaskService>();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("nb-NO") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true
};

// Custom RouteDataRequestCultureProvider to read culture from route values
localizationOptions.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());

// Query string overrides cookie; accept-language fallback.
localizationOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());

var app = builder.Build();

// Apply EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<YanitorDbContext>();
    await db.Database.MigrateAsync();
}

app.UseRequestLocalization(localizationOptions);

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// IMPORTANT: Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Sign-in endpoint using ASP.NET Core Authentication (form POST)
app.MapPost("/auth/sign-in", async (HttpContext httpContext, Yanitor.Web.Services.IAuthenticationService authService) =>
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
app.MapPost("/api/auth/sign-in", async (SignInRequest request, Yanitor.Web.Services.IAuthenticationService authService, HttpContext httpContext) =>
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
    return Results.Ok();
});

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

await app.RunAsync();

// Custom culture provider that reads from route data
public class RouteDataRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var routeValues = httpContext.Request.RouteValues;

        if (routeValues.TryGetValue("culture", out var cultureValue) && cultureValue is string culture)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
            }
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }
}

public record SignInRequest(string Email, string? Culture);
