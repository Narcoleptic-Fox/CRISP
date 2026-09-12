using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Respawn;
using Testcontainers.MySql;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// One MySQL 8.4 for the whole integration collection: a Testcontainer by default, or whatever
/// <c>CRISP_TEST_MYSQL_CONNECTION_STRING</c> names on CI. The schema comes from the real
/// migrations; tests reset state between runs with Respawn rather than recreating the database.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string CiConnectionStringVariable = "CRISP_TEST_MYSQL_CONNECTION_STRING";
    private const string TestDatabase = "crisp_tests";

    private MySqlContainer? _container;
    private Respawner? _respawner;

    public CrispServerApplicationFactory Factory { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        using var setupTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        var connectionString = await GetConnectionStringAsync(setupTimeout.Token);
        EnsureTestDatabase(connectionString);
        await WaitUntilReachableAsync(connectionString, setupTimeout.Token);

        Factory = new CrispServerApplicationFactory(connectionString);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));

        // The migrations are the schema. EnsureCreated would hide a migration that does not match
        // the model, which is exactly the kind of drift an integration suite exists to catch.
        await db.Database.MigrateAsync(setupTimeout.Token);

        await using var dbConnection = db.Database.GetDbConnection();
        await dbConnection.OpenAsync(setupTimeout.Token);
        _respawner = await Respawner.CreateAsync(
            dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.MySql,
                TablesToIgnore = ["__EFMigrationsHistory"],
            }
        );
    }

    /// <summary>Wipes every table except the migration history.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    /// <summary>Seeds an active application user and returns the email the test header should carry.</summary>
    public async Task<string> SeedUserAsync(Roles role, string? email = null)
    {
        email ??= $"{role.ToString().ToLowerInvariant()}@crisp.test";
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Set<UserEntity>()
            .Add(
                new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Username = email.Split('@')[0],
                    Email = email,
                    Role = role,
                    IsActive = true,
                }
            );
        await db.SaveChangesAsync();
        return email;
    }

    /// <summary>A client whose requests authenticate as the given seeded user, or anonymously when null.</summary>
    public HttpClient CreateClient(string? asUser)
    {
        var client = Factory.CreateClient();
        if (asUser is not null)
            client.DefaultRequestHeaders.Add(TestUserAuthHandler.UserHeader, asUser);
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        Factory?.Dispose();
        if (_container is not null)
            await _container.DisposeAsync();

        MySqlConnection.ClearAllPools();
    }

    private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken)
    {
        var ciConnectionString = Environment.GetEnvironmentVariable(CiConnectionStringVariable);
        if (!string.IsNullOrWhiteSpace(ciConnectionString))
            return WithTimeouts(ciConnectionString);

        _container = new MySqlBuilder("mysql:8.4")
            .WithDatabase(TestDatabase)
            .WithUsername("crisp")
            .WithPassword("crisp_password")
            .Build();

        await _container.StartAsync(cancellationToken);
        return WithTimeouts(_container.GetConnectionString());
    }

    private static string WithTimeouts(string connectionString) =>
        new MySqlConnectionStringBuilder(connectionString)
        {
            ConnectionTimeout = 15,
            DefaultCommandTimeout = 30,
        }.ConnectionString;

    // Respawn truncates every table it finds. Refuse to point that at anything but the test database.
    private static void EnsureTestDatabase(string connectionString)
    {
        var database = new MySqlConnectionStringBuilder(connectionString).Database;
        if (!string.Equals(database, TestDatabase, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Integration tests must use the test database '{TestDatabase}', not '{database}'."
            );
    }

    private static async Task WaitUntilReachableAsync(
        string connectionString,
        CancellationToken cancellationToken
    )
    {
        const int attempts = 12;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch (MySqlException) when (attempt < attempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
