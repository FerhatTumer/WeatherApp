using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Repositories;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Domain.Entities;
using StackExchange.Redis;
using WeatherApp.Application.Services;

namespace WeatherApp.Infrastructure.Services;

public class FavoriteCityService : IFavoriteCityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IWeatherApiClient> _clients;

    public FavoriteCityService(IUnitOfWork unitOfWork, IEnumerable<IWeatherApiClient> clients)
    {
        _unitOfWork = unitOfWork;
        _clients = clients;
    }

    public async Task<IEnumerable<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        var weatherInfos = new List<WeatherInfo>();

        foreach (var cityName in cityNames)
        {
            var weatherInfo = await GetWeatherInfoAsync(cityName);
            if (weatherInfo != null)
            {
                weatherInfos.Add(weatherInfo);
            }
        }

        return weatherInfos;
    }

    public async Task<FavoriteCitySummary> GetFavoriteCitySummaryAsync()
    {
        var favoriteCities = await _unitOfWork.Repository<FavoriteCity>().GetAllAsync();
        var cityNames = favoriteCities.Select(c => c.CityName);

        var weatherInfos = await GetWeatherForCitiesAsync(cityNames);

        var hottestCity = weatherInfos.OrderByDescending(w => w.Temperature).FirstOrDefault();
        var coldestCity = weatherInfos.OrderBy(w => w.Temperature).FirstOrDefault();
        var averageTemperature = weatherInfos.Any() ? weatherInfos.Average(w => w.Temperature) : 0;

        return new FavoriteCitySummary
        {
            HottestCity = hottestCity,
            ColdestCity = coldestCity,
            AverageTemperature = averageTemperature
        };
    }

    private async Task<WeatherInfo?> GetWeatherInfoAsync(string cityName)
    {
        WeatherInfo? weatherInfo = null;
        foreach (var client in _clients)
        {
            try
            {
                weatherInfo = await client.FetchWeatherAsync(cityName);
                if (weatherInfo != null)
                {
                    break;
                }
            }
            catch
            {
            }
        }

        if (weatherInfo == null) return null;

        return weatherInfo;
    }
}
