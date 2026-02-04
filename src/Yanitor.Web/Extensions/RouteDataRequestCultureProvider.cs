using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Yanitor.Web.Extensions
{
    // Custom culture provider that reads culture from route data
    internal class RouteDataRequestCultureProvider : RequestCultureProvider
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
}
