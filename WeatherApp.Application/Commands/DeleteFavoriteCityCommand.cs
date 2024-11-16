using MediatR;

namespace WeatherApp.Application.Commands;

public record DeleteFavoriteCityCommand(Guid CityId) : IRequest<bool>;
