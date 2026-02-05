using CRISP.Server.Components;
//#if (IncludeAuth)
using CRISP.Server.Components.Account;
//#endif
using CRISP.Server.Data;
using CRISP.ServiceDefaults.Features;
using CRISP.ServiceDefaults.Middlwares;
using FluentValidation;
using Humanizer;
//#if (IncludeAuth)
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
//#endif
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

//#if (IncludeAuth)
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });
//#endif

//#if (UsePostgres)
            builder.AddNpgsqlDbContext<ApplicationDbContext>("postgres");
//#endif
//#if (UseSqlServer)
            builder.AddSqlServerDbContext<ApplicationDbContext>("sqlserver");
//#endif
//#if (UseSqlite)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//#endif
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//#if (IncludeAuth)
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
//#endif

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
//#if (IncludeAuth)
            app.UseAuthentication();
            app.UseAuthorization();
//#endif

            app.UseMiddleware<LoggingMiddleware>();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

//#if (IncludeAuth)
            app.MapAdditionalIdentityEndpoints();
//#endif
            app.MapFeatures();

            app.UseOutputCache();

            app.Run();
        }
    }
}
