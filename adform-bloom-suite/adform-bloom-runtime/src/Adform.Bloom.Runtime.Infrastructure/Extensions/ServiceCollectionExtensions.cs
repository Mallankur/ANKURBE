using Adform.Bloom.Application.Abstractions.Cache;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Infrastructure.Cache;
using Adform.Bloom.Runtime.Infrastructure.Decorators;
using Adform.Bloom.Runtime.Infrastructure.Metrics;
using Adform.Bloom.Runtime.Infrastructure.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.Aerospike.Extensions;
using Adform.Ciam.Aerospike.Repository;
using Adform.Ciam.Cache.Decorators;
using Adform.Ciam.Health.Extensions;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using Adform.Ciam.OngDb.Pure.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Adform.Bloom.Runtime.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfigurationRoot configuration, IWebHostEnvironment environment)
        {
            services
                .Decorate<IRequestHandler<SubjectRuntimeQuery, IEnumerable<RuntimeResult>>>((inner, sp) =>
                    new MeasuredQueryHandler<SubjectRuntimeQuery, IEnumerable<RuntimeResult>>(inner,
                        sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.QueryDuration.Name),
                        "subjectRuntime"));

            services.Decorate<IRequestHandler<SubjectIntersectionQuery, IEnumerable<RuntimeResult>>>((inner, sp) =>
                new MeasuredQueryHandler<SubjectIntersectionQuery, IEnumerable<RuntimeResult>>(inner,
                    sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.QueryDuration.Name),
                    "subjectIntersection"));

            services
                .Decorate<IRequestHandler<LegacyTenantExistenceQuery, Result<bool>>>((inner, sp) =>
                    new MeasuredQueryHandler<LegacyTenantExistenceQuery, Result<bool>>(inner,
                        sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.QueryDuration.Name),
                        "legacyTenantExistenceQuery"));

            services
                .Decorate<IRequestHandler<NodeExistenceQuery, Result<bool>>>((inner, sp) =>
                    new MeasuredQueryHandler<NodeExistenceQuery, Result<bool>>(inner,
                        sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.QueryDuration.Name),
                        "nodeExistenceQuery"));

            services
                .Decorate<IRequestHandler<RoleExistenceQuery, Result<bool>>>((inner, sp) =>
                    new MeasuredQueryHandler<RoleExistenceQuery, Result<bool>>(inner,
                        sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.QueryDuration.Name),
                        "roleExistenceQuery"));

            if (environment.EnvironmentName == "testenv" ||
                environment.IsDevelopment())
            {
                services.ConfigureNeo(configuration);
            }
            else
            {
                services.ConfigureNeoCluster(configuration);
            }

            services.AddSingleton<ICursorToResultConverter, CursorToResultConverter>();
            services.AddSingleton<IValidateQuery, ValidateQuery>();
            services.AddSingleton<IAdformTenantProvider, AdformTenantProvider>();
            services.AddSingleton<IExistenceProvider, ExistenceProvider>();
            services.AddSingleton<IExistenceQueryValidator, ExistenceQueryValidator>();
           
            services.AddSingleton<IRuntimeProvider, RuntimeProvider>();
            services.Decorate<IRuntimeProvider, RuntimeProviderCache>();
            services.Decorate<IRuntimeProvider>((inner, sp) => new MeasuredRuntimeProvider(inner,
                 sp.GetRequiredService<IMetricsProvider>().GetHistogram(RuntimeMetrics.NeoDuration.Name)));

            services.AddSingleton<IKeyGenerator<SubjectQueryBase>, KeyGenerator>();
            services.AddSingleton<IRuntimeCacheManager, RuntimeCacheManager>();
            services.ConfigurationAerospike(configuration);
            services.AddWarmupFilter();
            return services;
        }

    }
}