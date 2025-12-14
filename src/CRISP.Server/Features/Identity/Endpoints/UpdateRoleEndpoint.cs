using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class UpdateRoleEndpoint : IModifyEndpoint<UpdateRole>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPut("", Handle)
            .WithName(nameof(UpdateRole))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanUpdateRole);
        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] UpdateRole command,
        [FromServices] IModifyService<UpdateRole> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await eventDispatcher.Publish(new RoleUpdated(command.Id), cancellationToken);
        await cache.EvictByTagAsync(nameof(Role), cancellationToken);
        await cache.EvictByTagAsync(nameof(Roles), cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class UpdateRoleService : IModifyService<UpdateRole>
{
    private readonly ApplicationDbContext _context;

    public UpdateRoleService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async ValueTask Send(UpdateRole command, CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
        if (role is null)
            throw new NotFoundException(command.Id, nameof(role));
        role.Name = command.Name;
        role.PermissionsList = command.Permissions;
        _context.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
