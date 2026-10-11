using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Maps and handles the endpoint that updates user settings.
/// </summary>
public static class UpdateUserSettingsEndpoint
{
    /// <summary>
    /// Maps the update user settings endpoint to the user settings route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPut("/", HandleAsync)
        .WithName(nameof(UpdateUserSettings))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }


    /// <summary>
    /// Handles a user settings update request.
    /// </summary>
    /// <param name="command">The user settings update command.</param>
    /// <param name="service">The user settings update service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A no-content result when the update succeeds.</returns>
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateUserSettings command,
        [FromServices] IModifyService<UpdateUserSettings> service,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        return Results.NoContent();
    }
}

/// <summary>
/// Updates user preference settings.
/// </summary>
internal sealed class UpdateUserSettingsService(
    ApplicationDbContext db,
    ICurrentUser currentUser,
    ILogger<UpdateUserService> logger) : IModifyService<UpdateUserSettings>
{
    private readonly DbSet<UserEntity> _users = db.Set<UserEntity>();

    /// <summary>
    /// Updates the settings of the caller's own account. The <see cref="UpdateUserSettings.Id"/>
    /// in the request body is intentionally ignored — the target is resolved from the
    /// authenticated principal's email so a caller can only ever change their own settings.
    /// </summary>
    /// <param name="command">The user settings update command.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task-like value that completes when the settings are updated.</returns>
    public async ValueTask Send(UpdateUserSettings command, CancellationToken cancellationToken = default)
    {
        var email = currentUser.Email;
        if (string.IsNullOrEmpty(email))
            throw RejectedException.Forbidden("No authenticated identity to update settings for.");

        var entity = await _users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken: cancellationToken);
        if (entity is null)
            throw RejectedException.NotFound($"No application account exists for '{email}'.");

        entity.Settings = new UserSettings
        {
            AllowNotifications = command.AllowNotifications
        };
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated settings for user with ID {UserId}", entity.Id);
    }
}
