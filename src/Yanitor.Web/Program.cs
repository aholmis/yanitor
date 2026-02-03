using Yanitor.Web.Components;
using Yanitor.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.AddYanitorServices();

var app = builder.Build();

// Apply EF Core migrations at startup
await app.ApplyMigrationsAsync();

// Request localization
app.UseYanitorRequestLocalization();

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

// IMPORTANT: Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthenticationEndpoints();
app.MapCultureEndpoints();

await app.RunAsync();