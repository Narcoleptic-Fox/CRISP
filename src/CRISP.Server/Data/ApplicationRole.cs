using CRISP.Core.Identity;
using CRISP.ServiceDefaults.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace CRISP.Server.Data;

public class ApplicationRole : IdentityRole<Guid>, ISoftDelete
{
    public string Permission { get; set; } = string.Empty;
    public IEnumerable<Permissions> PermissionsList
    {
        get =>
        Permission
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => Enum.TryParse(p.Trim(), out Permissions permission) ? permission : Permissions.None)
        .Distinct();

        set
        {
            Permission = string.Join(',', value.Select(p => p.ToString()));
        }
    }
    public bool IsDeleted { get; set; }
}
