using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Services.Interfaces;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, WeatherInfo?>> FetchWeatherAsync(IEnumerable<string> cityNames)
    {
        var cities = string.Join(",", cityNames);
        var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/current.json?key=147d644004414106a2f75650232001&q={cities}");

        if (!response.IsSuccessStatusCode)
        {
            return cityNames.ToDictionary(city => city, _ => (WeatherInfo?)null);
        }

        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true // Büyük/küçük harf duyarlılığını kapatır
        };

        var weatherApiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(content, options);

        if (weatherApiResponse == null)
        {
            return cityNames.ToDictionary(city => city, _ => (WeatherInfo?)null);
        }

        return new Dictionary<string, WeatherInfo?>
        {
            {
                weatherApiResponse.Location.Name,
                new WeatherInfo
                {
                    City = weatherApiResponse.Location.Name,
                    Temperature = weatherApiResponse.Current.TempC,
                    Condition = weatherApiResponse.Current.Condition.Text
                }
            }
        };
    }
}
