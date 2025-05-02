using CRISP.Blazor.Client.Modules.Weather.Models;
using CRISP.Blazor.Client.Modules.Weather.Requests;
using System.Net.Http.Json;

namespace CRISP.Blazor.Client.Modules.Weather.Services
{
    public interface IWeatherService :
        IFilteredQueryService<GetWeatherForecasts, WeatherFilter, WeatherForecast>
    {

    }
    public class WeatherService(HttpClient httpClient) : IWeatherService
    {
        public async ValueTask<Response<PagedResult<WeatherForecast>>> Send(GetWeatherForecasts query, CancellationToken cancellationToken = default)
        {
            Response<PagedResult<WeatherForecast>>? response = await httpClient.GetFromJsonAsync<Response<PagedResult<WeatherForecast>>>($"api/weather?{query.Filter}");
            return response ?? Response<PagedResult<WeatherForecast>>.Failure("Failed to fetch weather data", new[] { "No response from server" });
        }
    }
}
