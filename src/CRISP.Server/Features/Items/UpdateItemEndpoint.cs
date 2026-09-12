using CRISP.Core.Common;
using CRISP.Core.Items;
using CRISP.Server.Common;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using CRISP.Server.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that updates items.
/// </summary>
public static class UpdateItemEndpoint
{
    /// <summary>
    /// Maps the update item endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
        .WithName(nameof(UpdateItem))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>
    /// Handles an item update request.
    /// </summary>
    /// <param name="command">The item update command.</param>
    /// <param name="service">The item update service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A no-content result when the update succeeds.</returns>
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateItem command,
        [FromServices] IModifyService<UpdateItem> service,
        CancellationToken cancellationToken)
    {
        // The route is the authority on which item this is. RouteIdBinder has already copied it
        // onto the command before validation ran; this keeps the handler honest if the filter is
        // ever removed from the group.
        await service.Send(command with { Id = id }, cancellationToken);
        return Results.NoContent();
    }
}

/// <summary>
/// Updates items after enforcing unique names, publishing a rename event when the name changes.
/// </summary>
internal sealed class UpdateItemService(
    ApplicationDbContext db,
    IEventPublisher publisher,
    ILogger<UpdateItemService> logger) : IModifyService<UpdateItem>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Updates the requested item.
    /// </summary>
    /// <param name="command">The item update command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task-like value that completes when the item is updated.</returns>
    public async ValueTask Send(UpdateItem command, CancellationToken cancellationToken = default)
    {
        var entity = await _items.FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);
        if (entity is null)
            throw RejectedException.NotFound($"Item with ID '{command.Id}' not found.");

        if (await _items.AnyAsync(i => i.Id != command.Id && i.Name == command.Name, cancellationToken))
            throw RejectedException.Conflict($"Item with name '{command.Name}' already exists.");

        var previousName = entity.Name;

        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.Owner = command.Owner;
        entity.IsActive = command.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated item with ID {ItemId}.", entity.Id);

        // Demonstrates the in-process event bus: publish on rename so decoupled
        // handlers (see ItemRenamedLogger) can react.
        if (!string.Equals(previousName, command.Name, StringComparison.Ordinal))
            await publisher.Publish(new ItemRenamedEvent(entity.Id, previousName, command.Name), cancellationToken);
    }
}
