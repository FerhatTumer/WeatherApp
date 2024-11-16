using WeatherApp.Domain.Events;

namespace WeatherApp.Domain.Entities;

public interface IEntityWithDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
