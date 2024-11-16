public class WeatherStackResponse
{
    public CurrentWeatherStack Current { get; set; } = new();
}

public class CurrentWeatherStack
{
    public double Temperature { get; set; }
}
