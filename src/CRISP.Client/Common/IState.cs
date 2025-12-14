using System.Text.Json.Serialization;

namespace CRISP.Client.Common;

public interface IState
{
    [JsonIgnore]
    static abstract bool LocalStorage { get; }

    [JsonIgnore]
    static abstract string StorageKey { get; }
}

public abstract class BaseState
{
    public IEnumerable<Guid>? Ids { get; set; }
}
