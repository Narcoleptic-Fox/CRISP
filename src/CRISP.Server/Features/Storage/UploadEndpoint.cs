//#if (IncludeAuth)
using CRISP.Core.Identity;
//#endif
using CRISP.Core.Storage;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using CRISP.Server.Data.ValueObjects;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//#if (IncludeAuth)
using CRISP.Server.Authorization;
//#endif
using CRISP.Server.Common;
using System.Security.Claims;

namespace CRISP.Server.Features.Storage;

public static class UploadEndpoint
{
    private static readonly IEnumerable<Type> HasFileEntities = typeof(IHasStoredFiles).Assembly.GetTypes()
                                                                                       .Where(t => typeof(IHasStoredFiles).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                                                                       .ToList();
//#if (IncludeAuth)
    private static async Task<(string Name, Roles Role)?> GetEntityInfo(ApplicationDbContext db, string key, CancellationToken cancellationToken)
    {
        var user = await db.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == key, cancellationToken);
        if (user is not null)
            return (user.Username, user.Role);

        var apiKey = await db.Set<ApiKeyEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Name == key, cancellationToken);
        if (apiKey is not null)
            return (apiKey.Name, apiKey.Roles.Max());
        return null;
    }
//#endif

    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPost("/upload", HandleAsync)
        .WithName("Upload")
//#if (IncludeAuth)
        .RequireAuthorization(Policies.WriteAccess)
//#endif
        .Produces<PresignedUrl>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    public static async Task<IResult> HandleAsync(
       [FromBody] CreatePresignedUploadUrl command,
       [FromServices] IObjectStorage service,
       [FromServices] ApplicationDbContext db,
       ClaimsPrincipal claimsPrincipal,
       CancellationToken cancellationToken)
    {
        // The name recorded on the stored file. With authentication it is the provisioned user or
        // API key behind the principal; without, the same "System" the audit interceptor writes.
        var uploadedBy = "System";
//#if (IncludeAuth)
        var userKey =
        claimsPrincipal.FindFirstValue(ClaimTypes.Email) ??
        claimsPrincipal.FindFirstValue("preferred_username") ??
        claimsPrincipal.FindFirstValue("upn") ??
        claimsPrincipal.Identity?.Name;
        var user = await GetEntityInfo(db, userKey ?? "", cancellationToken);

        if (user is null)
            return Results.Unauthorized();
        if (user.Value.Role < Roles.Editor)
            return Results.Forbid();
        uploadedBy = user.Value.Name;
//#endif

        var entityName = command.EntityType.Contains("Entity") ? command.EntityType : $"{command.EntityType}Entity";
        var entityType = HasFileEntities.FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (entityType is null)
            return Results.BadRequest($"Entity does not support file attachments: {command.EntityType}");

        var entity = await db.FindAsync(entityType, [command.EntityId], cancellationToken);
        if (entity is null)
            return Results.BadRequest($"Entity not found: {command.EntityType} with ID {command.EntityId}");
        if (entity is not IHasStoredFiles hasFilesEntity)
            return Results.BadRequest($"Entity does not support file attachments: {command.EntityType}");


        var fileId = Guid.NewGuid();
        var extension = MimeTypes.GetMimeTypeExtensions(command.ContentType).FirstOrDefault() ?? Path.GetExtension(command.OriginalFileName);
        if (!string.IsNullOrWhiteSpace(extension) && !extension.StartsWith('.'))
            extension = $".{extension}";
        var key = $"{command.EntityType.Pluralize()}/{command.EntityId}/files/{fileId}{extension}";
        var result = await service.CreateUploadUrlAsync(new ObjectUploadUrlRequest(
            key,
            command.ContentType,
            command.ExpiresInMinutes),
            cancellationToken);

        var stored = new StoredFileObject
        {
            Id = fileId,
            OriginalFileName = command.OriginalFileName,
            ContentType = command.ContentType,
            SizeBytes = command.SizeBytes,
            ObjectKey = key,
            UploadedOn = DateTimeOffset.UtcNow,
            UploadedBy = uploadedBy
        };
        // Adding to the tracked owner's collection is enough to mark the file Added. db.Entry() cannot
        // be used here: StoredFileObject is an owned shared-type entity, so it is only reachable
        // through its owner's collection entry.
        hasFilesEntity.Files.Add(stored);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(result);
    }
}
