using CRISP.Core.Identity;

namespace CRISP.Server.Authorization;

/// <summary>
/// Provides service registration helpers for API authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers role-based authorization policies used by the API.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.ReadAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Viewer.ToString(), Roles.Editor.ToString(), Roles.Admin.ToString());
            })
            .AddPolicy(Policies.WriteAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Editor.ToString(), Roles.Admin.ToString());
            })
            .AddPolicy(Policies.AdminAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Admin.ToString());
            });

        return services;
    }
}
