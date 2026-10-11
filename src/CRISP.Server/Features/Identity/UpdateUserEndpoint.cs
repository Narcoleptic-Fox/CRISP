using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Common;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using CRISP.Server.Events;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that updates users.
/// </summary>
public static class UpdateUserEndpoint
{
    /// <summary>
    /// Maps the update user endpoint to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPut("/", HandleAsync)
        .WithName(nameof(UpdateUser))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }


    /// <summary>
    /// Handles a user update request.
    /// </summary>
    /// <param name="command">The user update command.</param>
    /// <param name="service">The user update service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A no-content result when the update succeeds.</returns>
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateUser command,
        [FromServices] IModifyService<UpdateUser> service,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        return Results.NoContent();
    }
}

/// <summary>
/// Updates users by identifier.
/// </summary>
internal sealed class UpdateUserService(
    ApplicationDbContext db,
    IEventPublisher events,
    ILogger<UpdateUserService> logger) : IModifyService<UpdateUser>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Updates the requested user.
    /// </summary>
    /// <param name="command">The user update command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task-like value that completes when the user is updated.</returns>
    public async ValueTask Send(UpdateUser command, CancellationToken cancellationToken = default)
    {
        var entity = await _users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken: cancellationToken);
        if (entity is null)
            throw RejectedException.NotFound($"User with ID '{command.Id}' not found.");

        var previousUsername = entity.Username;

        entity.Username = command.Username;
        entity.Email = command.Email;
        entity.Role = command.Role;
        entity.IsActive = command.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated user with ID {UserId}", entity.Id);

        // Asset rows snapshot the owner username; propagate the rename so they
        // don't keep displaying the old value.
        if (!string.Equals(previousUsername, command.Username, StringComparison.Ordinal))
            await events.Publish(new UsernameChangedEvent(previousUsername, command.Username), cancellationToken);
    }
}
