using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CRISP.Server.Features.Identity.Endpoints;

public class DeleteUserEndpoint : IModifyEndpoint<DeleteCommand>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapDelete("{id}", Handle)
            .WithName("DeleteUser")
            .Produces<IResult>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanDeleteUser);

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] DeleteCommand command,
        [FromKeyedServices(nameof(Users))] IModifyService<DeleteCommand> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync($"{nameof(User)}", cancellationToken);
        await cache.EvictByTagAsync($"{nameof(Users)}", cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class DeleteUserService : IModifyService<DeleteCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteUserService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _context.Users.FindAsync(command.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException(command.Id, nameof(User));
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
