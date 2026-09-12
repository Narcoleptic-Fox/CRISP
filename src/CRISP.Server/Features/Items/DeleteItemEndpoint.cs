using CRISP.Core.Common;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that deletes items.
/// </summary>
public static class DeleteItemEndpoint
{
    /// <summary>
    /// Maps the delete item endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", HandleAsync)
        .WithName("DeleteItem")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>
    /// Handles an item delete request.
    /// </summary>
    /// <param name="command">The delete command supplied from route parameters.</param>
    /// <param name="service">The keyed item delete service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A no-content result when deletion succeeds.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] DeleteCommand command,
        [FromKeyedServices(nameof(Items))] IModifyService<DeleteCommand> service,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        return Results.NoContent();
    }
}

/// <summary>
/// Deletes items by identifier. The soft-delete interceptor converts the EF delete into a
/// soft delete (sets <c>IsDeleted</c>), and the entity's query filter then hides the row.
/// </summary>
internal sealed class DeleteItemService(ApplicationDbContext db, ILogger<DeleteItemService> logger) : IModifyService<DeleteCommand>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Deletes the requested item.
    /// </summary>
    /// <param name="command">The delete command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task-like value that completes when the item is deleted.</returns>
    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _items.FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);
        if (entity is null)
            throw RejectedException.NotFound($"Item with ID '{command.Id}' not found.");

        _items.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted item {Name} with ID {Id}.", entity.Name, entity.Id);
    }
}
