
using CRISP.Blazor.Client.Modules.Weather.Services;

namespace CRISP.Blazor.Client.Modules.Weather
{
    public class WeatherModule : IModule
    {
        public string ModuleName => nameof(Weather);

        public void RegisterServices(IServiceCollection services) => services.AddScoped<IWeatherService, WeatherService>();
    }
}
