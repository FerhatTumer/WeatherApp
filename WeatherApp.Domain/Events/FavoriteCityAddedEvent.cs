using WeatherApp.Domain.Entities;

namespace WeatherApp.Domain.Events;

public class FavoriteCityAddedEvent : DomainEvent
{
    public FavoriteCity FavoriteCity { get; }

    public FavoriteCityAddedEvent(FavoriteCity favoriteCity)
    {
        FavoriteCity = favoriteCity;
    }
}
