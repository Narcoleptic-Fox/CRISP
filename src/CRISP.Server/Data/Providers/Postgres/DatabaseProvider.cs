using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data;

/// <summary>
/// Everything that differs between relational providers, for PostgreSQL. The other providers define a
/// class of the same name and shape in their own folder; exactly one is compiled into a solution.
/// </summary>

public static class DatabaseProvider
{
    public const string Name = "PostgreSQL";

    /// <summary>The connection string the parts assemble to, in this provider's dialect.</summary>
    public static string BuildConnectionString(string host, uint port, string database, string user, string password) =>
        $"Host={host};Port={port};Database={database};Username={user};Password={password};";

    /// <summary>Called once at registration; the returned delegate configures every context.</summary>
    public static Action<DbContextOptionsBuilder> CreateConfigurator(string connectionString) =>
        options => options.UseNpgsql(connectionString);

    /// <summary>Design-time configuration for the EF tools; no database needs to be running.</summary>
    public static void ConfigureDesignTime(DbContextOptionsBuilder options) =>
        options.UseNpgsql("Host=127.0.0.1;Port=5432;Database=crisp;Username=postgres;Password=password;");

    /// <summary>The column for an object-storage key, which sits in a unique index.</summary>
    public static PropertyBuilder<string> ObjectKeyColumn(PropertyBuilder<string> property) =>
        property.HasMaxLength(1024);

    /// <summary>Model-wide conventions this provider needs. None here.</summary>
    public static void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}
