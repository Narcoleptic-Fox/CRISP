using CRISP.Server.Authentication;
using CRISP.Server.Data;
using CRISP.Server.Data.Interceptors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// Hosts the real server in-process against the test database, with the seeded-user authentication
/// scheme added in front of the production ones.
/// </summary>
public sealed class CrispServerApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:default"] = connectionString,
                        ["Email:Provider"] = "None",
                        ["ObjectStorage:Provider"] = "None",
                    }
                );
            }
        );

        builder.ConfigureTestServices(services =>
        {
            AddTestUserAuthentication(services);

            // AddDatabaseInfrastructure probed the server version at registration against whatever
            // connection string the configuration had then; re-register against the test database.
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(
                (provider, options) =>
                {
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    options.AddInterceptors(
                        provider.GetRequiredService<SoftDeleteInterceptor>(),
                        provider.GetRequiredService<AuditLogInterceptor>()
                    );
                }
            );
        });
    }

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
}
