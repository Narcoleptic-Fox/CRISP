namespace CRISP.Core.Identity;
public record Roles : BaseModel
{
    public string Name { get; init; } = string.Empty;
    public IEnumerable<Permissions> Permissions { get; init; } = [];
}
