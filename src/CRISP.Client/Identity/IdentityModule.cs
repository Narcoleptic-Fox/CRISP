using CRISP.Client.Common;
using CRISP.Client.Identity.Services;
using CRISP.Core.Identity;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CRISP.Client.Identity;

public sealed class IdentityModule : IModule
{
    public WebAssemblyHostBuilder AddModule(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddHttpClient<IUserService, UserService>(client =>
                                       client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}/api/{nameof(Identity)}/{nameof(Users)}"))
                        .AddHttpMessageHandler<CookieHandler>();
        builder.Services.AddHttpClient<IRoleService, RoleService>(client =>
                                       client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}/api/{nameof(Identity)}/{nameof(Roles)}"))
                        .AddHttpMessageHandler<CookieHandler>();
        return builder;
    }
}
