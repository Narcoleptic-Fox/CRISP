using CRISP.Core.Storage;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using CRISP.Server.Authorization;
using CRISP.Server.Common;
using System.Security.Claims;

namespace CRISP.Server.Features.Storage;

public static class DownloadEndpoint
{
    private static readonly IEnumerable<Type> HasFileEntities = typeof(IHasStoredFiles).Assembly.GetTypes()
                                                                                       .Where(t => typeof(IHasStoredFiles).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                                                                       .ToList();
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPut("/download", HandleAsync)
        .WithName("Download")
        .RequireAuthorization(Policies.ReadAccess)
        .Produces<PresignedUrl>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    public static async Task<IResult> HandleAsync(
       [FromBody] CreatePresignedDownloadUrl command,
       [FromServices] IObjectStorage service,
       [FromServices] ApplicationDbContext db,
       ClaimsPrincipal claimsPrincipal,
       CancellationToken cancellationToken)
    {
        var entityName = command.EntityType.Contains("Entity") ? command.EntityType : $"{command.EntityType}Entity";
        var entityType = HasFileEntities.FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (entityType is null)
            return Results.BadRequest($"Entity does not support file attachments: {command.EntityType}");
        var entity = await db.FindAsync(entityType, [command.EntityId], cancellationToken);
        db.ChangeTracker.Clear(); // Detach the entity to avoid tracking issues
        if (entity is null)
            return Results.BadRequest($"Entity not found: {command.EntityType} with ID {command.EntityId}");
        if (entity is not IHasStoredFiles hasFilesEntity)
            return Results.BadRequest($"Entity does not support file attachments: {command.EntityType}");

        // The thumbnail is stored outside the Files collection, so fall back to it when the
        // requested id isn't an attached file (lets a single download flow serve both).
        var storedFile = hasFilesEntity.Files.FirstOrDefault(f => f.Id == command.FileId);
        if (storedFile is null && entity is IHasThumbnail hasThumbnail && hasThumbnail.Thumbnail?.Id == command.FileId)
            storedFile = hasThumbnail.Thumbnail;
        if (storedFile is null)
            return Results.BadRequest($"File ID {command.FileId} is not associated with entity {command.EntityType} with ID {command.EntityId}");

        var request = new ObjectDownloadUrlRequest(
            storedFile.ObjectKey,
            command.ExpiresInMinutes,
            storedFile.OriginalFileName);
        var result = await service.CreateDownloadUrlAsync(request, cancellationToken);
        return Results.Ok(result);
    }
}
