using System;
using Adform.Ciam.Configuration.Extensions;
using Adform.Ciam.Logging.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Adform.Bloom.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.ConfigureConfig(context.HostingEnvironment);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(o => { o.AddServerHeader = false; })
                        .ConfigureLogging()
#if DEBUG
                        .UseUrls($"http://*:{Environment.GetEnvironmentVariable("HTTP_PORT") ?? "5002"}")
#endif
                        .UseStartup<Startup>();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                });
    }
}