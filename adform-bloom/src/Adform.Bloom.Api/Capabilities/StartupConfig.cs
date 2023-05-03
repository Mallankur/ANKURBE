using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupConfig
    {
        public static IConfigurationRoot ConfigureConfig(this IWebHostEnvironment environment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            return builder
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}