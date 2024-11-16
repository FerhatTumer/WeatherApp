using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WeatherApp.Domain.Events;

namespace WeatherApp.Infrastructure.Services;

public class DomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Dispatch(IEnumerable<DomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("Handle");
                if (handleMethod == null)
                    throw new InvalidOperationException($"Handler {handlerType.Name} does not contain a 'Handle' method.");

                await (Task)handleMethod.Invoke(handler, new[] { domainEvent });
            }
        }
    }
}
