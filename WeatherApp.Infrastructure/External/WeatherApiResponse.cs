using System.Text.Json.Serialization;

namespace WeatherApp.Infrastructure.External;

public class WeatherApiResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temp_c")]
    public float TempC { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("condition")]
    public WeatherCondition Condition { get; set; }
}

public class WeatherCondition
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
