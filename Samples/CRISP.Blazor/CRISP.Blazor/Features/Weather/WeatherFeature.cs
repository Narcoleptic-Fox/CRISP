
using CRISP.Blazor.Client.Modules.Weather.Models;
using CRISP.Blazor.Client.Modules.Weather.Requests;
using CRISP.Blazor.Features.Weather.Endpoints;

namespace CRISP.Blazor.Features.Weather
{
    public class WeatherFeature : IFeature
    {
        public string ModuleName => nameof(Weather);

        public void RegisterServices(IServiceCollection services) =>
            services.AddScoped<IFilteredQueryService<GetWeatherForecasts, WeatherFilter, WeatherForecast>, GetWeatherForecastsService>();
        public void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup("api/weather");
            GetWeatherForecastsEndpoint.Map(group);
        }
    }
}
