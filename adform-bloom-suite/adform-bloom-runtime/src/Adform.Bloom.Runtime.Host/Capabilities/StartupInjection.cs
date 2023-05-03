using Adform.Bloom.Application.Extensions;
using Adform.Bloom.Runtime.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Runtime.Host.Capabilities
{
    public static class StartupInjection
    {
        public static IServiceCollection ConfigureInjection(this IServiceCollection services,
            IConfigurationRoot configuration, IWebHostEnvironment environment)
        {
            services.AddApplication()
                .AddInfrastructure(configuration, environment);
            return services;
        }
    }
}