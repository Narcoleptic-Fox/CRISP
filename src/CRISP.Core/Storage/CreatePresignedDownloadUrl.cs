namespace CRISP.Core.Storage;

/// <summary>
/// Represents a request to create a temporary download URL for a file attached to an entity.
/// </summary>
[ExportTsInterface]
public sealed record CreatePresignedDownloadUrl
{
    /// <summary>
    /// Gets the entity type that owns the requested file.
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the entity that owns the requested file.
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the requested file metadata record.
    /// </summary>
    public Guid FileId { get; init; }

    /// <summary>
    /// Gets the requested number of minutes before the presigned download URL expires.
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 15;
}

/// <summary>
/// Validates presigned download URL requests before storage access is granted.
/// </summary>
public sealed class CreatePresignedDownloadUrlValidator : AbstractValidator<CreatePresignedDownloadUrl>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePresignedDownloadUrlValidator"/> class.
    /// </summary>
    public CreatePresignedDownloadUrlValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(100).WithMessage("Entity type must not exceed 100 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required.");

        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required.");

        RuleFor(x => x.ExpiresInMinutes)
            .InclusiveBetween(1, 60).WithMessage("Expiration must be between 1 and 60 minutes.");
    }
}