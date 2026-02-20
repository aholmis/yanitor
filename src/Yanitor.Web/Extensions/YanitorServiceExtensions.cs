using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Yanitor.Web.Domain.Services;
using Yanitor.Web.Data;
using Yanitor.Web.Services;
using Yanitor.Web.Services.Notifications;
using Yanitor.Web.BackgroundServices;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Yanitor.Web.Services.Reminders;

namespace Yanitor.Web.Extensions
{
    internal static class YanitorServiceExtensions
    {
        public static void AddYanitorServices(this WebApplicationBuilder builder)
        {
            // UI, Localization and Logging
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
            builder.Services.AddScoped<IOtpService, OtpService>();

            // Register domain services
            builder.Services.AddSingleton<ITaskProvider, TaskProvider>();
            builder.Services.AddScoped<IItemProvider, ItemProvider>();
            // Replace in-memory with EF-backed implementation
            builder.Services.AddScoped<IHouseConfigurationService, EfHouseConfigurationService>();
            builder.Services.AddScoped<IActiveTaskService, ActiveTaskService>();

            // Register notification services
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
            builder.Services.AddScoped<INotificationService, EmailNotificationService>();
            builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();

            // Register reminder calculator
            builder.Services.AddTransient<TaskReminderCalculator>();

            builder.Services.AddHostedService<TaskReminderWorker>();
            builder.Services.AddHostedService<OtpCleanupWorker>();
        }
    }
}
