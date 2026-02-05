using CRISP.Core.Identity;
using CRISP.Server.Features.Identity.Endpoints;
using CRISP.ServiceDefaults.Features;

namespace CRISP.Server.Features.Identity;

public class IdentityFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // Register User Services
        services.AddScoped<IQueryService<GetUserByEmail, User>, GetUserByEmailService>();
        services.AddScoped<IQueryService<SingularQuery<User>, User>, GetUserService>();
        services.AddScoped<IQueryService<GetUsers, PagedResponse<Users>>, GetUsersService>();
        services.AddScoped<IModifyService<UpdateUser>, UpdateUserService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteUserService>(nameof(Users));

        // Register Role Services
        services.AddScoped<ICreateService<CreateRole>, CreateRoleService>();
        services.AddScoped<IQueryService<SingularQuery<Role>, Role>, GetRoleByIdService>();
        services.AddScoped<IQueryService<GetRoleByName, Role>, GetRoleByNameService>();
        services.AddScoped<IQueryService<GetRoles, PagedResponse<Roles>>, GetRolesService>();
        services.AddScoped<IModifyService<UpdateRole>, UpdateRoleService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteRoleService>(nameof(Roles));

        return services;
    }

    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder identityGroup = app.MapGroup("/identity")
                                             .WithTags(nameof(Identity));

        // Register User Endpoints
        RouteGroupBuilder group = identityGroup.MapGroup("/users")
                                               .WithTags(nameof(Users))
                                               .ProducesProblem(StatusCodes.Status401Unauthorized)
                                               .RequireAuthorization();

        GetUserByEmailEndpoint.MapEndpoint(group);
        GetUserByIdEndpoint.MapEndpoint(group);
        GetUsersEndpoint.MapEndpoint(group);
        UpdateUserEndpoint.MapEndpoint(group);
        DeleteUserEndpoint.MapEndpoint(group);

        // Register Role Endpoints
        group = identityGroup.MapGroup("/roles")
                             .WithTags(nameof(Roles))
                             .ProducesProblem(StatusCodes.Status401Unauthorized)
                             .RequireAuthorization();

        CreateRoleEndpoint.MapEndpoint(group);
        GetRoleByIdEndpoint.MapEndpoint(group);
        GetRoleByNameEndpoint.MapEndpoint(group);
        GetRolesEndpoint.MapEndpoint(group);
        UpdateRoleEndpoint.MapEndpoint(group);
        DeleteRoleEndpoint.MapEndpoint(group);

        return app;
    }
}
