using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity;

public static class GetUserNamesEndpoint
{
    /// <summary>
    /// Maps the get vendors endpoint to the vendor route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/names", HandleAsync)
        .WithName(nameof(GetUserNames))
        .Produces<IEnumerable<AutoComplete>>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Handles a paged vendor search request.
    /// </summary>
    /// <param name="query">The vendor search query.</param>
    /// <param name="service">The vendor search service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing a paged vendor response.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetUserNames query,
        [FromServices] IQueryService<GetUserNames, IEnumerable<AutoComplete>> service,
        CancellationToken cancellationToken)
    {
        var users = await service.Send(query, cancellationToken);
        return Results.Ok(users);
    }
}

internal sealed class GetUserNamesService(ApplicationDbContext db) : IQueryService<GetUserNames, IEnumerable<AutoComplete>>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();
    /// <summary>
    /// Sends a paged vendor search query.
    /// </summary>
    /// <param name="query">The vendor search query.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A list of autocomplete models for users matching the query.</returns>
    public async ValueTask<IEnumerable<AutoComplete>> Send(GetUserNames query, CancellationToken cancellationToken)
    {
        return await _users
            .Where(u => u.IsActive)
            .Where(u => query.SearchTerm == null || u.Username.Contains(query.SearchTerm))
            .ProjectToAutoComplete()
            .ToListAsync(cancellationToken);
    }
}
