using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Identity.Endpoints;

public sealed class GetUserByEmailEndpoint : IQueryEndpoint<GetUserByEmail, User>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("email/{email}", Handle)
            .WithName(nameof(GetUserByEmail))
            .Produces<User>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(Permissions.CanReadUser)
            .CacheOutput(builder => builder.SetVaryByRouteValue("email").Tag(nameof(User)));
        return app;
    }
    public static async Task<IResult> Handle(
        [AsParameters] GetUserByEmail query,
        [FromServices] IQueryService<GetUserByEmail, User> service,
        CancellationToken cancellationToken)
    {
        User user = await service.Send(query);
        return TypedResults.Ok(user);
    }
}

public sealed class GetUserByEmailService : IQueryService<GetUserByEmail, User>
{
    private readonly ApplicationDbContext _context;

    public GetUserByEmailService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ValueTask<User> Send(GetUserByEmail query, CancellationToken cancellationToken = default)
    {
        User? user = _context.Users.AsNoTracking()
                                  .Where(e => e.Email == query.Email)
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
            throw new NotFoundException(query.Email, nameof(User));

        return ValueTask.FromResult(user);
    }
}
