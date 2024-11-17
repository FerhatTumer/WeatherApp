using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;

namespace WeatherApp.Application.Services;

public class FavoriteCityService : IFavoriteCityService
{
    private readonly IWeatherService _weatherService;

    public FavoriteCityService(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<List<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        var weatherInfos = await _weatherService.GetWeatherForCitiesAsync(cityNames);
        return weatherInfos;
    }
}
