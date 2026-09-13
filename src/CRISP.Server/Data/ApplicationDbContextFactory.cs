using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRISP.Server.Data;

/// <summary>
/// Design-time factory used by EF Core tooling (for example <c>dotnet ef migrations add</c>).
/// </summary>
/// <remarks>
/// Uses the provider's fixed design-time configuration rather than the application's, so migrations
/// can be created and scripted without a running database. At runtime the application configures
/// the context from configuration in <c>DataCollectionExtensions</c>; this factory is only invoked
/// by the EF tools.
/// </remarks>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        DatabaseProvider.ConfigureDesignTime(builder);
        return new ApplicationDbContext(builder.Options);
    }
}
