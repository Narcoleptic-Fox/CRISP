using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetUserByIdEndpoint : ISingularQueryEndpoint<User>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("{id}", Handle)
            .WithName("GetUserById")
            .Produces<User>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanReadUser)
            .CacheOutput(builder => builder.SetVaryByRouteValue("id").Tag(nameof(User)));

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] SingularQuery<User> query,
        [FromServices] IQueryService<SingularQuery<User>, User> service,
        CancellationToken cancellationToken)
    {
        User user = await service.Send(query);
        return TypedResults.Ok(user);
    }
}

public sealed class GetUserService : IQueryService<SingularQuery<User>, User>
{
    private readonly ApplicationDbContext _context;

    public GetUserService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ValueTask<User> Send(SingularQuery<User> query, CancellationToken cancellationToken = default)
    {
        User? user = _context.Users.AsNoTracking()
                                   .Where(e => e.Id == query.Id)
                                   .Select(u => new User
                                   {
                                       Id = u.Id,
                                       UserName = u.UserName ?? "",
                                       Email = u.Email ?? "",
                                       PhoneNumber = u.PhoneNumber ?? "",
                                       LockoutEnd = u.LockoutEnd,
                                       LockOutEnabled = u.LockoutEnabled,
                                   })
                                   .FirstOrDefault();
        if (user is null)
            throw new NotFoundException(query.Id, nameof(User));

        return ValueTask.FromResult(user);
    }
}
