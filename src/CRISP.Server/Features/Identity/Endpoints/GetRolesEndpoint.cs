using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetRolesEndpoint : IPagedQueryEndpoint<GetRoles, Roles>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("", Handle)
            .WithName(nameof(GetRoles))
            .Produces<PagedResponse<Role>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanReadRole)
            .CacheOutput(builder => builder.SetVaryByQuery(
                nameof(GetRoles.Page),
                nameof(GetRoles.PageSize),
                nameof(GetRoles.SortBy),
                nameof(GetRoles.SortDescending),
                nameof(GetRoles.Ids),
                nameof(GetRoles.Names),
                nameof(GetRoles.Permissions))
                .Tag(nameof(Roles)));
        return app;
    }
    public static async Task<IResult> Handle(
        [AsParameters] GetRoles query,
        [FromServices] IQueryService<GetRoles, PagedResponse<Roles>> service,
        CancellationToken cancellationToken)
    {
        PagedResponse<Roles> response = await service.Send(query, cancellationToken);
        return TypedResults.Ok(response);
    }
}

public sealed class GetRolesService : IQueryService<GetRoles, PagedResponse<Roles>>
{
    private readonly ApplicationDbContext _context;

    public GetRolesService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<PagedResponse<Roles>> Send(GetRoles query, CancellationToken cancellationToken)
    {
        IQueryable<ApplicationRole> rolesQuery = from r in _context.Roles.AsNoTracking().OrderBy(e => e.Id)
                                                 where query.Ids == null || query.Ids.Contains(r.Id)
                                                 where query.Names == null || query.Names.Contains(r.Name)
                                                 where query.Permissions == null || query.Permissions.Any(p => r.Permission.Contains($"{p}"))
                                                 select r;

        int totalCount = await rolesQuery.CountAsync(cancellationToken);
        List<ApplicationRole> rolesList = await rolesQuery.OrderBy(query.GetSortByOrDefault(), query.GetSortDescendingOrDefault())
                                                          .Skip(query.GetPageOrDefault() * query.GetPageSizeOrDefault())
                                                          .Take(query.GetPageSizeOrDefault())
                                                          .ToListAsync(cancellationToken);

        return new()
        {
            Items = rolesList.Select(r => new Roles
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Permissions = r.PermissionsList
            }).ToList(),
            TotalCount = totalCount,
        };
    }
}
