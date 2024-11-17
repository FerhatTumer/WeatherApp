using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherInfo?> GetWeatherAsync(string cityName);
    Task<List<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames);
}
