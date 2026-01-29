using CRISP.Server.Components;
using CRISP.Server.Components.Account;
using CRISP.Server.Data;
using CRISP.ServiceDefaults.Features;
using CRISP.ServiceDefaults.Middlwares;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using MudBlazor.Services;
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
                })
                /*.AddIdentityCookies()*/;

            builder.AddNpgsqlDbContext<ApplicationDbContext>("postgres");
            //string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                //.AddRoles<ApplicationRole>()
                //.AddRoleStore<ApplicationDbContext>()
                //.AddRoleManager<RoleManager<ApplicationRole>>()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddOutputCache(options =>
            {
                options.UseCaseSensitivePaths = true; // Enable case-sensitive paths
                options.DefaultExpirationTimeSpan = 1.Minutes();
                options.AddBasePolicy(builder => builder.Cache());
            });

            builder.Services.AddExceptionHandler<ExceptionHandler>();

            Assembly[] assembliesToRegisterFrom =
            [
                typeof(CreateCommand).Assembly,    // Core Layer
                typeof(Program).Assembly, // Application Layer
            ];
            builder.Services.AddValidatorsFromAssemblies(assembliesToRegisterFrom);

            builder.Services.AddFeatures<Program>();
            builder.Services.AddSingleton<IRenderContext, ServerRenderContext>();


            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 3000;
            });

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();
            app.MapFeatures();

            app.UseOutputCache();

            app.Run();
        }
    }
}
