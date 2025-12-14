using CRISP.ServiceDefaults.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CRISP.ServiceDefaults.Data.Interceptors;
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _currentUser;
    public SoftDeleteInterceptor(IHttpContextAccessor httpContextAccessors)
    {
        _httpContextAccessor = httpContextAccessors;
        _currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

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
