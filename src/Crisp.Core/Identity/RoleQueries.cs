namespace CRISP.Core.Identity;

public sealed record GetRoleByName : IQuery<Role>
{
    public GetRoleByName()
    {
    }
    public GetRoleByName(string name)
    {
        Name = name;
    }
    public string Name { get; init; } = string.Empty;
}

public sealed record GetRoles : PagedQuery<Roles>
{
    public IEnumerable<string>? Names { get; set; }
    public IEnumerable<Permissions>? Permissions { get; set; }
}