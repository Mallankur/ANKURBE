using System;
using System.IO;
using Adform.Ciam.Configuration.Extensions;
using Adform.Ciam.Grpc.Configuration;
using Adform.Ciam.Grpc.Extensions;
using Adform.Ciam.Logging.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Adform.Bloom.Read.Host;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.ConfigureConfig(context.HostingEnvironment);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {            
                var httpPort = Environment.GetEnvironmentVariable("HTTP_PORT")?? "5004";
                var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT")?? "9696";
                webBuilder.UseGrpcServer(p=>
                    {
                        p.HttpPort= int.Parse(httpPort);
                        p.GrpcPort= int.Parse(grpcPort);
                    })
                    .ConfigureLogging()
                    .UseStartup<Startup>();
            })
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                options.ValidateOnBuild = true;
            });
}