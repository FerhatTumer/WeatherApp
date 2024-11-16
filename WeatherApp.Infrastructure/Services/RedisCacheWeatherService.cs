using StackExchange.Redis;
using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Domain.Entities;

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

    public async Task<WeatherInfo> GetWeatherAsync(string cityName)
    {
        var cacheKey = cityName.ToLower();

        // Redis Cache kontrolü
        var cachedData = await _redisCache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<WeatherInfo>(cachedData)!;
        }

        // API'den veri çekme
        var weatherInfo = await _innerWeatherService.GetWeatherAsync(cityName);

        // Redis'e kaydet
        await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(weatherInfo), TimeSpan.FromMinutes(10));

        return weatherInfo;
    }
}
