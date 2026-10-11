namespace CRISP.Core.Identity;

[ExportTsInterface]
public sealed record GetUserNames : IQuery<IEnumerable<AutoComplete>>
{
    public string? SearchTerm { get; init; }
}
