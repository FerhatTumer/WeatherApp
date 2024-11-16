using MediatR;
using WeatherApp.Application.Commands;
using WeatherApp.Application.Repositories;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Application.Handlers;

public class DeleteFavoriteCityHandler : IRequestHandler<DeleteFavoriteCityCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFavoriteCityHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteFavoriteCityCommand request, CancellationToken cancellationToken)
    {
        var city = await _unitOfWork.Repository<FavoriteCity>().GetByIdAsync(request.CityId);
        if (city == null) return false;

        _unitOfWork.Repository<FavoriteCity>().Remove(city);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
