using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;
using System.Security.Claims;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles endpoints that return the authenticated user's application profile.
/// </summary>
public static class GetCurrentEndpoint
{
    /// <summary>
    /// Maps the current-user aliases to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/me", HandleAsync)
        .WithName("GetMyUser")
        .Produces<User>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/current", HandleAsync)
        .WithName("GetCurrent")
        .Produces<User>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }


    /// <summary>
    /// Handles a current-user lookup request using claims from the authenticated principal.
    /// </summary>
    /// <param name="claimsPrincipal">The authenticated principal.</param>
    /// <param name="db">The application database context.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result with the current user, unauthorized when no identity claim exists, or not found when no user matches.</returns>
    public static async Task<IResult> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(currentUser.Email))
            return Results.Unauthorized();

        var user = await currentUser.Get(cancellationToken);
        return user is not null
            ? Results.Ok(user)
            : Results.Problem(
                title: "Account not provisioned",
                detail: "No application account exists for this identity.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { ["code"] = "unprovisioned" });
    }
}

internal sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ApplicationDbContext _db;

    public CurrentUserService(IHttpContextAccessor contextAccessor, ApplicationDbContext db)
    {
        _contextAccessor = contextAccessor;
        _db = db;
    }

    public string Username =>
        _contextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

    public string? Email =>
        _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ??
        _contextAccessor.HttpContext?.User.FindFirstValue("preferred_username") ??
        _contextAccessor.HttpContext?.User.FindFirstValue("upn") ??
        _contextAccessor.HttpContext?.User.Identity?.Name;

    public async ValueTask<User?> Get(CancellationToken cancellationToken = default)
    {
        var email = Email;
        if (string.IsNullOrEmpty(email))
            return null;

        var entity = await _db.Set<UserEntity>()
                              .AsNoTracking()
                              .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return entity?.ToUser();
    }
}
