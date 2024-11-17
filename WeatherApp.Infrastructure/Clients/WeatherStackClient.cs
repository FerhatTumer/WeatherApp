using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Infrastructure.External.WeatherStack;

public class WeatherStackClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherStackClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, WeatherInfo?>> FetchWeatherAsync(IEnumerable<string> cityNames)
    {
        var cities = string.Join(",", cityNames);
        var response = await _httpClient.GetAsync($"http://api.weatherstack.com/current?access_key=838c0d5e8fcc1dbbc66e8c1c0a14c6e5&query={cities}");

        if (!response.IsSuccessStatusCode)
        {
            return cityNames.ToDictionary(city => city, _ => (WeatherInfo?)null);
        }

        var content = await response.Content.ReadAsStringAsync();

        var weatherStackResponse = JsonSerializer.Deserialize<WeatherStackResponse>(content);

        if (weatherStackResponse == null || weatherStackResponse.Current == null)
        {
            return cityNames.ToDictionary(city => city, _ => (WeatherInfo?)null);
        }

        return new Dictionary<string, WeatherInfo?>
        {
            {
                weatherStackResponse.Location.Name,
                new WeatherInfo
                {
                    City = weatherStackResponse.Location.Name,
                    Temperature = weatherStackResponse.Current.Temperature,
                    Condition = weatherStackResponse.Current.WeatherDescriptions.FirstOrDefault() ?? "N/A"
                }
            }
        };
    }
}
