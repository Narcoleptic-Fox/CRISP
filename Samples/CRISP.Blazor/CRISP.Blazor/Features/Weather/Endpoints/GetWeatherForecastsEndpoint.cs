using CRISP.Blazor.Client.Modules.Weather.Models;
using CRISP.Blazor.Client.Modules.Weather.Requests;

namespace CRISP.Blazor.Features.Weather.Endpoints
{
    public class GetWeatherForecastsEndpoint :
        QueryEndpointBase<GetWeatherForecasts, PagedResult<WeatherForecast>>,
        IFilteredQueryEndpoint<GetWeatherForecasts, WeatherFilter, WeatherForecast>
    {
        public static void Map(RouteGroupBuilder builder) =>
            builder.MapGet("", Handle)
                   .Produces<Response<PagedResult<WeatherForecast>>>();
    }

    public sealed class GetWeatherForecastsService : IFilteredQueryService<GetWeatherForecasts, WeatherFilter, WeatherForecast>
    {
        public async ValueTask<Response<PagedResult<WeatherForecast>>> Send(GetWeatherForecasts query, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            return Response<PagedResult<WeatherForecast>>.Success(new PagedResult<WeatherForecast>
            {
                Items =
                [
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        TemperatureC = 20,
                        Summary = "Sunny"
                    }
                ],
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            });
        }
    }
}
