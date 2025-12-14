using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetUsersEndpoint : IPagedQueryEndpoint<GetUsers, Users>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("", Handle)
            .WithName(nameof(GetUsers))
            .Produces<PagedResponse<Users>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanReadUser)
            .CacheOutput(builder => builder.SetVaryByQuery(
                nameof(GetRoles.Page),
                nameof(GetRoles.PageSize),
                nameof(GetRoles.SortBy),
                nameof(GetRoles.SortDescending),
                nameof(GetUsers.Ids),
                nameof(GetUsers.UserNames),
                nameof(GetUsers.Emails),
                nameof(GetUsers.PhoneNumbers),
                nameof(GetUsers.LockedOut))
                .Tag(nameof(Users)));

        return app;
    }
    public static async Task<IResult> Handle(
        [AsParameters] GetUsers query,
        [FromServices] IQueryService<GetUsers, PagedResponse<Users>> service,
        CancellationToken cancellationToken)
    {
        PagedResponse<Users> response = await service.Send(query, cancellationToken);
        return TypedResults.Ok(response);
    }
}

public sealed class GetUsersService : IQueryService<GetUsers, PagedResponse<Users>>
{
    private readonly ApplicationDbContext _context;
    public GetUsersService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async ValueTask<PagedResponse<Users>> Send(GetUsers query, CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> usersQuery = from e in _context.Users.AsNoTracking().OrderBy(u => u.Id)
                                                 where query.Ids == null || query.Ids.Contains(e.Id)
                                                 where query.UserNames == null || (e.UserName != null && query.UserNames.Contains(e.UserName))
                                                 where query.Emails == null || (e.Email != null && query.Emails.Contains(e.Email))
                                                 where query.PhoneNumbers == null || (e.PhoneNumber != null && query.PhoneNumbers.Contains(e.PhoneNumber))
                                                 where query.LockedOut == null || query.LockedOut == query.LockedOut
                                                 select e;

        int totalCount = await usersQuery.CountAsync(cancellationToken);
        List<ApplicationUser> usersList = await usersQuery.OrderBy(query.GetSortByOrDefault(), query.GetSortDescendingOrDefault())
                                                          .Skip(query.GetPageOrDefault() * query.GetPageSizeOrDefault())
                                                          .Take(query.GetPageSizeOrDefault())
                                                          .ToListAsync(cancellationToken);

        return new()
        {
            Items = usersList.Select(u => new Users
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
            }).ToList(),
            TotalCount = totalCount,
        };
    }
}
