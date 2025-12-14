using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class UpdateUserEndpoint : IModifyEndpoint<UpdateUser>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPut("", Handle)
           .WithName(nameof(UpdateUser))
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
           .RequireAuthorization(Permissions.CanUpdateUser);

        return app;
    }
    public static async Task<IResult> Handle(
        [FromBody] UpdateUser command,
        [FromServices] IModifyService<UpdateUser> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync($"{nameof(User)}", cancellationToken);
        await cache.EvictByTagAsync($"{nameof(Users)}", cancellationToken);
        await eventDispatcher.Publish(new UserUpdated(command.Id), cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class UpdateUserService : IModifyService<UpdateUser>
{
    private readonly ApplicationDbContext _context;

    public UpdateUserService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask Send(UpdateUser command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException(command.Id, nameof(Users));

        user.UserName = command.UserName;
        user.Email = command.Email;
        user.PhoneNumber = command.PhoneNumber;
        _context.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
