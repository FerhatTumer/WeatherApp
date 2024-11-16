using FluentValidation;
using WeatherApp.Application.Commands;

namespace WeatherApp.Application.Validators;

public class AddFavoriteCityValidator : AbstractValidator<AddFavoriteCityCommand>
{
    public AddFavoriteCityValidator()
    {
        RuleFor(x => x.CityName)
            .NotEmpty().WithMessage("City name is required.")
            .MaximumLength(10).WithMessage("City name cannot exceed 100 characters.");
    }
}
