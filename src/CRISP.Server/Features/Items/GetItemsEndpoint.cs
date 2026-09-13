using CRISP.Core;
using CRISP.Core.Common;
using CRISP.Core.Extensions;
using CRISP.Core.Items;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//#if (IncludeAuth)
using CRISP.Server.Authorization;
//#endif

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that searches items.
/// </summary>
public static class GetItemsEndpoint
{
    /// <summary>
    /// Maps the get items endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
        .WithName(nameof(GetItems))
//#if (IncludeAuth)
        .RequireAuthorization(Policies.ReadAccess)
//#endif
        .Produces<PagedResponse<Item>>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Handles a paged item search request.
    /// </summary>
    /// <param name="query">The item search query.</param>
    /// <param name="service">The item search service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing a paged item response.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetItems query,
        [FromServices] IQueryService<GetItems, PagedResponse<Item>> service,
        CancellationToken cancellationToken)
    {
        var items = await service.Send(query, cancellationToken);
        return Results.Ok(items);
    }
}

/// <summary>
/// Searches items using optional filters, sorting, and paging.
/// </summary>
internal sealed class GetItemsService(ApplicationDbContext db) : IQueryService<GetItems, PagedResponse<Item>>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Sends a paged item search query.
    /// </summary>
    /// <param name="query">The item search query.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A paged response of matching items.</returns>
    public async ValueTask<PagedResponse<Item>> Send(GetItems query, CancellationToken cancellationToken = default)
    {
        var entities = from e in _items.AsNoTracking().SortBy(query.SortBy, query.SortDescending)
                       where query.Ids == null || !query.Ids.Any() || query.Ids.Contains(e.Id)
                       where query.Name == null || e.Name.Contains(query.Name)
                       where query.Owner == null || (e.Owner != null && e.Owner.Contains(query.Owner))
                       where query.IsActive == null || e.IsActive == query.IsActive
                       select e;
        var totalCount = await entities.CountAsync(cancellationToken);
        var items = await entities.ProjectToItem()
                                  .ToPaged(query.Page, query.PageSize)
                                  .ToListAsync(cancellationToken);
        return new PagedResponse<Item>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
