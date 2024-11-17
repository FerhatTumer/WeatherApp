using StackExchange.Redis;
using System.Text.Json;
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

    //public async Task<WeatherInfo?> GetWeatherAsync(string cityName)
    //{
    //    var cacheKey = cityName.ToLower();

    //    // Redis Cache kontrolü
    //    var cachedData = await _redisCache.StringGetAsync(cacheKey);
    //    if (!cachedData.IsNullOrEmpty)
    //    {
    //        return JsonSerializer.Deserialize<WeatherInfo>(cachedData);
    //    }

    //    // API'den veri çekme
    //    var weatherInfo = await _innerWeatherService.GetWeatherAsync(cityName);

    //    if (weatherInfo != null)
    //    {
    //        // Redis'e kaydet
    //        await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(weatherInfo), TimeSpan.FromMinutes(10));
    //    }

    //    return weatherInfo;
    //}

    public async Task<Dictionary<string, WeatherInfo?>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        var cacheKeys = cityNames.Select(city => city.ToLower()).ToList();
        var results = new Dictionary<string, WeatherInfo?>();

        // Redis'ten kontrol et
        foreach (var cacheKey in cacheKeys)
        {
            var cachedData = await _redisCache.StringGetAsync(cacheKey);
            if (!cachedData.IsNullOrEmpty)
            {
                results[cacheKey] = JsonSerializer.Deserialize<WeatherInfo>(cachedData);
            }
        }

        // Cache'te olmayan şehirler için API'den veri çekme
        var missingCities = cityNames.Except(results.Keys, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingCities.Any())
        {
            var fetchedData = await _innerWeatherService.GetWeatherForCitiesAsync(missingCities);
            foreach (var (city, weatherInfo) in fetchedData)
            {
                if (weatherInfo != null)
                {
                    // Redis'e kaydet
                    await _redisCache.StringSetAsync(city.ToLower(), JsonSerializer.Serialize(weatherInfo), TimeSpan.FromMinutes(10));
                    results[city.ToLower()] = weatherInfo;
                }
            }
        }

        return results;
    }
}
