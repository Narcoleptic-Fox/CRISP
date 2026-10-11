using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data;

/// <summary>
/// Everything that differs between relational providers, for SQL Server. The other providers define a
/// class of the same name and shape in their own folder; exactly one is compiled into a solution.
/// </summary>

public static class DatabaseProvider
{
    public const string Name = "SQL Server";

    /// <summary>The connection string the parts assemble to, in this provider's dialect.</summary>
    /// <remarks>
    /// <c>TrustServerCertificate</c> because a container or an internal instance rarely carries a
    /// certificate the client trusts; replace with a real certificate chain for an internet-facing
    /// server.
    /// </remarks>
    public static string BuildConnectionString(string host, uint port, string database, string user, string password) =>
        $"Server={host},{port};Database={database};User Id={user};Password={password};TrustServerCertificate=true;";

    /// <summary>Called once at registration; the returned delegate configures every context.</summary>
    public static Action<DbContextOptionsBuilder> CreateConfigurator(string connectionString) =>
        options => options.UseSqlServer(connectionString);

    /// <summary>Design-time configuration for the EF tools; no database needs to be running.</summary>
    public static void ConfigureDesignTime(DbContextOptionsBuilder options) =>
        options.UseSqlServer("Server=127.0.0.1,1433;Database=crisp;User Id=sa;Password=Password1!;TrustServerCertificate=true;");

    /// <summary>The column for an object-storage key, which sits in a unique index.</summary>
    /// <remarks>
    /// SQL Server caps an index key at 1700 bytes; 1024 characters of nvarchar would be 2048.
    /// Keys are ASCII by construction (<c>ObjectStorageKey.Normalize</c>), so varchar loses nothing.
    /// </remarks>
    public static PropertyBuilder<string> ObjectKeyColumn(PropertyBuilder<string> property) =>
        property.HasMaxLength(1024).IsUnicode(false);

    /// <summary>Model-wide conventions this provider needs. None here.</summary>
    public static void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}
