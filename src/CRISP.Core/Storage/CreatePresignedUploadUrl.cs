namespace CRISP.Core.Storage;

/// <summary>
/// Represents a request to create a temporary upload URL for a file attached to an entity.
/// </summary>
[ExportTsInterface]
public sealed record CreatePresignedUploadUrl
{
    /// <summary>
    /// Gets the entity type that will own the uploaded file.
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the entity that will own the uploaded file.
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Gets the original file name supplied by the client.
    /// </summary>
    public string OriginalFileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the MIME content type supplied by the client.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Gets the requested number of minutes before the presigned upload URL expires.
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 15;
}

/// <summary>
/// Validates presigned upload URL requests before storage access is granted.
/// </summary>
public sealed class CreatePresignedUploadUrlValidator : AbstractValidator<CreatePresignedUploadUrl>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePresignedUploadUrlValidator"/> class.
    /// </summary>
    public CreatePresignedUploadUrlValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(100).WithMessage("Entity type must not exceed 100 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required.");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("Original file name is required.")
            .MaximumLength(255).WithMessage("Original file name must not exceed 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .MaximumLength(255).WithMessage("Content type must not exceed 255 characters.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero.");

        RuleFor(x => x.ExpiresInMinutes)
            .InclusiveBetween(1, 60).WithMessage("Expiration must be between 1 and 60 minutes.");
    }
}