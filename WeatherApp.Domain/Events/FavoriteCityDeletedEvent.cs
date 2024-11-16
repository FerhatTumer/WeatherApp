using WeatherApp.Domain.Entities;

namespace WeatherApp.Domain.Events;

public class FavoriteCityDeletedEvent : DomainEvent
{
    public FavoriteCity FavoriteCity { get; }

    public FavoriteCityDeletedEvent(FavoriteCity favoriteCity)
    {
        FavoriteCity = favoriteCity;
    }
}
