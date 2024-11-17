using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services.Interfaces;

public interface IWeatherApiClient
{
    Task<Dictionary<string, WeatherInfo?>> FetchWeatherAsync(IEnumerable<string> cityNames);
}
