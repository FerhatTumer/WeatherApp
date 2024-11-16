using MediatR;

namespace WeatherApp.Application.Commands;

public record AddFavoriteCityCommand(string CityName) : IRequest;