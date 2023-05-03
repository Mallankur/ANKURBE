using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Metrics;
using Adform.Bloom.Api.Middlewares;
using Adform.Bloom.Api.Services;
using Adform.Ciam.Aerospike.Services;
using Adform.Ciam.ExceptionHandling.Extensions;
using Adform.Ciam.GraphQLAdvanced.Extensions;
using Adform.Ciam.Health.Extensions;
using Adform.Ciam.Kafka.Extensions;
using Adform.Ciam.Logging.Extensions;
using Adform.Ciam.Monitoring.Extensions;
using Adform.Ciam.Monitoring.Metrics;
using Adform.Ciam.OngDb.Services;
using Adform.Ciam.RabbitMQ.Extensions;
using Adform.Ciam.RabbitMQ.Services;
using Adform.Ciam.SharedKernel.Configuration;
using Adform.Ciam.TokenProvider.Extensions;
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
using System;
using Adform.Bloom.Client.Contracts.Services;
using Tags = Adform.Ciam.OngDb.Core.Tags;

namespace Adform.Bloom.Api
{
    public class Startup
    {
        public Startup(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            HostEnvironment = hostingEnvironment;
            Configuration = (IConfigurationRoot)configuration;
        }

        private IWebHostEnvironment HostEnvironment { get; }
        private IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .ConfigureConfigurationValidation()
                .ConfigureOAuth(Configuration)
                .ConfigureOauthClient(Configuration)
                .ConfigureInjection(Configuration, HostEnvironment)
                .ConfigureMetrics(
                    BloomMetrics.CacheExecutionDuration,
                    BloomMetrics.KafkaProducerExecutionDuration,
                    BloomMetrics.OngAclDuration,
                    BloomMetrics.CqrsDuration,
                    BloomMetrics.OngDuration,
                    BloomMetrics.HandledExceptionsCounter)
                .ConfigureSwagger(Configuration)
                .ConfigureAuthorization()
                .ConfigureHealth(Configuration,
                    b => b
                        .AddCheck<OngDbHealthCheck>(
                            Tags.OngDB,
                            HealthStatus.Unhealthy,
                            new[] { Tags.OngDB })
                        //.AddCheck<KafkaHealthCheck>(
                        //    Ciam.Kafka.Tags.Kafka,
                        //    HealthStatus.Unhealthy,
                        //    new[] {Ciam.Kafka.Tags.Kafka})
                        .AddCheck<AerospikeHealthCheck>(
                            Ciam.Aerospike.Tags.Aerospike,
                            HealthStatus.Unhealthy,
                            new[] { Ciam.Aerospike.Tags.Aerospike })
                        .AddCheck<BloomRuntimeHealthCheck>(
                            Bloom.Client.Contracts.Tags.BloomRuntime,
                            HealthStatus.Unhealthy,
                            new[] { Bloom.Client.Contracts.Tags.BloomRuntime })
                        .AddCheck<ReadModelHealthCheck>(
                                Services.Tags.ReadModel,
                                HealthStatus.Unhealthy,
                                new[] { Services.Tags.ReadModel })
                        .AddCheck<RabbitMQHealthCheck>(
                                    Ciam.RabbitMQ.Tags.RabbitMQ,
                                    HealthStatus.Unhealthy,
                                    new[] { Ciam.RabbitMQ.Tags.RabbitMQ })
                )
                .ConfigureLogging(Configuration)
                .ConfigureKafka(Configuration)
                .ConfigureRabbitMQ(Configuration)
                .ConfigureGraphql(Configuration)
                .ConfigureMvc();

            services.AddDefaultCorrelationId(o =>
            {
                o.UpdateTraceIdentifier = false;
                o.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
                o.RequestHeader = "CorrelationId";
                o.ResponseHeader = "CorrelationId";
                o.AddToLoggingScope = true;
                o.IgnoreRequestHeader = false;
                o.IncludeInResponse = true;
            });

        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            IServiceProvider serviceProvider
        )
        {
            app
                .UseCorrelationId()
                .UseGatewayRequestId()
                .UseMaaSHostMiddleware(c => c.Request.Path.StartsWithSegments("/v1"))
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
                            Tool = {Enable = false}
                        });
                    endpoints.MapControllers();
                    endpoints.MapMetrics();
                })
                .UseHealthEndpoints()
                .UseSwaggerUi(apiVersionDescriptionProvider, Configuration)
#warning disabled in favor of orchestration
                .AddBusinessMetrics(serviceProvider);
        }
    }
}
