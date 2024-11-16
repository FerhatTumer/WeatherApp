using System.Diagnostics;
using WeatherApp.Domain.Events;

namespace WeatherApp.Application.Handlers;

public class FavoriteCityAddedEventHandler : IEventHandler<FavoriteCityAddedEvent>
{
    public Task Handle(FavoriteCityAddedEvent domainEvent)
    {
        Debug.WriteLine($"Favorite city added: {domainEvent.FavoriteCity.CityName}");
        return Task.CompletedTask;
    }
}
