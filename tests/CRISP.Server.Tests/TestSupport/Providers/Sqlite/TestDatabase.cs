using CRISP.Server.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// The integration tests' database, for SQLite. The other providers define a class of the same
/// name and shape in their own folder; exactly one is compiled into a solution.
/// </summary>
/// <remarks>
/// No container: a file under the temp folder, deleted on dispose. Respawn has no SQLite adapter,
/// so a reset drops the file and re-applies the migrations, which for a schema this size is fast.
/// </remarks>
internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly string _path;

    private TestDatabase(string path)
    {
        _path = path;
        ConnectionString = $"Data Source={path}";
    }

    public string ConnectionString { get; }

    public static Task<TestDatabase> StartAsync(CancellationToken cancellationToken)
    {
        var directory = Path.Combine(Path.GetTempPath(), "crisp_tests");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"crisp_tests-{Guid.NewGuid():N}.db");
        return Task.FromResult(new TestDatabase(path));
    }

    public Task WaitUntilReachableAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Nothing to prepare: the reset re-creates the file.</summary>
    public Task PrepareResetAsync(ApplicationDbContext db, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Drops the database file and re-applies the migrations.</summary>
    public async Task ResetAsync(ApplicationDbContext db)
    {
        SqliteConnection.ClearAllPools();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path))
            File.Delete(_path);
        return ValueTask.CompletedTask;
    }
}
