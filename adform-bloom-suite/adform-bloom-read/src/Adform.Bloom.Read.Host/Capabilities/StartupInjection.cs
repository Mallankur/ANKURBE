using Adform.Bloom.Read.Application.Extensions;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Host.Services;
using Adform.Bloom.Read.Infrastructure.Extensions;
using Adform.Ciam.SharedKernel.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Read.Host.Capabilities;

public static class StartupInjection
{
    public static IServiceCollection ConfigureInjection(this IServiceCollection services,
        IConfigurationRoot configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddApplication();
        services.AddInfrastructure(configuration);
        
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IBusinessAccountService, BusinessAccountService>();
        return services;
    }
}