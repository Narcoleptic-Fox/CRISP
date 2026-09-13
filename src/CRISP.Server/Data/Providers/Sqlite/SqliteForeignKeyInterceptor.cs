using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CRISP.Server.Data;

/// <summary>
/// Enforces SQLite foreign key constraints on every physical connection the application opens.
/// </summary>
/// <remarks>
/// SQLite foreign key enforcement is a per-connection session setting, not something persisted in
/// the database file, and whether it defaults to on depends on how the native SQLite library was
/// built. Relying on that compiled-in default is fragile, so this interceptor explicitly issues
/// <c>PRAGMA foreign_keys = ON;</c> every time a connection is opened -- including connections
/// reused from Microsoft.Data.Sqlite's native handle pool -- so the schema's foreign keys are never
/// silently unenforced.
/// </remarks>
public sealed class SqliteForeignKeyInterceptor : DbConnectionInterceptor
{
    /// <inheritdoc />
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        EnableForeignKeys(connection);
        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc />
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await EnableForeignKeysAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void EnableForeignKeys(DbConnection connection)
    {
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
    }

    private static async Task EnableForeignKeysAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
