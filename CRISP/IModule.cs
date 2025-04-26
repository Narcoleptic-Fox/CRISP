using Microsoft.Extensions.DependencyInjection;

namespace CRISP
{
    public interface IModule
    {
        void RegisterModule(IServiceCollection services);
    }
}
