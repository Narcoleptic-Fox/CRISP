namespace CRISP.Core.Audit;

[ExportTsInterface]
public sealed record GetMessages : IQuery<IEnumerable<SystemMessage>>
{
    public string EntityType { get; init; } = default!;
    public Guid EntityId { get; init; }
}
