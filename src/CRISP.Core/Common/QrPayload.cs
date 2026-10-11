namespace CRISP.Core.Common;

[ExportTsInterface]
public abstract record QrPayload
{
    public int SchemaVersion { get; init; } = 1;
    public string EntityType { get; init; } = default!;
    public Guid Id { get; init; }
}
