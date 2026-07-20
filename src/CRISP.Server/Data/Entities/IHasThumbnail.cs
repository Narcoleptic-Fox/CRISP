using CRISP.Server.Data.ValueObjects;

namespace CRISP.Server.Data.Entities;

/// <summary>
/// Marks an entity that carries a single thumbnail image alongside its attached files.
/// The thumbnail is stored separately from <see cref="IHasStoredFiles.Files"/> but is
/// resolved through the same presigned-download flow.
/// </summary>
public interface IHasThumbnail
{
    StoredFileObject? Thumbnail { get; set; }
}
