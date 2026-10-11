using CRISP.Core.Common;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that deletes users.
/// </summary>
public static class DeleteUserEndpoint
{
    /// <summary>
    /// Maps the delete user endpoint to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", HandleAsync)
        .WithName("DeleteUser")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }


    /// <summary>
    /// Handles a user delete request.
    /// </summary>
    /// <param name="command">The delete command supplied from route parameters.</param>
    /// <param name="service">The keyed user delete service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A no-content result when deletion succeeds.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] DeleteCommand command,
        [FromKeyedServices(nameof(Identity))] IModifyService<DeleteCommand> service,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        return Results.NoContent();
    }
}

/// <summary>
/// Deletes users by identifier.
/// </summary>
internal sealed class DeleteUserService(ApplicationDbContext db) : IModifyService<DeleteCommand>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Deletes the requested user.
    /// </summary>
    /// <param name="command">The delete command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task-like value that completes when the user is deleted.</returns>
    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _users.FindAsync([command.Id], cancellationToken);
        if (entity == null)
            throw RejectedException.NotFound($"User with ID '{command.Id}' not found.");

        _users.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}