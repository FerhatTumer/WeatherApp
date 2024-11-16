using StackExchange.Redis;
using WeatherApp.Application.Handlers;
using WeatherApp.Application.Repositories;
using WeatherApp.Application.Services.Interfaces;
using WeatherApp.Data;
using WeatherApp.Infrastructure.Clients;
using WeatherApp.Infrastructure.Data;
using WeatherApp.Infrastructure.Repositories;
using WeatherApp.Application.Validators;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using System;
using FluentValidation.AspNetCore;
using WeatherApp.Application.Services;
using WeatherApp.Infrastructure.Services;
using WeatherApp.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>();
builder.Services.AddHttpClient<IWeatherApiClient, WeatherStackClient>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddScoped<IWeatherService, WeatherService>();
//builder.Services.Decorate<IWeatherService, RedisCacheWeatherService>();
builder.Services.AddScoped<IFavoriteCityService, FavoriteCityService>();



builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("WeatherApp"));
});

builder.Services.AddScoped<DomainEventDispatcher>();
builder.Services.AddTransient<IEventHandler<FavoriteCityAddedEvent>, FavoriteCityAddedEventHandler>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(AddFavoriteCityHandler).Assembly));


// Fluent Validation
builder.Services.AddValidatorsFromAssemblyContaining<AddFavoriteCityValidator>();
builder.Services.AddScoped<IValidatorService, ValidatorService>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
