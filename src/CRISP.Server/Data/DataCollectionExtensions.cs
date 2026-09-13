using CRISP.Server.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Data;

public static class DataCollectionExtensions
{
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ConnectionStringExpansion.Expand(
            configuration.GetConnectionString("default")
                ?? throw new InvalidOperationException("Connection string 'default' not found."));

        // Provider-specific work that should happen once (MySQL probes the server version here)
        // happens now, not per context.
        var configure = DatabaseProvider.CreateConfigurator(connectionString);

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            configure(options);
            options.AddInterceptors(
                provider.GetRequiredService<SoftDeleteInterceptor>(),
                provider.GetRequiredService<AuditLogInterceptor>());
        });
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        return services;
    }
}
