using CRISP.Core.Identity;
using CRISP.ServiceDefaults;
using CRISP.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Builder;

namespace CRISP.ServiceDefaults;
public static class IAuthorizationExtensions
{
    public static TBuilder RequireAuthorization<TBuilder>(this TBuilder builder, params Permissions[] permissions) where TBuilder : IEndpointConventionBuilder
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        ArgumentNullException.ThrowIfNull(permissions);
        return builder.RequireAuthorization(permissions.Select(n => new HasPermissionAttribute(n)).ToArray());
    }
}
