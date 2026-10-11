using CRISP.Server.Data.ValueObjects;

namespace CRISP.Server.Data.Entities;

public interface IHasStoredFiles
{
    List<StoredFileObject> Files { get; set; }
}
