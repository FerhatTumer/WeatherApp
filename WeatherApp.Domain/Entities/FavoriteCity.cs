using System.ComponentModel.DataAnnotations.Schema;
using WeatherApp.Domain.Events;

namespace WeatherApp.Domain.Entities;

public class FavoriteCity : IEntityWithDomainEvents
{
    public Guid Id { get; private set; }
    public string CityName { get; private set; }

    [NotMapped]
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public FavoriteCity(string cityName)
    {
        Id = Guid.NewGuid();
        CityName = cityName;
        AddDomainEvent(new FavoriteCityAddedEvent(this));
    }

    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
