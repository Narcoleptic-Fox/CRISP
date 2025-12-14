using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CRISP.Client.Common
{
    public interface IModule
    {
        WebAssemblyHostBuilder AddModule(WebAssemblyHostBuilder builder);
    }
}
