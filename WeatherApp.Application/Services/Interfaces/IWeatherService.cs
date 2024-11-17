using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services.Interfaces;

public interface IWeatherService
{
    Task<Dictionary<string, WeatherInfo?>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames);
}
