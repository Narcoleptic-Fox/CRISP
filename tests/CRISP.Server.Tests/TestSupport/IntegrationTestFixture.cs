using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// One database for the whole integration collection, provided by <see cref="TestDatabase"/> for
/// the configured provider. The schema comes from the real migrations; tests reset state between
/// runs rather than recreating the host.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private TestDatabase _database = default!;

    public CrispServerApplicationFactory Factory { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        using var setupTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        _database = await TestDatabase.StartAsync(setupTimeout.Token);
        await _database.WaitUntilReachableAsync(setupTimeout.Token);

        Factory = new CrispServerApplicationFactory(_database.ConnectionString);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));

        // The migrations are the schema. EnsureCreated would hide a migration that does not match
        // the model, which is exactly the kind of drift an integration suite exists to catch. A
        // solution generated without the sample (or for a provider other than PostgreSQL) ships no
        // migration yet; say so rather than silently creating a schema no deploy will ever see.
        if (!db.Database.GetMigrations().Any())
        {
            throw new InvalidOperationException(
                "No EF Core migrations found. Run `dotnet ef migrations add InitialCreate --project src/CRISP.Server` "
                + "(the design-time factory needs no database) and build again before running the integration tests."
            );
        }

        await db.Database.MigrateAsync(setupTimeout.Token);
        await _database.PrepareResetAsync(db, setupTimeout.Token);
    }

    /// <summary>Wipes every table except the migration history.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await _database.ResetAsync(db);
    }

    /// <summary>Seeds an active application user and returns the email the test header should carry.</summary>
    public async Task<string> SeedUserAsync(Roles role, string? email = null)
    {
        email ??= $"{role.ToString().ToLowerInvariant()}@crisp.test";
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Set<UserEntity>().Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = email.Split('@')[0],
            Email = email,
            Role = role,
            IsActive = true,
        });
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
        if (_database is not null)
            await _database.DisposeAsync();
    }
}
