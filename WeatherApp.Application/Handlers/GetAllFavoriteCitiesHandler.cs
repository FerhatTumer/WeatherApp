using MediatR;
using WeatherApp.Application.Repositories;
using WeatherApp.Application.Queries;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Application.Handlers;

public class GetAllFavoriteCitiesHandler : IRequestHandler<GetAllFavoriteCitiesQuery, List<FavoriteCity>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllFavoriteCitiesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FavoriteCity>> Handle(GetAllFavoriteCitiesQuery request, CancellationToken cancellationToken)
    {
        var favoriteCities = await _unitOfWork.Repository<FavoriteCity>().GetAllAsync();
        return favoriteCities.ToList();
    }
}
