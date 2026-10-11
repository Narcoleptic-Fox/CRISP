using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data;

/// <summary>
/// Everything that differs between relational providers, for MySQL. The other providers define a
/// class of the same name and shape in their own folder; exactly one is compiled into a solution.
/// </summary>

public static class DatabaseProvider
{
    public const string Name = "MySQL";

    /// <summary>The connection string the parts assemble to, in this provider's dialect.</summary>
    public static string BuildConnectionString(string host, uint port, string database, string user, string password) =>
        $"server={host};port={port};database={database};uid={user};pwd={password};";

    /// <summary>
    /// Called once at registration; the returned delegate configures every context.
    /// </summary>
    /// <remarks>
    /// The server version is detected here, once, and reused. Calling <c>ServerVersion.AutoDetect</c>
    /// inside the per-context lambda opened a fresh probe connection on every context creation, i.e.
    /// every request; under restart churn or load those synchronous probes exhausted the pool and made
    /// every database-backed request hang.
    /// </remarks>
    public static Action<DbContextOptionsBuilder> CreateConfigurator(string connectionString)
    {
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        return options => options.UseMySql(connectionString, serverVersion);
    }

    /// <summary>
    /// Design-time configuration for the EF tools. A fixed server version so migrations can be created
    /// and scripted without a running database.
    /// </summary>
    public static void ConfigureDesignTime(DbContextOptionsBuilder options) =>
        options.UseMySql(
            "server=127.0.0.1;port=3306;database=crisp;uid=root;pwd=password;",
            new MySqlServerVersion(new Version(8, 4, 0)));

    /// <summary>
    /// The column for an object-storage key, which sits in a unique index.
    /// </summary>
    /// <remarks>
    /// MySQL limits an index key to 3072 bytes and utf8mb4 is four bytes per character, so a
    /// 1024-character key must be stored in a single-byte charset. Keys are ASCII by construction
    /// (<c>ObjectStorageKey.Normalize</c>), so latin1 loses nothing.
    /// </remarks>
    public static PropertyBuilder<string> ObjectKeyColumn(PropertyBuilder<string> property) =>
        property.HasMaxLength(1024).HasColumnType("varchar(1024)").HasCharSet("latin1");

    /// <summary>Model-wide conventions this provider needs. None here.</summary>
    public static void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}
