using CRISP.Core.Audit;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features;

public static class GetActivityEndpoint
{
    public static RouteGroupBuilder Map<TEntity>(this RouteGroupBuilder group)
        where TEntity : BaseAuditableEntity
    {
        var entityType = typeof(TEntity).Name.Replace("Entity", "");
        group.MapGet("/{Id}/activity", HandleAsync<TEntity>)
        .WithName($"Get{entityType}Activity")
        .Produces<IEnumerable<SystemMessage>>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    private const int defaultPageSize = 100;
    public static async Task<IResult> HandleAsync<TEntity>(
        Guid id,
        [FromServices] ApplicationDbContext db,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
        where TEntity : BaseAuditableEntity
    {
        var entityType = typeof(TEntity).Name.Replace("Entity", "");
        var entities = from e in db.Set<SystemMessageEntity>().AsNoTracking()
                       where e.EntityType == entityType && e.EntityId == id
                       orderby e.Timestamp descending
                       select e;

        var messages = await entities.ProjectToSystemMessage()
                               .Skip((page ?? 0) * (pageSize ?? defaultPageSize))
                               .Take(pageSize ?? defaultPageSize)
                               .ToListAsync(cancellationToken);

        return Results.Ok(messages);
    }
}
