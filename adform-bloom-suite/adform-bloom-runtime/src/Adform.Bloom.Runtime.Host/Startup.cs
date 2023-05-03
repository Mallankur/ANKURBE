using System;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Infrastructure.Metrics;
using Adform.Ciam.Authorization.Extensions;
using Adform.Ciam.Cache.Services;
using Adform.Ciam.ExceptionHandling.Extensions;
using Adform.Ciam.GraphQLAdvanced.Extensions;
using Adform.Ciam.Health.Extensions;
using Adform.Ciam.Logging.Extensions;
using Adform.Ciam.Monitoring.Extensions;
using Adform.Ciam.Monitoring.Metrics;
using Adform.Ciam.OngDb.Pure.Services;
using CorrelationId;
using CorrelationId.DependencyInjection;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace Adform.Bloom.Runtime.Host
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfigurationRoot _configuration;

        public Startup(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _hostEnvironment = hostingEnvironment;
            _configuration = (IConfigurationRoot)configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .ConfigureOAuth(_configuration)
                .ConfigureInjection(_configuration, _hostEnvironment)
                .ConfigureMetrics(
                    RuntimeMetrics.QueryDuration,
                    RuntimeMetrics.NeoDuration,
                    RuntimeMetrics.CacheExecutionDuration,
                    RuntimeMetrics.KafkaConsumerExecutionDuration,
                    RuntimeMetrics.QueryPreparationDuration,
                    RuntimeMetrics.QueryExecutionDuration
                )
                .ConfigureSwagger(_configuration)
                .ConfigureAuthorization(options =>
                {
                    options.AddScopePolicy(StartupOAuth.Scopes.Readonly);
                })
                .ConfigureHealth(_configuration,
                    b => b
                        .AddCheck<OngDbHealthCheck>(
                            Ciam.OngDb.Core.Tags.OngDB,
                            HealthStatus.Unhealthy,
                            new[] { Ciam.OngDb.Core.Tags.OngDB })
                        .AddCheck<DistributedCacheHealthCheck>(
                            Ciam.Cache.Tags.DistributedCache,
                            HealthStatus.Unhealthy,
                            new[] { Ciam.Cache.Tags.DistributedCache })
                    )
                .ConfigureLogging(_configuration)
                .ConfigureGraphql(_configuration)
                .ConfigureMvc();

            services.AddDefaultCorrelationId(o =>
            {
                o.UpdateTraceIdentifier = false;
                o.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
                o.ResponseHeader = "CorrelationId";
                o.IncludeInResponse = true;
            });
            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            app
                .UseCorrelationId()
                .UseMaaSHostMiddleware()
                .UseErrorHandler(DefaultMetrics.ErrorCounter)
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseLaaSMiddleware()
                .UseGraphQLSchema()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGraphQL().WithOptions(
                        new GraphQLServerOptions
                        {
                            Tool = { Enable = false }
                        });
                    endpoints.MapControllers();
                    endpoints.MapMetrics();
                })
                .UseHealthEndpoints()
                .UseSwaggerUi(apiVersionDescriptionProvider, _configuration);
        }

    }
}
