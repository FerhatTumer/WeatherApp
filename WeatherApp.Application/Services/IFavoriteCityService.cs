using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services;

public interface IFavoriteCityService
{
    Task<List<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames);
}
