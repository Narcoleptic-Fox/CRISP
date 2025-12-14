using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetRoleByNameEndpoint : IQueryEndpoint<GetRoleByName, Role>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("by-name/{name}", Handle)
           .WithName(nameof(GetRoleByName))
           .Produces<Role>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .RequireAuthorization(Permissions.CanReadRole)
           .CacheOutput(builder => builder.Tag(nameof(Role)));

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] GetRoleByName query,
        [FromServices] IQueryService<GetRoleByName, Role> service,
        CancellationToken cancellationToken)
    {
        Role role = await service.Send(query, cancellationToken);
        return TypedResults.Ok(role);
    }
}

public sealed class GetRoleByNameService : IQueryService<GetRoleByName, Role>
{
    private readonly ApplicationDbContext _context;

    public GetRoleByNameService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<Role> Send(GetRoleByName query, CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(e => e.Name == query.Name, cancellationToken);
        if (role is null)
            throw new NotFoundException(query.Name, nameof(Role));

        return new()
        {
            Name = role.Name ?? "",
            Permissions = role.PermissionsList,
        };
    }
}
