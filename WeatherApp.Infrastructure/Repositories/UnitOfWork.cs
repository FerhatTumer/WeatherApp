using WeatherApp.Application.Repositories;
using WeatherApp.Domain.Entities;
using WeatherApp.Domain.Events;
using WeatherApp.Infrastructure.Data;
using WeatherApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace WeatherApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly DomainEventDispatcher _dispatcher;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context, DomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (!_repositories.ContainsKey(typeof(T)))
        {
            var repository = new GenericRepository<T>(_context);
            _repositories.Add(typeof(T), repository);
        }

        return (IGenericRepository<T>)_repositories[typeof(T)];
    }

    public async Task<int> SaveChangesAsync()
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IEntityWithDomainEvents entity && entity.DomainEvents.Any())
            .Select(e => (IEntityWithDomainEvents)e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .Distinct()
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await _dispatcher.Dispatch(new[] { domainEvent });
        }

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
