using Microsoft.EntityFrameworkCore;
using CRISP.Server.Data.Interceptors;

namespace CRISP.Server.Data;

public static class DataCollectionExtensions
{
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("default")
            ?? throw new InvalidOperationException("Connection string 'default' not found.");

        // Detect the server version ONCE at startup and reuse it for every DbContext. Calling
        // ServerVersion.AutoDetect inside the per-context options lambda opened a fresh probe
        // connection on every context creation (i.e. every request); under restart churn or load
        // those synchronous probes exhaust the connection pool and make context creation — and
        // therefore every DB-backed request — hang indefinitely.
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseMySql(connectionString, serverVersion);
            options.AddInterceptors(
                provider.GetRequiredService<SoftDeleteInterceptor>(),
                provider.GetRequiredService<AuditLogInterceptor>());
        });
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        return services;
    }
}
