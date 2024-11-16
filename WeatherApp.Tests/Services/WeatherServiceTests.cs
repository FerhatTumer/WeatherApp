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

    public WeatherServiceTests()
    {
        _mockApiClient1 = new Mock<IWeatherApiClient>();
        _mockApiClient2 = new Mock<IWeatherApiClient>();

        var clients = new List<IWeatherApiClient> { _mockApiClient1.Object, _mockApiClient2.Object };

        var mockRedis = new Mock<StackExchange.Redis.IDatabase>();
        var mockConnection = new Mock<StackExchange.Redis.IConnectionMultiplexer>();
        mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockRedis.Object);

        _weatherService = new WeatherService(clients, mockConnection.Object);
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldReturnAverageTemperature_WhenBothApisReturnData()
    {
        // Arrange
        var cityName = "Istanbul";
        _mockApiClient1.Setup(c => c.FetchWeatherAsync(cityName)).ReturnsAsync(new WeatherInfo { City = cityName, Temperature = 20 });
        _mockApiClient2.Setup(c => c.FetchWeatherAsync(cityName)).ReturnsAsync(new WeatherInfo { City = cityName, Temperature = 30 });

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
        _mockApiClient1.Setup(c => c.FetchWeatherAsync(cityName)).ReturnsAsync(new WeatherInfo { City = cityName, Temperature = 20 });
        _mockApiClient2.Setup(c => c.FetchWeatherAsync(cityName)).ThrowsAsync(new Exception("API Failure"));

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
        _mockApiClient1.Setup(c => c.FetchWeatherAsync(cityName)).ThrowsAsync(new Exception("API Failure"));
        _mockApiClient2.Setup(c => c.FetchWeatherAsync(cityName)).ThrowsAsync(new Exception("API Failure"));

        var mockRedis = new Mock<StackExchange.Redis.IDatabase>();
        var cachedData = new WeatherInfo { City = cityName, Temperature = 15 };
        mockRedis.Setup(r => r.StringGetAsync(cityName.ToLower(), default))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(cachedData));

        var mockConnection = new Mock<StackExchange.Redis.IConnectionMultiplexer>();
        mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockRedis.Object);

        var weatherServiceWithCache = new WeatherService(new List<IWeatherApiClient> { _mockApiClient1.Object, _mockApiClient2.Object }, mockConnection.Object);

        // Act
        var result = await weatherServiceWithCache.GetWeatherAsync(cityName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityName, result.City);
        Assert.Equal(15, result.Temperature); // Cached result
    }
}
