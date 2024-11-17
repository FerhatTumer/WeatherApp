public class WeatherApiResponse
{
    public WeatherApiLocation Location { get; set; }
    public CurrentWeather Current { get; set; }
}

public class WeatherApiLocation
{
    public string Name { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string TzId { get; set; }
    public string Localtime { get; set; }
}


public class CurrentWeather
{
    public double TempC { get; set; }
    public double TempF { get; set; }
    public Condition Condition { get; set; }
    public double WindMph { get; set; }
    public double WindKph { get; set; }
    public int Humidity { get; set; }
    public int Cloud { get; set; }
    public double FeelslikeC { get; set; }
    public double FeelslikeF { get; set; }
}

public class Condition
{
    public string Text { get; set; }
    public string Icon { get; set; }
    public int Code { get; set; }
}
