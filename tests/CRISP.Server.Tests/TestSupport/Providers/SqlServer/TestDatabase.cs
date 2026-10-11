using System.Data.Common;
using CRISP.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// The integration tests' database, for SQL Server. The other providers define a class of the same name
/// and shape in their own folder; exactly one is compiled into a solution.
/// </summary>
/// <remarks>
/// Starts a Testcontainer unless <c>CRISP_TEST_SQLSERVER_CONNECTION_STRING</c> names a server (CI does
/// that with a service container). Refuses any database not named <c>crisp_tests</c>, because the
/// reset truncates every table it finds.
/// </remarks>

internal sealed class TestDatabase : IAsyncDisposable
{
    private const string CiConnectionStringVariable = "CRISP_TEST_SQLSERVER_CONNECTION_STRING";
    private const string TestDatabaseName = "crisp_tests";

    private readonly MsSqlContainer? _container;
    private Respawner? _respawner;

    private TestDatabase(string connectionString, MsSqlContainer? container)
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

        // The container's connection string points at master; the migrations create crisp_tests
        // on first run because EF's SQL Server provider creates a missing database on Migrate.
        var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync(cancellationToken);
        var connectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = TestDatabaseName,
        }.ConnectionString;
        return new TestDatabase(EnsureTestDatabase(WithTimeouts(connectionString)), container);
    }

    public async Task WaitUntilReachableAsync(CancellationToken cancellationToken)
    {
        // Reach the server (master), not the test database, which may not exist yet.
        var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" }.ConnectionString;
        const int attempts = 12;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(master);
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch (SqlException) when (attempt < attempts)
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
            DbAdapter = DbAdapter.SqlServer,
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
        SqlConnection.ClearAllPools();
    }

    private static string WithTimeouts(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 15, CommandTimeout = 30, TrustServerCertificate = true }.ConnectionString;

    private static string EnsureTestDatabase(string connectionString)
    {
        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        if (!string.Equals(database, TestDatabaseName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Integration tests must use the test database '{TestDatabaseName}', not '{database}'.");
        return connectionString;
    }
}
