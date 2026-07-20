using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that retrieves a single user.
/// </summary>
public static class GetUserEndpoint
{
    /// <summary>
    /// Maps the get user endpoint to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", HandleAsync)
        .WithName(nameof(GetUser))
        .Produces<User>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }


    /// <summary>
    /// Handles a single-user query request.
    /// </summary>
    /// <param name="query">The user query supplied from route parameters.</param>
    /// <param name="service">The user query service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing the requested user.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetUser query,
        [FromServices] IQueryService<GetUser, User> service,
        CancellationToken cancellationToken)
    {
        User user = await service.Send(query, cancellationToken);
        return Results.Ok(user);
    }
}

/// <summary>
/// Retrieves users by identifier.
/// </summary>
internal sealed class GetUserService(ApplicationDbContext db) : IQueryService<GetUser, User>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Sends a single-user query.
    /// </summary>
    /// <param name="query">The user query.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The requested user.</returns>
    public async ValueTask<User> Send(GetUser query, CancellationToken cancellationToken = default)
    {
        var entity = await _users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken: cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"User with ID '{query.Id}' not found.");

        return entity.ToUser();
    }
}