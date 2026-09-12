using CRISP.Core.Common;
using CRISP.Core.Items;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that creates items.
/// </summary>
public static class CreateItemEndpoint
{
    /// <summary>
    /// Maps the create item endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
        .WithName(nameof(CreateItem))
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Handles an item creation request.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="version">The API version route value.</param>
    /// <param name="command">The item creation command.</param>
    /// <param name="service">The item creation service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A created result containing the new item identifier.</returns>
    public static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromRoute] string version,
        [FromBody] CreateItem command,
        [FromServices] ICreateService<CreateItem> service,
        CancellationToken cancellationToken)
    {
        var id = await service.Send(command, cancellationToken);
        return Results.Created($"/api/v{version}/{nameof(Item).Pluralize()}/{id}", id);
    }
}

/// <summary>
/// Creates items after enforcing unique names.
/// </summary>
internal sealed class CreateItemService(ApplicationDbContext db, ILogger<CreateItemService> logger) : ICreateService<CreateItem>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Creates an item and returns its identifier.
    /// </summary>
    /// <param name="command">The item creation command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The identifier of the created item.</returns>
    public async ValueTask<Guid> Send(CreateItem command, CancellationToken cancellationToken = default)
    {
        if (await _items.AnyAsync(i => i.Name == command.Name, cancellationToken))
            throw RejectedException.Conflict($"Item with name '{command.Name}' already exists.");

        var entity = new ItemEntity
        {
            Name = command.Name,
            Description = command.Description,
            Owner = command.Owner,
            IsActive = true
        };
        _items.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created item {Name} with ID {Id}.", entity.Name, entity.Id);
        return entity.Id;
    }
}
