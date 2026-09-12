using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRISP.Server.Data;

/// <summary>
/// Design-time factory used by EF Core tooling (for example <c>dotnet ef migrations add</c>).
/// </summary>
/// <remarks>
/// Uses a fixed <see cref="MySqlServerVersion"/> rather than <c>ServerVersion.AutoDetect</c> so
/// migrations can be created and scripted without a running database. At runtime the application
/// still configures the context from configuration in <c>DataCollectionExtensions</c>; this factory
/// is only invoked by the EF tools.
/// </remarks>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(
                "server=127.0.0.1;port=3306;database=crisp;uid=root;pwd=password;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new ApplicationDbContext(options);
    }
}
