using System.Text.Json;
using WeatherApp.Application.DTOs;
using WeatherApp.Application.Repositories;
using WeatherApp.Application.Services;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Infrastructure.Services;

public class FavoriteCityService : IFavoriteCityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeatherService _weatherService;

    public FavoriteCityService(IUnitOfWork unitOfWork, IWeatherService weatherService)
    {
        _unitOfWork = unitOfWork;
        _weatherService = weatherService;
    }

    public async Task<IEnumerable<WeatherInfo>> GetWeatherForCitiesAsync(IEnumerable<string> cityNames)
    {
        // IWeatherService'in toplu şehir desteğini kullanarak optimize edilmiş hava durumu sorgusu
        var weatherData = await _weatherService.GetWeatherForCitiesAsync(cityNames);

        // Null olan veya hatalı verileri filtrele
        return weatherData
            .Where(kv => kv.Value != null)
            .Select(kv => kv.Value!)
            .ToList();
    }

    public async Task<FavoriteCitySummary> GetFavoriteCitySummaryAsync()
    {
        // Tüm favori şehirleri al
        var favoriteCities = await _unitOfWork.Repository<FavoriteCity>().GetAllAsync();
        var cityNames = favoriteCities.Select(c => c.CityName);

        // Favori şehirler için hava durumu bilgilerini al
        var weatherInfos = await GetWeatherForCitiesAsync(cityNames);

        // En sıcak ve en soğuk şehirleri ve ortalama sıcaklığı hesapla
        var hottestCity = weatherInfos.OrderByDescending(w => w.Temperature).FirstOrDefault();
        var coldestCity = weatherInfos.OrderBy(w => w.Temperature).FirstOrDefault();
        var averageTemperature = weatherInfos.Any() ? weatherInfos.Average(w => w.Temperature) : 0;

        return new FavoriteCitySummary
        {
            HottestCity = hottestCity,
            ColdestCity = coldestCity,
            AverageTemperature = averageTemperature
        };
    }

    //private async Task<WeatherInfo?> GetWeatherInfoAsync(string cityName)
    //{
    //    try
    //    {
    //        // Şehir için hava durumu bilgisi al
    //        return await _weatherService.GetWeatherAsync(cityName);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Hata durumunda null döndür
    //        return null;
    //    }
    //}
}
