using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Infrastructure.Clients;

public class WeatherStackClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherStackClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherInfo?> FetchWeatherAsync(string cityName)
    {
        var response = await _httpClient.GetAsync($"http://api.weatherstack.com/current?access_key=838c0d5e8fcc1dbbc66e8c1c0a14c6e5&query={cityName}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var weatherData = JsonSerializer.Deserialize<WeatherStackResponse>(content);

        return weatherData != null
            ? new WeatherInfo
            {
                City = cityName,
                Temperature = weatherData.Current.Temperature,
                Condition = "WeatherStack Condition"
            }
            : null;
    }
}
