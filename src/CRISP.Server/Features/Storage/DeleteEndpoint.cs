using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using CRISP.Server.Authorization;
using CRISP.Server.Common;

namespace CRISP.Server.Features.Storage;

public static class DeleteEndpoint
{
    private static readonly IEnumerable<Type> HasFileEntities = typeof(IHasStoredFiles).Assembly.GetTypes()
                                                                                       .Where(t => typeof(IHasStoredFiles).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                                                                       .ToList();

    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapDelete("/{fileId:guid}", HandleAsync)
        .WithName("DeleteFile")
        .RequireAuthorization(Policies.WriteAccess)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    public static async Task<IResult> HandleAsync(
        Guid fileId,
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromServices] IObjectStorage storage,
        [FromServices] ApplicationDbContext db,
        [FromServices] ILogger<IObjectStorage> logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return Results.BadRequest("Entity type is required.");
        if (entityId == Guid.Empty)
            return Results.BadRequest("Entity ID is required.");

        var entityName = entityType.Contains("Entity") ? entityType : $"{entityType}Entity";
        var resolvedType = HasFileEntities.FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (resolvedType is null)
            return Results.BadRequest($"Entity does not support file attachments: {entityType}");

        var entity = await db.FindAsync(resolvedType, [entityId], cancellationToken);
        if (entity is null)
            return Results.BadRequest($"Entity not found: {entityType} with ID {entityId}");
        if (entity is not IHasStoredFiles hasFilesEntity)
            return Results.BadRequest($"Entity does not support file attachments: {entityType}");

        // Only attached files are deletable. The thumbnail lives outside the Files collection and is
        // replaced through the asset edit flow, so it must not be reachable from here.
        var storedFile = hasFilesEntity.Files.FirstOrDefault(f => f.Id == fileId);
        if (storedFile is null)
            return Results.BadRequest($"File ID {fileId} is not associated with entity {entityType} with ID {entityId}");

        hasFilesEntity.Files.Remove(storedFile);
        await db.SaveChangesAsync(cancellationToken);

        // The metadata row is already gone, so the caller can never retry this object key. Log it
        // loudly rather than failing the request, otherwise a transient storage error leaves the UI
        // showing a file that no longer exists.
        try
        {
            await storage.DeleteAsync(storedFile.ObjectKey, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Deleted file metadata {FileId} but failed to delete object {ObjectKey} from storage. The object is now orphaned.",
                fileId, storedFile.ObjectKey);
        }

        return Results.NoContent();
    }
}
