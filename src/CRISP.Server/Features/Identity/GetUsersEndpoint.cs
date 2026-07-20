using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Core.Common;
using CRISP.Core.Extensions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that searches users.
/// </summary>
public static class GetUsersEndpoint
{
    /// <summary>
    /// Maps the get users endpoint to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
        .WithName(nameof(GetUsers))
        .Produces<PagedResponse<User>>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }


    /// <summary>
    /// Handles a paged user search request.
    /// </summary>
    /// <param name="query">The user search query.</param>
    /// <param name="service">The user search service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing a paged user response.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetUsers query,
        [FromServices] IQueryService<GetUsers, PagedResponse<User>> service,
        CancellationToken cancellationToken)
    {
        var users = await service.Send(query, cancellationToken);
        return Results.Ok(users);
    }
}

/// <summary>
/// Searches users using optional filters, sorting, and paging.
/// </summary>
internal sealed class GetUsersService(ApplicationDbContext db, ILogger<GetUsersService> logger) : IQueryService<GetUsers, PagedResponse<User>>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Sends a paged user search query.
    /// </summary>
    /// <param name="query">The user search query.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A paged response of matching users.</returns>
    public async ValueTask<PagedResponse<User>> Send(GetUsers query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Searching users with filters: {Filters}", new
        {
            query.Ids,
            query.Email,
            query.Username,
            query.Role,
            query.IsActive
        });
        var entities = from e in _users.AsNoTracking().SortBy(query.SortBy, query.SortDescending)
                       where query.Ids == null || !query.Ids.Any() || query.Ids.Contains(e.Id)
                       where query.Email == null || e.Email.Contains(query.Email)
                       where query.Username == null || e.Username.Contains(query.Username)
                       where query.Role == null || e.Role == query.Role
                       where query.IsActive == null || e.IsActive == query.IsActive
                       select e;

        var totalCount = await entities.CountAsync(cancellationToken);
        var users = await entities.ProjectToUser()
                                  .ToPaged(query.Page, query.PageSize)
                                  .ToListAsync(cancellationToken);

        return new PagedResponse<User>
        {
            Items = users,
            TotalCount = totalCount
        };
    }
}
