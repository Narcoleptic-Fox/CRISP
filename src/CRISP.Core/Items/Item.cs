using CRISP.Core.Storage;

namespace CRISP.Core.Items;

/// <summary>
/// Represents an item tracked by the application.
/// </summary>
/// <remarks>
/// Item is the template's worked example of a complete vertical slice: it is auditable,
/// soft-deletable, carries file attachments, and is exposed end to end (Core contract →
/// server feature → generated client types → React UI). Copy this slice when adding a new
/// domain concept and rename throughout.
/// </remarks>
[ExportTsInterface]
public sealed record Item : BaseAuditableModel
{
    /// <summary>
    /// Gets the item display name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the optional free-text description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the free-text owner name. Sourced from existing values via the owner-names autocomplete.
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is available for active use.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the files attached to the item that are safe to expose outside persistence.
    /// </summary>
    public IReadOnlyCollection<StoredFile> Files { get; init; } = [];
}
