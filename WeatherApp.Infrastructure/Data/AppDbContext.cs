using Microsoft.EntityFrameworkCore;
using WeatherApp.Domain.Entities;
using WeatherApp.Domain.Events;

namespace WeatherApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<FavoriteCity> FavoriteCities { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<WeatherApp.Domain.Events.DomainEvent>();
        modelBuilder.Ignore<WeatherApp.Domain.Events.FavoriteCityAddedEvent>();
        modelBuilder.Entity<FavoriteCity>().HasKey(f => f.Id);
        modelBuilder.Entity<FavoriteCity>().Property(f => f.CityName).IsRequired().HasMaxLength(100);
    }
}
