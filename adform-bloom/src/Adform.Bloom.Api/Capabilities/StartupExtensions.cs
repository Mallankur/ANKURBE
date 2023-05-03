using System;
using Adform.Bloom.Api.Metrics;
using Adform.Bloom.DataAccess.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder AddBusinessMetrics(this IApplicationBuilder app,
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAdminGraphRepository>();

            Prometheus.Metrics.DefaultRegistry.AddBeforeCollectCallback(async ct =>
            {
                var result = await repo.GetStats();
                foreach (var (key, value) in result)
                {
                    BusinessMetrics.NumberOfNodesGauge.Set(value, key);
                }
            });
            return app;
        }
    }
}