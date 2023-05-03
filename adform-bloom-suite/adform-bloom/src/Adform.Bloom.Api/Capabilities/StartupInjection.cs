using Adform.Bloom.Api.Metrics;
using Adform.Bloom.Api.Services;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.DataAccess.Decorators;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.Ports;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Extensions;
using Adform.Bloom.Write.Extensions;
using Adform.Ciam.Aerospike.Extensions;
using Adform.Ciam.Cache.Decorators;
using Adform.Ciam.Health.Extensions;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.SharedKernel.Services;
using Adform.Ciam.TokenProvider.Configuration;
using Adform.Ciam.TokenProvider.Handlers;
using Adform.Ciam.TokenProvider.Services;
using Grpc.Net.Client;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Client;
using System;
using System.Net.Http;
using Adform.Bloom.DataAccess.Providers.Access;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Middleware.Extensions;
using Adform.Ciam.Kafka.Configuration;
using Adform.Ciam.Kafka.Producer;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupInjection
    {
        public static IServiceCollection ConfigureInjection(this IServiceCollection services,
            IConfigurationRoot configuration, IWebHostEnvironment environment)
        {
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddMediatR(typeof(AuditEvent).Assembly);

            services.ConfigureQueries();
            services.ConfigureCommands(configuration);

            services.RegisterTransactionalBehaviors("CommandHandler");
            services.RegisterMeasuredMediatrBehavior("Handler", BloomMetrics.CqrsDuration.Name);

            services.RegisterRepositories();

            services.RegisterDomain();

            services.RegisterConfigurations(configuration);

            services.AddSingleton<IBloomCacheManager, BloomCacheManager>();
            services.ConfigurationAerospike(configuration);
            services.Decorate<IDistributedCache>((inner, sp) =>
                new MeasuredDistributedCache(inner,
                    sp.GetRequiredService<IMetricsProvider>()
                        .GetHistogram(BloomMetrics.CacheExecutionDuration.Name)));
            
            if (environment.EnvironmentName == "testenv" ||
                environment.IsDevelopment())
                services.ConfigureNeo(configuration);
            else
                services.ConfigureNeoCluster(configuration);

            services.ConfigureBloomMiddleware(configuration);

            //services.AddHostedService<AuditDispatcherService>();//TODO: uncomment for async enqueue
            services.AddSingleton<IAuditService, AuditService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            
            services.AddSingleton<ICallContextEnhancer, CallContextEnhancer>();

            services.AddSingleton(sp =>
            {
                GrpcClientFactory.AllowUnencryptedHttp2 = true;
                return GrpcChannel
                    .ForAddress(
                        $"{configuration.GetValue<string>("ReadModel:Host")}:{configuration.GetValue<string>("ReadModel:GrpcPort")}")
                    .CreateGrpcService<IUserService>();
            });
            services.AddSingleton(sp =>
            {
                GrpcClientFactory.AllowUnencryptedHttp2 = true;
                return GrpcChannel
                    .ForAddress(
                        $"{configuration.GetValue<string>("ReadModel:Host")}:{configuration.GetValue<string>("ReadModel:GrpcPort")}")
                    .CreateGrpcService<IBusinessAccountService>();
            });

            services.AddSingleton<IReadModelClient>(sp => new ReadModelModelClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("ReadModel")));
            services.AddHttpClient("ReadModel",
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                $"{configuration.GetSection("ReadModel")["Host"]}:{configuration.GetValue<string>("ReadModel:Port")}");
                    })
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var tokenProvider = provider.GetRequiredService<ITokenProvider>();
                    var oauthOptions = provider.GetRequiredService<IOptions<OAuth2Configuration>>();
                    var clientOptions = provider.GetRequiredService<IOptions<BloomReadClientSettings>>();
                    return new OAuthHandler(tokenProvider, oauthOptions, clientOptions);
                });

            services.AddSingleton<IKafkaProducer<string, SubjectAssignmentsNotification>>(provider =>
            {
                var histogram = provider.GetRequiredService<IMetricsProvider>()
                    .GetHistogram(BloomMetrics.KafkaProducerExecutionDuration.Name);
                var options = provider.GetRequiredService<IOptions<KafkaConfiguration>>();
                return new KafkaProducer<string, SubjectAssignmentsNotification>(
                    "ciam.bloom.master_account_assigments_notification",
                    options,
                    histogram);
            });
            
            services.AddSingleton<IKafkaProducer<string, SubjectAuthorizationResultChangedEvent>>(provider =>
            {
                var histogram = provider.GetRequiredService<IMetricsProvider>()
                    .GetHistogram(BloomMetrics.KafkaProducerExecutionDuration.Name);
                var options = provider.GetRequiredService<IOptions<KafkaConfiguration>>();
                return new KafkaProducer<string, SubjectAuthorizationResultChangedEvent>(
                    "ciam.bloom.master_account_assigments_notification",
                    options,
                    histogram);
            });

            services.AddSingleton<IClaimPrincipalGenerator, ClaimPrincipalGenerator>();

            services.AddWarmupFilter();

            return services;
        }

        private static void RegisterDomain(this IServiceCollection services)
        {
            services.AddSingleton<IAccessValidator, AccessValidator>();
            services.AddSingleton<IRoleValidator, ValidatorAdapter>();
            services.AddSingleton<ISubjectValidator, ValidatorAdapter>();
            services.AddSingleton<ITenantValidator, ValidatorAdapter>();
            services.AddSingleton<IPermissionValidator, ValidatorAdapter>();
            services.AddSingleton<IPolicyValidator, ValidatorAdapter>();
            services.AddSingleton<IFeatureValidator, ValidatorAdapter>();
            services.AddSingleton<ILicensedFeatureValidator, ValidatorAdapter>();
        }

        private static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IAdminGraphRepository, AdminGraphRepository>();
            services.Decorate<IAdminGraphRepository>((inner, sp) => new MeasuredAdminGraphRepository(inner,
                sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngDuration.Name)));
            services.AddSingleton<IDataLoaderRepository, DataLoaderRepository>();
            RegisterProviders(services);
        }

        private static void RegisterProviders(this IServiceCollection services)
        {
            services.AddSingleton<IVisibilityProvider<QueryParamsTenantIds,Subject>, SubjectVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsTenantIds, Subject>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsTenantIds,Subject>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services.AddSingleton<IVisibilityProvider<QueryParamsTenantIds,Contracts.Output.Permission>, PermissionVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsTenantIds,Contracts.Output.Permission>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsTenantIds,Contracts.Output.Permission>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services.AddSingleton<IVisibilityProvider<QueryParamsRoles,Contracts.Output.Role>, RoleVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsRoles,Contracts.Output.Role>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsRoles,Contracts.Output.Role>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services.AddSingleton<IVisibilityProvider<QueryParamsBusinessAccount, Contracts.Output.Tenant>, TenantVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsBusinessAccount, Contracts.Output.Tenant>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsBusinessAccount, Contracts.Output.Tenant>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services.AddSingleton<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>, FeatureVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services.AddSingleton<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Policy>, PolicyVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Policy>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Policy>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));
            
            services
                .AddSingleton<IVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>,
                    LicensedFeatureVisibilityProvider>();
            services.Decorate<IVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>>((inner, sp) =>
                new MeasuredVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(BloomMetrics.OngAclDuration.Name)));

            services
                .AddSingleton<IAccessProvider<Contracts.Output.BusinessAccount, QueryParamsTenantIds, Contracts.Output.Role>,
                    RoleByBusinessAccountAccessProvider>();
            
            services.AddSingleton<IAccessProvider<Subject, QueryParamsTenantIds, Contracts.Output.Role>, RoleByUserIdAccessProvider>();
           
            services
                .AddSingleton<IAccessProvider<Contracts.Output.BusinessAccount, QueryParamsTenantIds, Contracts.Output.User>,
                    UserByBusinessAccountAccessProvider>();

            services
                .AddSingleton<IAccessProvider<Contracts.Output.Feature, QueryParams, Contracts.Output.BusinessAccount>,
                    BusinessAccountByFeatureAccessProvider>();

            services
                .AddSingleton<IAccessProvider<Contracts.Output.Role, QueryParamsTenantIds, Contracts.Output.Feature>,
                    FeatureByRoleAccessProvider>();

            services.AddSingleton<IUserReadModelProvider, UserReadModelProvider>();
            services.AddSingleton<IBusinessAccountReadModelProvider, BusinessAccountReadModelProvider>();
        }

        private static void RegisterConfigurations(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<ValidationConfiguration>(configuration.GetSection(nameof(ValidationConfiguration)));
            services.Configure<BloomReadClientSettings>(configuration.GetSection("ReadModel"));
        }
    }
}
