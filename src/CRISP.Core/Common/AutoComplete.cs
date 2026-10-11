namespace CRISP.Core.Common;

[ExportTsInterface]
public sealed record AutoComplete : BaseModel
{
    public string Name { get; init; } = default!;
}
