using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Data.Interceptors;

/// <summary>
/// Converts EF Core delete operations on soft-delete entities into updates that mark rows as deleted.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _currentUser;
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeleteInterceptor"/> class.
    /// </summary>
    /// <param name="httpContextAccessors">The HTTP context accessor reserved for current-user context.</param>
    public SoftDeleteInterceptor(IHttpContextAccessor httpContextAccessors)
    {
        _httpContextAccessor = httpContextAccessors;
        _currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }

    /// <summary>
    /// Converts synchronous delete operations into soft-delete updates before saving.
    /// </summary>
    /// <param name="eventData">The EF Core save changes event data.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result returned by the base interceptor.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Converts asynchronous delete operations into soft-delete updates before saving.
    /// </summary>
    /// <param name="eventData">The EF Core save changes event data.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>The interception result returned by the base interceptor.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result);
    }

    private void UpdateToSoftDelete(DbContext? context)
    {
        if (context is null)
            return;
        foreach (EntityEntry<ISoftDelete> entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
        }
    }
}
