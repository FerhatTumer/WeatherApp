using System.Collections.Concurrent;
using System.Text.Json;
using Serilog;
using StackExchange.Redis;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;

namespace WeatherApp.Application.Services;

public class WeatherService : IWeatherService
{
    private readonly IEnumerable<IWeatherApiClient> _clients;
    private readonly IDatabase _redisCache;
    private readonly ConcurrentDictionary<string, List<TaskCompletionSource<WeatherInfo>>> _pendingRequests = new();
    private readonly ConcurrentDictionary<string, DateTime> _requestTimers = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RequestDelay = TimeSpan.FromSeconds(5);

    public WeatherService(IEnumerable<IWeatherApiClient> clients, IConnectionMultiplexer redis)
    {
        _clients = clients;
        _redisCache = redis.GetDatabase();
    }

    public async Task<Dictionary<string, WeatherInfo?>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        var cacheKeys = cityNames.Select(city => city.ToLower()).ToList();
        var results = new Dictionary<string, WeatherInfo?>();

        // Önce Redis'ten kontrol et
        foreach (var cacheKey in cacheKeys)
        {
            var cachedData = await _redisCache.StringGetAsync(cacheKey);
            if (!cachedData.IsNullOrEmpty)
            {
                Log.Information("Cache hit for city: {City}", cacheKey);
                results[cacheKey] = JsonSerializer.Deserialize<WeatherInfo>(cachedData);
            }
        }

        // Cache'te olmayan şehirler için API çağrısı
        var missingCities = cityNames.Except(results.Keys, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingCities.Any())
        {
            var fetchedData = await FetchWeatherDataFromApisAsync(missingCities);
            foreach (var (city, weatherInfo) in fetchedData)
            {
                if (weatherInfo != null)
                {
                    await _redisCache.StringSetAsync(city.ToLower(), JsonSerializer.Serialize(weatherInfo), CacheDuration);
                    results[city.ToLower()] = weatherInfo;
                }
            }
        }

        return results;
    }

    private async Task ProcessRequestsAsync(string cacheKey)
    {
        // 5 saniye bekleme
        await Task.Delay(RequestDelay);

        List<string> citiesToProcess;
        lock (_pendingRequests)
        {
            citiesToProcess = _pendingRequests.Keys.ToList();
        }

        var weatherData = await FetchWeatherDataFromApisAsync(citiesToProcess);

        lock (_pendingRequests)
        {
            foreach (var city in citiesToProcess)
            {
                if (_pendingRequests.TryRemove(city, out var tasks))
                {
                    var response = weatherData.TryGetValue(city, out var weatherInfo)
                        ? weatherInfo
                        : null;

                    foreach (var tcs in tasks)
                    {
                        if (response != null)
                            tcs.SetResult(response);
                        else
                            tcs.SetException(new Exception("Weather data unavailable."));
                    }
                }

                _requestTimers.TryRemove(city, out _);
            }
        }
    }

    private async Task<Dictionary<string, WeatherInfo?>> FetchWeatherDataFromApisAsync(IEnumerable<string> cities)
    {
        var results = new ConcurrentDictionary<string, List<WeatherInfo>>();

        foreach (var client in _clients)
        {
            try
            {
                var clientResults = await client.FetchWeatherAsync(cities);

                foreach (var (city, weatherInfo) in clientResults)
                {
                    if (weatherInfo != null)
                    {
                        results.AddOrUpdate(
                            city,
                            new List<WeatherInfo> { weatherInfo },
                            (_, existing) =>
                            {
                                existing.Add(weatherInfo);
                                return existing;
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "API client failed while fetching data for cities.");
            }
        }

        // Her şehir için ortalamaları hesapla
        var res =  results.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Any()
                ? new WeatherInfo
                {
                    City = pair.Key,
                    Temperature = pair.Value.Average(w => w.Temperature),
                    Condition = "Average from APIs"
                }
                : null);

        return res;
    }
}
