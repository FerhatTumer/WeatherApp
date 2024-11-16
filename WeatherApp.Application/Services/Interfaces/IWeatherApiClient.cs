using WeatherApp.Application.DTOs;

namespace WeatherApp.Application.Services.Interfaces;

public interface IWeatherApiClient
{
    Task<WeatherInfo?> FetchWeatherAsync(string cityName);
    //Task<IEnumerable<WeatherInfo>> FetchWeatherForCitiesAsync(IEnumerable<string> cityNames);
}
