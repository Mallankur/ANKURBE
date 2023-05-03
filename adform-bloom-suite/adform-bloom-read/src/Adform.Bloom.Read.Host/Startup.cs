using System;
using Adform.Bloom.Read.Host.Capabilities;
using Adform.Bloom.Read.Host.Metrics;
using Adform.Bloom.Read.Host.Services;
using Adform.Bloom.Read.Infrastructure.HealthChecks;
using Adform.Bloom.Read.Infrastructure.Metrics;
using Adform.Ciam.Authorization.Extensions;
using Adform.Ciam.ExceptionHandling.Extensions;
using Adform.Ciam.Grpc.Extensions;
using Adform.Ciam.Health.Extensions;
using Adform.Ciam.Logging.Extensions;
using Adform.Ciam.Monitoring.Extensions;
using Adform.Ciam.Monitoring.Metrics;
using Adform.Ciam.SharedKernel.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Tags = Adform.Ciam.Health.Tags;

namespace Adform.Bloom.Read.Host;

public class Startup
{
    private IConfigurationRoot Configuration { get; }

    public Startup(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
    {
        Configuration = (IConfigurationRoot) configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .ConfigureConfigurationValidation()
            .ConfigureOAuth(Configuration)
            .ConfigureInjection(Configuration)
            .ConfigureMetrics(
                CommonMetrics.ExecutionDuration,
                CommonMetrics.PsgqlDuration,
                CommonMetrics.HandledExceptionsCounter,
                CommonMetrics.CacheExecutionDuration)
            .ConfigureAuthorization(options =>
            {
                options.AddScopePolicy(StartupOAuth.Scopes.Full, StartupOAuth.Scopes.Readonly);
                options.AddScopePolicy(StartupOAuth.Scopes.Full);
                options.AddScopePolicy(StartupOAuth.Scopes.Readonly);
            })
            .ConfigureHealth(Configuration,
                b => b
                    .AddCheck<NpgSqlHealthCheck>(
                        Read.Infrastructure.HealthChecks.Tags.NpgSql,
                        HealthStatus.Unhealthy,
                        new[] {Read.Infrastructure.HealthChecks.Tags.NpgSql})
            )
            .ConfigureLogging(Configuration)
            .ConfigureGrpcServer(typeof(MetricsInterceptor));
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IServiceProvider serviceProvider
    )
    {
        app
            .UseMaaSHostMiddleware()
            .UseErrorHandler(DefaultMetrics.ErrorCounter)
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseLaaSMiddleware()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.UseGrpcEndpoints(typeof(BusinessAccountService), typeof(UserService));
            })
            .UseHealthEndpoints();
    }
}