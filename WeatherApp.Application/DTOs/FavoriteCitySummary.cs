namespace WeatherApp.Application.DTOs;

public class FavoriteCitySummary
{
    public WeatherInfo? HottestCity { get; set; }
    public WeatherInfo? ColdestCity { get; set; }
    public double AverageTemperature { get; set; }
}
