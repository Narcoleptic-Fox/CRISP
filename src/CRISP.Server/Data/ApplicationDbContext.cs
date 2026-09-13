using Microsoft.EntityFrameworkCore;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data;

/// <summary>
/// EF Core database context for the CRISP server.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Gets the API keys persisted for service-to-service authentication.
    /// </summary>
    public DbSet<ApiKeyEntity> ApiKeys => Set<ApiKeyEntity>();

    /// <summary>
    /// Applies entity configuration classes from the server assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder used by EF Core.</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        DatabaseProvider.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
