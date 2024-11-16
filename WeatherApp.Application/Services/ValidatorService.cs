using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherApp.Application.Services;

public interface IValidatorService
{
    Task<List<string>> ValidateAsync<T>(T instance);
}

public class ValidatorService : IValidatorService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<string>> ValidateAsync<T>(T instance)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null) return new List<string>();

        var result = await validator.ValidateAsync(instance);
        return result.IsValid ? new List<string>() : result.Errors.Select(e => e.ErrorMessage).ToList();
    }
}
