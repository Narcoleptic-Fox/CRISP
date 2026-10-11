using CRISP.Core.Common;
using CRISP.Core.Identity;
using Asp.Versioning;
using Humanizer;
using CRISP.Server.Authorization;
using CRISP.Server.Common;
using CRISP.Server.Features;

namespace CRISP.Server.Features.Identity;

/// <summary>
/// Registers identity services and maps user endpoints.
/// </summary>
public class IdentityFeature : IFeature
{
    /// <summary>
    /// Registers user and user settings command/query handlers.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // User
        services.AddScoped<ICreateService<CreateUser>, CreateUserService>();
        services.AddScoped<IModifyService<UpdateUser>, UpdateUserService>();
        services.AddScoped<IQueryService<GetUser, User>, GetUserService>();
        services.AddScoped<IQueryService<GetUsers, PagedResponse<User>>, GetUsersService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteUserService>(nameof(Identity));
        services.AddScoped<IQueryService<GetUserNames, IEnumerable<AutoComplete>>, GetUserNamesService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // User Settings
        services.AddScoped<IModifyService<UpdateUserSettings>, UpdateUserSettingsService>();

        return services;
    }

    /// <summary>
    /// Maps versioned user and user settings endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder to configure.</param>
    /// <returns>The endpoint route builder.</returns>
    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        // Users
        var group = app.MapGroup(nameof(User).Pluralize())
            .WithTags(nameof(Identity));
        // v1
        var v1 = group.HasApiVersion(new ApiVersion(1, 0));

        // User management (CRUD) is admin-only. The name autocomplete is needed by the asset
        // owner picker, so it's a plain read. /me and /current must stay authenticated-only so an
        // unprovisioned (roleless) user can still discover they have no account (403 unprovisioned).
        var authenticated = v1.MapGroup("").RequireAuthorization();
        var read = v1.MapGroup("").RequireAuthorization(Policies.ReadAccess);
        var admin = v1.MapGroup("").RequireAuthorization(Policies.AdminAccess);
        CreateUserEndpoint.Map(admin);
        UpdateUserEndpoint.Map(admin);
        GetUserEndpoint.Map(admin);
        GetUsersEndpoint.Map(admin);
        GetCurrentEndpoint.Map(authenticated);
        DeleteUserEndpoint.Map(admin);
        GetUserNamesEndpoint.Map(read);

        // User Settings — any authenticated user manages their OWN settings (target resolved from
        // the token, not the request body; see UpdateUserSettingsService).
        group = app.MapGroup(nameof(UserSettings))
            .WithTags(nameof(Identity));
        // v1
        v1 = group
            .HasApiVersion(new ApiVersion(1, 0));
        UpdateUserSettingsEndpoint.Map(v1.RequireAuthorization());

        return app;
    }
}
