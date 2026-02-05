using CRISP.Core.Identity;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class CreateRoleEndpoint : ICreateEndpoint<CreateRole>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPost("", Handle)
           .WithName(nameof(CreateRole))
           .Produces<Guid>(StatusCodes.Status201Created)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .RequireAuthorization(Permissions.CanCreateRole);

        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] CreateRole command,
        [FromServices] ICreateService<CreateRole> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        Guid id = await service.Send(command, cancellationToken);
        await eventDispatcher.Publish(new RoleCreated(id), cancellationToken);
        await cache.EvictByTagAsync(nameof(Role), cancellationToken);
        await cache.EvictByTagAsync(nameof(Roles), cancellationToken);
        return TypedResults.Created($"{nameof(Identity)}/{nameof(Roles)}/{id}", id);
    }
}

public sealed class CreateRoleService : ICreateService<CreateRole>
{
    public ValueTask<Guid> Send(CreateRole command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
