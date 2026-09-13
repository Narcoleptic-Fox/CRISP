using System.Data.Common;
using CRISP.Server.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// The integration tests' database, for PostgreSQL. The other providers define a class of the same name
/// and shape in their own folder; exactly one is compiled into a solution.
/// </summary>
/// <remarks>
/// Starts a Testcontainer unless <c>CRISP_TEST_POSTGRES_CONNECTION_STRING</c> names a server (CI does
/// that with a service container). Refuses any database not named <c>crisp_tests</c>, because the
/// reset truncates every table it finds.
/// </remarks>

internal sealed class TestDatabase : IAsyncDisposable
{
    private const string CiConnectionStringVariable = "CRISP_TEST_POSTGRES_CONNECTION_STRING";
    private const string TestDatabaseName = "crisp_tests";

    private readonly PostgreSqlContainer? _container;
    private Respawner? _respawner;

    private TestDatabase(string connectionString, PostgreSqlContainer? container)
    {
        ConnectionString = connectionString;
        _container = container;
    }

    public string ConnectionString { get; }

    public static async Task<TestDatabase> StartAsync(CancellationToken cancellationToken)
    {
        var ci = Environment.GetEnvironmentVariable(CiConnectionStringVariable);
        if (!string.IsNullOrWhiteSpace(ci))
            return new TestDatabase(EnsureTestDatabase(WithTimeouts(ci)), container: null);

        var container = new PostgreSqlBuilder("postgres:16")
            .WithDatabase(TestDatabaseName)
            .WithUsername("crisp")
            .WithPassword("crisp_password")
            .Build();
        await container.StartAsync(cancellationToken);
        return new TestDatabase(EnsureTestDatabase(WithTimeouts(container.GetConnectionString())), container);
    }

    public async Task WaitUntilReachableAsync(CancellationToken cancellationToken)
    {
        const int attempts = 12;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch (NpgsqlException) when (attempt < attempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    /// <summary>Prepares the reset strategy once the schema exists.</summary>
    public async Task PrepareResetAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"],
        });
    }

    /// <summary>Wipes every table except the migration history.</summary>
    public async Task ResetAsync(ApplicationDbContext db)
    {
        await using DbConnection connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
    }

    private static string WithTimeouts(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString) { Timeout = 15, CommandTimeout = 30 }.ConnectionString;

    private static string EnsureTestDatabase(string connectionString)
    {
        var database = new NpgsqlConnectionStringBuilder(connectionString).Database;
        if (!string.Equals(database, TestDatabaseName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Integration tests must use the test database '{TestDatabaseName}', not '{database}'.");
        return connectionString;
    }
}
