using System.Threading.Tasks;
using Moq;
using WeatherApp.Application.Services;
using Xunit;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Application.DTOs;

namespace WeatherApp.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherApiClient> _mockApiClient1;
    private readonly Mock<IWeatherApiClient> _mockApiClient2;
    private readonly WeatherService _weatherService;
    private readonly Mock<StackExchange.Redis.IDatabase> _mockRedis;

    public WeatherServiceTests()
    {
        _mockApiClient1 = new Mock<IWeatherApiClient>();
        _mockApiClient2 = new Mock<IWeatherApiClient>();

        var clients = new List<IWeatherApiClient> { _mockApiClient1.Object, _mockApiClient2.Object };

        _mockRedis = new Mock<StackExchange.Redis.IDatabase>();
        var mockConnection = new Mock<StackExchange.Redis.IConnectionMultiplexer>();
        mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedis.Object);

        _weatherService = new WeatherService(clients, mockConnection.Object);
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldReturnAverageTemperature_WhenBothApisReturnData()
    {
        // Arrange
        var cityName = "Istanbul";
        var weatherData = new Dictionary<string, WeatherInfo?>
        {
            { cityName, new WeatherInfo { City = cityName, Temperature = 20 } },
            { cityName, new WeatherInfo { City = cityName, Temperature = 30 } }
        };

        _mockApiClient1.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(weatherData);

        _mockApiClient2.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(weatherData);

        // Act
        var result = await _weatherService.GetWeatherAsync(cityName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityName, result.City);
        Assert.Equal(25, result.Temperature); // (20 + 30) / 2
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldReturnSingleApiResult_WhenOneApiFails()
    {
        // Arrange
        var cityName = "Istanbul";
        var weatherData = new Dictionary<string, WeatherInfo?>
        {
            { cityName, new WeatherInfo { City = cityName, Temperature = 20 } }
        };

        _mockApiClient1.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(weatherData);

        _mockApiClient2.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new Exception("API Failure"));

        // Act
        var result = await _weatherService.GetWeatherAsync(cityName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityName, result.City);
        Assert.Equal(20, result.Temperature); // Only first API's result
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldReturnCacheResult_WhenAllApisFail()
    {
        // Arrange
        var cityName = "Istanbul";
        var cachedData = new WeatherInfo { City = cityName, Temperature = 15 };

        _mockApiClient1.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new Exception("API Failure"));

        _mockApiClient2.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new Exception("API Failure"));

        _mockRedis.Setup(r => r.StringGetAsync(cityName.ToLower(), default))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(cachedData));

        // Act
        var result = await _weatherService.GetWeatherAsync(cityName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityName, result.City);
        Assert.Equal(15, result.Temperature); // Cached result
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldHandleMultipleCities()
    {
        // Arrange
        var cityNames = new[] { "Istanbul", "Ankara" };
        var weatherData = new Dictionary<string, WeatherInfo?>
        {
            { "Istanbul", new WeatherInfo { City = "Istanbul", Temperature = 20 } },
            { "Ankara", new WeatherInfo { City = "Ankara", Temperature = 15 } }
        };

        _mockApiClient1.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(weatherData);

        _mockApiClient2.Setup(c => c.FetchWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(weatherData);

        // Act
        var istanbulResult = await _weatherService.GetWeatherAsync("Istanbul");
        var ankaraResult = await _weatherService.GetWeatherAsync("Ankara");

        // Assert
        Assert.NotNull(istanbulResult);
        Assert.Equal("Istanbul", istanbulResult.City);
        Assert.Equal(20, istanbulResult.Temperature);

        Assert.NotNull(ankaraResult);
        Assert.Equal("Ankara", ankaraResult.City);
        Assert.Equal(15, ankaraResult.Temperature);
    }
}
