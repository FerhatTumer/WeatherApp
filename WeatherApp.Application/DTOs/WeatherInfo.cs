namespace WeatherApp.Application.DTOs;

public class WeatherInfo
{
    public string City { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
}
