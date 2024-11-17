using System.Text.Json;
using StackExchange.Redis;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;

namespace WeatherApp.Infrastructure.Services;

public class RedisCacheWeatherService : IWeatherService
{
    private readonly IWeatherService _innerWeatherService;
    private readonly IDatabase _redisCache;

    public RedisCacheWeatherService(IWeatherService innerWeatherService, IConnectionMultiplexer redis)
    {
        _innerWeatherService = innerWeatherService;
        _redisCache = redis.GetDatabase();
    }

    public async Task<WeatherInfo?> GetWeatherAsync(string cityName)
    {
        var cacheKey = cityName.ToLower();

        var cachedData = await _redisCache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<WeatherInfo>(cachedData);
        }

        var weatherInfo = await _innerWeatherService.GetWeatherAsync(cityName);

        if (weatherInfo != null)
        {
            await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(weatherInfo), TimeSpan.FromMinutes(10));
        }

        return weatherInfo;
    }

    public async Task<List<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        var tasks = cityNames.Select(async city =>
        {
            var cacheKey = city.ToLower();
            var cachedData = await _redisCache.StringGetAsync(cacheKey);

            if (!cachedData.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<WeatherInfo>(cachedData);
            }

            var weather = await _innerWeatherService.GetWeatherAsync(city);
            if (weather != null)
            {
                await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(weather), TimeSpan.FromMinutes(10));
            }
            return weather;
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList();
    }
}
