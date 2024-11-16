using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Domain.Entities;
using WeatherApp.Infrastructure.External;

namespace WeatherApp.Infrastructure.Clients;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherInfo?> FetchWeatherAsync(string cityName)
    {
        var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/current.json?key=147d644004414106a2f75650232001&q={cityName}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var weatherData = JsonSerializer.Deserialize<WeatherApiResponse>(content);

        return weatherData != null
            ? new WeatherInfo
            {
                City = cityName,
                Temperature = weatherData.Current.TempC,
                Condition = weatherData.Current.Condition.Text
            }
            : null;
    }
}
