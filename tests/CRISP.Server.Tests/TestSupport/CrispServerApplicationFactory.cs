//#if (IncludeAuth)
using CRISP.Server.Authentication;
//#endif
using CRISP.Server.Data;
using CRISP.Server.Data.Interceptors;
//#if (IncludeAuth)
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// Hosts the real server in-process against the test database, with the seeded-user authentication
/// scheme added in front of the production ones.
/// </summary>
public sealed class CrispServerApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configuration the server must see while it registers services, not after.
    /// </summary>
    /// <remarks>
    /// ConfigureAppConfiguration inside ConfigureWebHost is applied after Program has run its
    /// registration, and AddDatabaseInfrastructure reads the connection string there. It worked
    /// by accident in the template repository because a gitignored appsettings.Development.json
    /// supplied one; a fresh checkout has no such file. Host configuration is in place before
    /// WebApplication.CreateBuilder returns, so Program sees these values.
    /// </remarks>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration =>
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:default"] = connectionString,
                    ["Email:Provider"] = "None",
                    ["ObjectStorage:Provider"] = "None",
                    // The suite makes hundreds of requests a minute as a handful of users; the
                    // production limits (50 global, 100 per user) trip mid-run against a fast database.
                    // Rate limiting has its own unit tests; here it must stay out of the way.
                    ["RateLimiting:GlobalPermitLimitPerMinute"] = "1000000",
                    ["RateLimiting:AuthenticatedPermitLimitPerMinute"] = "1000000",
                    ["RateLimiting:AnonymousPermitLimitPerMinute"] = "1000000",
                }
            )
        );
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
//#if (IncludeAuth)
            AddTestUserAuthentication(services);
//#endif

            // Program registered the context against the same test connection string (host
            // configuration above), but through the provider's one-time configurator; re-register so
            // the test host owns the registration outright and the interceptors are the real ones.
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            var configure = DatabaseProvider.CreateConfigurator(connectionString);
            services.AddDbContext<ApplicationDbContext>(
                (provider, options) =>
                {
                    configure(options);
                    options.AddInterceptors(
                        provider.GetRequiredService<SoftDeleteInterceptor>(),
                        provider.GetRequiredService<AuditLogInterceptor>()
                    );
                }
            );
        });
    }

//#if (IncludeAuth)
    /// <summary>
    /// Adds the seeded-user scheme and teaches the composite scheme to select it.
    /// </summary>
    /// <remarks>
    /// Production's selector forwards to API key when its header is present and JWT bearer otherwise.
    /// Post-configuring it here inserts a third branch ahead of those, keeping the production
    /// ordering intact for every request that does not carry the test header.
    /// </remarks>
    private static void AddTestUserAuthentication(IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestUserAuthHandler>(
                TestUserAuthHandler.SchemeName,
                _ => { }
            );

        services.PostConfigure<PolicySchemeOptions>(
            AuthenticationExtensions.CompositeScheme,
            options =>
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(TestUserAuthHandler.UserHeader)
                        ? TestUserAuthHandler.SchemeName
                    : context.Request.Headers.ContainsKey(ApiKeyAuthOptions.HeaderName)
                        ? ApiKeyAuthOptions.DefaultScheme
                    : JwtBearerDefaults.AuthenticationScheme
        );
    }
//#endif
}
