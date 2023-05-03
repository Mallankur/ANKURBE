using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Api.Services
{
    public class ReadModelHealthCheck : IHealthCheck
    {
        private readonly IReadModelClient _modelClient;
        private readonly ILogger<ReadModelHealthCheck> _logger;

        public ReadModelHealthCheck(ILogger<ReadModelHealthCheck> logger, IReadModelClient modelClient)
        {
            _logger = logger;
            _modelClient = modelClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"{nameof(ReadModelHealthCheck)} executed.");
            var isHealthy = false;
            Exception? exception = null;

            try
            {
                if (await _modelClient.IsHealthy())
                    isHealthy = true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(ReadModelHealthCheck)} threw an exception.");
                exception = e;
            }

            return new HealthCheckResult(isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                exception: exception);
        }
    }
}