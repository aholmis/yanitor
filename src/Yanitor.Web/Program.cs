using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Yanitor.Web.Components;
using Yanitor.Web.Domain.Components;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddLogging();

// Register domain services
builder.Services.AddSingleton<IItemProvider, ItemProvider>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
