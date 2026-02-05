using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class DeleteRoleEndpoint : IModifyEndpoint<DeleteCommand>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteCommand))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanDeleteRole);

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] DeleteCommand command,
        [FromKeyedServices(nameof(Roles))] IModifyService<DeleteCommand> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await eventDispatcher.Publish(new RoleDeleted(new Role()), cancellationToken);
        await cache.EvictByTagAsync(nameof(Role), cancellationToken);
        await cache.EvictByTagAsync(nameof(Roles), cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class DeleteRoleService : IModifyService<DeleteCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteRoleService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
        if (role is null)
            throw new NotFoundException(command.Id, nameof(role));
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
