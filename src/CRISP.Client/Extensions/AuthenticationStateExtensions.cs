using CRISP.Core.Constants;
using CRISP.Core.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CRISP.Client.Extensions;

public static class AuthenticationStateExtensions
{
    public static bool HasPermission(this AuthenticationState? authenticationState, Permissions permission)
    {
        if (authenticationState is null ||
           authenticationState.User.Identity is not { IsAuthenticated: true })
            return false;

        Claim? permissionClaim = authenticationState.User.Claims.SingleOrDefault(x => x.Type == ClaimNames.Permissions);
        return permissionClaim?.Value.UnpackPermissionsFromString().ContainsPermission(permission) == true;
    }
    public static bool HasPermissions(this AuthenticationState? authenticationState, params Permissions[] permissions) =>
        permissions.All(p => authenticationState.HasPermission(p));
}
