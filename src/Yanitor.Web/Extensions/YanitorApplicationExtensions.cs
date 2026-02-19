using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Yanitor.Web.Data;

namespace Yanitor.Web.Extensions
{
    internal static class YanitorApplicationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<YanitorDbContext>();
            await db.Database.MigrateAsync();
        }

        public static void UseYanitorRequestLocalization(this WebApplication app)
        {
            var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("no") };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                ApplyCurrentCultureToResponseHeaders = true
            };

            localizationOptions.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
            localizationOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
            localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            localizationOptions.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());

            app.UseRequestLocalization(localizationOptions);
        }
    }
}
