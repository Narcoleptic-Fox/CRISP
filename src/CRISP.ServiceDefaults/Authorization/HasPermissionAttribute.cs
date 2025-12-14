using CRISP.Core.Identity;
using Microsoft.AspNetCore.Authorization;

namespace CRISP.ServiceDefaults.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HasPermissionAttribute"/> class with the specified permission.
    /// </summary>
    /// <param name="permission">The permission required to access the resource.</param>
    public HasPermissionAttribute(Permissions permission)
    {
        Policy = $"{permission}";
    }
}
