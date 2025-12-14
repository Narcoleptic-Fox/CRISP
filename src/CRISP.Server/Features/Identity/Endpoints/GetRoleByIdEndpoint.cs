using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetRoleByIdEndpoint : IQueryEndpoint<SingularQuery<Role>, Role>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("/{id:guid}", Handle)
           .WithName("GetRoleById")
           .Produces<Role>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .RequireAuthorization(Permissions.CanReadRole)
           .CacheOutput(builder => builder.Tag(nameof(Role)));

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] SingularQuery<Role> query,
        [FromServices] IQueryService<SingularQuery<Role>, Role> service,
        CancellationToken cancellationToken)
    {
        Role role = await service.Send(query, cancellationToken);
        return TypedResults.Ok(role);
    }
}

public sealed class GetRoleByIdService : IQueryService<SingularQuery<Role>, Role>
{
    private readonly ApplicationDbContext _context;

    public GetRoleByIdService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<Role> Send(SingularQuery<Role> query, CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken);
        if (role is null)
            throw new NotFoundException(query.Id, nameof(Role));

        return new()
        {
            Name = role.Name ?? "",
            Permissions = role.PermissionsList,
        };
    }
}
