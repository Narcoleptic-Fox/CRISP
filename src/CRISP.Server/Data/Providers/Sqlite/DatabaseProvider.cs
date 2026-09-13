using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRISP.Server.Data;

/// <summary>
/// Everything that differs between relational providers, for SQLite. The other providers define a
/// class of the same name and shape in their own folder; exactly one is compiled into a solution.
/// </summary>

/// <remarks>
/// SQLite is a file, not a server: <c>host</c>, <c>port</c>, <c>user</c> and <c>password</c> are
/// ignored and <c>database</c> is the file path. Foreign keys are off by default per connection and
/// are switched on by <see cref="SqliteForeignKeyInterceptor"/>.
/// </remarks>
public static class DatabaseProvider
{
    public const string Name = "SQLite";

    /// <summary>The connection string the parts assemble to, in this provider's dialect.</summary>
    public static string BuildConnectionString(string host, uint port, string database, string user, string password) =>
        $"Data Source={database}";

    /// <summary>Called once at registration; the returned delegate configures every context.</summary>
    public static Action<DbContextOptionsBuilder> CreateConfigurator(string connectionString) =>
        options => options.UseSqlite(connectionString).AddInterceptors(new SqliteForeignKeyInterceptor());

    /// <summary>Design-time configuration for the EF tools; no database needs to exist.</summary>
    public static void ConfigureDesignTime(DbContextOptionsBuilder options) =>
        options.UseSqlite("Data Source=crisp.db");

    /// <summary>The column for an object-storage key, which sits in a unique index.</summary>
    public static PropertyBuilder<string> ObjectKeyColumn(PropertyBuilder<string> property) =>
        property.HasMaxLength(1024);

    /// <summary>
    /// Model-wide conventions this provider needs.
    /// </summary>
    /// <remarks>
    /// SQLite stores <see cref="DateTimeOffset"/> as ISO-8601 text and EF Core refuses to translate
    /// an ORDER BY over it ("SQLite does not support expressions of type 'DateTimeOffset' in ORDER
    /// BY clauses"), which the activity feed does. Storing UTC ticks as an INTEGER makes ordering
    /// native. The offset is not kept: every timestamp this application writes is UTC.
    /// </remarks>
    public static void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<UtcTicksDateTimeOffsetConverter>();
    }
}

/// <summary>Stores a <see cref="DateTimeOffset"/> as UTC ticks so SQLite can order by it.</summary>
public sealed class UtcTicksDateTimeOffsetConverter()
    : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, long>(
        value => value.UtcTicks,
        ticks => new DateTimeOffset(ticks, TimeSpan.Zero));
