using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using FluentValidation;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that creates users.
/// </summary>
public static class CreateUserEndpoint
{
    /// <summary>
    /// Maps the create user endpoint to the user route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
        .WithName(nameof(CreateUser))
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }


    /// <summary>
    /// Handles a user creation request.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="version">The API version route value.</param>
    /// <param name="command">The user creation command.</param>
    /// <param name="service">The user creation service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A created result containing the new user identifier.</returns>
    public static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromRoute] string version,
        [FromBody] CreateUser command,
        [FromServices] ICreateService<CreateUser> service,
        CancellationToken cancellationToken)
    {
        var id = await service.Send(command, cancellationToken);
        return Results.Created($"/api/v{version}/{nameof(User).Pluralize()}/{id}", id);
    }
}

/// <summary>
/// Creates users after enforcing unique usernames and email addresses.
/// </summary>
internal sealed class CreateUserService(ApplicationDbContext db, ILogger<CreateUserService> logger) : ICreateService<CreateUser>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Creates a user and returns its identifier.
    /// </summary>
    /// <param name="command">The user creation command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The identifier of the created user.</returns>
    public async ValueTask<Guid> Send(CreateUser command, CancellationToken cancellationToken = default)
    {
        var entity = await _users.FirstOrDefaultAsync(u => u.Username == command.Username, cancellationToken: cancellationToken);
        if (entity is not null)
            throw RejectedException.Conflict($"User with username '{command.Username}' already exists.");

        entity = await _users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (entity is not null)
            throw RejectedException.Conflict($"User with email '{command.Email}' already exists.");

        entity = new UserEntity
        {
            Username = command.Username,
            Email = command.Email,
            Role = command.Role,
            IsActive = command.IsActive,
        };
        _users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created new user with ID {UserId}", entity.Id);
        return entity.Id;
    }

}