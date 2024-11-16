using MediatR;
using WeatherApp.Application.Commands;
using WeatherApp.Application.Repositories;
using WeatherApp.Domain.Entities;

namespace WeatherApp.Application.Handlers;

public class AddFavoriteCityHandler : IRequestHandler<AddFavoriteCityCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddFavoriteCityHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddFavoriteCityCommand request, CancellationToken cancellationToken)
    {
        var city = new FavoriteCity(request.CityName);
        await _unitOfWork.Repository<FavoriteCity>().AddAsync(city);
        await _unitOfWork.SaveChangesAsync();
    }
}
