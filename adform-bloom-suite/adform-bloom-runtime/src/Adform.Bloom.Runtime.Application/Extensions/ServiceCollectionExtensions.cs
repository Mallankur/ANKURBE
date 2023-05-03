using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {  
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddMediatR(typeof(ServiceCollectionExtensions));
        return services;
    }
}