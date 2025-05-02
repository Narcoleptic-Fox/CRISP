using CRISP.Blazor.Client.Modules.Weather.Models;

namespace CRISP.Blazor.Client.Modules.Weather.Requests
{
    public sealed record GetWeatherForecasts : FilteredQuery<WeatherFilter, WeatherForecast>;

    public sealed class WeatherFilter : DateRangeFilter
    {

    }
}
