namespace WeatherApp.Infrastructure.External.WeatherStack
{
    public class WeatherStackResponse
    {
        public WeatherStackLocation Location { get; set; }
        public WeatherStackCurrent Current { get; set; }
    }

    public class WeatherStackLocation
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string TimezoneId { get; set; }
        public string Localtime { get; set; }
    }

    public class WeatherStackCurrent
    {
        public double Temperature { get; set; }
        public List<string> WeatherDescriptions { get; set; }
        public int WindSpeed { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
    }
}
