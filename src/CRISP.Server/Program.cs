using CRISP.Server.Components;
using CRISP.Server.Components.Account;
using CRISP.Server.Data;
using CRISP.ServiceDefaults.Features;
using CRISP.ServiceDefaults.Middlwares;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace CRISP.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });

            builder.AddNpgsqlDbContext<ApplicationDbContext>("postgres");
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddOutputCache(options =>
            {
                options.UseCaseSensitivePaths = true;
                options.DefaultExpirationTimeSpan = 1.Minutes();
                options.AddBasePolicy(builder => builder.Cache());
            });

            builder.Services.AddExceptionHandler<ExceptionHandler>();

            Assembly[] assembliesToRegisterFrom =
            [
                typeof(CreateCommand).Assembly,    // Core Layer
                typeof(Program).Assembly,          // Application Layer
            ];
            builder.Services.AddValidatorsFromAssemblies(assembliesToRegisterFrom);

            builder.Services.AddFeatures<Program>();
            builder.Services.AddSingleton<IRenderContext, ServerRenderContext>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<LoggingMiddleware>();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapAdditionalIdentityEndpoints();
            app.MapFeatures();

            app.UseOutputCache();

            app.Run();
        }
    }
}
