using MediatR;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Application.Queries;

public record GetAllFavoriteCitiesQuery() : IRequest<List<FavoriteCity>>;
