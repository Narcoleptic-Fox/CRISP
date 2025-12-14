using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.ServiceDefaults.Features
{
    public interface IFeature
    {
        IServiceCollection AddFeature(IServiceCollection services);
        IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app);
    }
}
