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
    private readonly ConcurrentDictionary<string, WeatherRequestGroup> _pendingRequests = new();

    public WeatherService(IEnumerable<IWeatherApiClient> clients, IConnectionMultiplexer redis)
    {
        _clients = clients;
        _redisCache = redis.GetDatabase();
    }

    public async Task<WeatherInfo?> GetWeatherAsync(string cityName)
    {
        var cacheKey = cityName.ToLower();

        var requestGroup = _pendingRequests.GetOrAdd(cacheKey, _ => new WeatherRequestGroup());
        var tcs = new TaskCompletionSource<WeatherInfo>();
        requestGroup.AddRequest(tcs);

        if (!requestGroup.IsProcessing)
        {
            requestGroup.IsProcessing = true;
            _ = ProcessWeatherRequestsAsync(cityName, requestGroup);
        }

        return await tcs.Task;
    }

    private async Task ProcessWeatherRequestsAsync(string cityName, WeatherRequestGroup requestGroup)
    {
        await Task.Delay(5000);

        try
        {
            var weatherInfo = await FetchWeatherFromApisAsync(cityName);

            if (weatherInfo != null)
            {
                var cacheKey = cityName.ToLower();
                await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(weatherInfo), TimeSpan.FromMinutes(10));
            }

            requestGroup.CompleteAllRequests(weatherInfo);
        }
        catch (Exception ex)
        {
            requestGroup.FailAllRequests(ex);
        }
        finally
        {
            _pendingRequests.TryRemove(cityName.ToLower(), out _);
        }
    }

    private async Task<WeatherInfo?> FetchWeatherFromApisAsync(string cityName)
    {
        var responses = new List<WeatherInfo>();
        Exception? lastException = null;

        foreach (var client in _clients)
        {
            try
            {
                var weatherInfo = await client.FetchWeatherAsync(cityName);
                if (weatherInfo != null)
                {
                    responses.Add(weatherInfo);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("FetchWeatherAsync Error", cityName);
                lastException = ex;
            }
        }

        if (responses.Any())
        {
            var averageTemperature = responses.Average(r => r.Temperature);
            return new WeatherInfo
            {
                City = cityName,
                Temperature = averageTemperature,
                Condition = "Average from APIs"
            };
        }

        var cacheKey = cityName.ToLower();
        var cachedData = await _redisCache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<WeatherInfo>(cachedData);
        }

        if (lastException != null)
        {
            throw lastException;
        }

        Log.Warning("FetchWeatherAsync Empty response", cityName);

        return null;
    }
}

public class WeatherRequestGroup
{
    private readonly List<TaskCompletionSource<WeatherInfo>> _requests = new();

    public bool IsProcessing { get; set; }

    public void AddRequest(TaskCompletionSource<WeatherInfo> tcs)
    {
        lock (_requests)
        {
            _requests.Add(tcs);
        }
    }

    public void CompleteAllRequests(WeatherInfo? weatherInfo)
    {
        lock (_requests)
        {
            foreach (var tcs in _requests)
            {
                tcs.SetResult(weatherInfo!);
            }
            _requests.Clear();
        }
    }

    public void FailAllRequests(Exception ex)
    {
        lock (_requests)
        {
            foreach (var tcs in _requests)
            {
                tcs.SetException(ex);
            }
            _requests.Clear();
        }
    }
}
