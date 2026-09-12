using CRISP.Core.Common;
using CRISP.Core.Items;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that retrieves a single item.
/// </summary>
public static class GetItemEndpoint
{
    /// <summary>
    /// Maps the get item endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", HandleAsync)
        .WithName(nameof(GetItem))
        .Produces<Item>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>
    /// Handles a single-item query request.
    /// </summary>
    /// <param name="query">The item query supplied from route parameters.</param>
    /// <param name="service">The item query service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing the requested item.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetItem query,
        [FromServices] IQueryService<GetItem, Item> service,
        CancellationToken cancellationToken)
    {
        Item item = await service.Send(query, cancellationToken);
        return Results.Ok(item);
    }
}

/// <summary>
/// Retrieves items by identifier, including attached files.
/// </summary>
internal sealed class GetItemService(ApplicationDbContext db) : IQueryService<GetItem, Item>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Sends a single-item query.
    /// </summary>
    /// <param name="query">The item query.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The requested item.</returns>
    public async ValueTask<Item> Send(GetItem query, CancellationToken cancellationToken = default)
    {
        // Owned file metadata is included automatically with the owner.
        var entity = await _items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken);
        if (entity is null)
            throw RejectedException.NotFound($"Item with ID '{query.Id}' not found.");
        return entity.ToItem();
    }
}
