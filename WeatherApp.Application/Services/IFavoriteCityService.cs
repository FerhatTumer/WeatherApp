using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services;

public interface IFavoriteCityService
{
    Task<FavoriteCitySummary> GetFavoriteCitySummaryAsync();
    Task<IEnumerable<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cities);
}
