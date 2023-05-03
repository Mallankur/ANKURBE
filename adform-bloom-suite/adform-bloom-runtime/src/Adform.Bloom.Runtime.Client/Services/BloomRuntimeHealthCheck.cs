using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Client.Contracts.Services
{
    public class BloomRuntimeHealthCheck : IHealthCheck
    {
        private readonly IBloomRuntimeClient _client;
        private readonly ILogger<BloomRuntimeHealthCheck> _logger;

        public BloomRuntimeHealthCheck(ILogger<BloomRuntimeHealthCheck> logger, IBloomRuntimeClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"{nameof(BloomRuntimeHealthCheck)} executed.");
            var isHealthy = false;
            Exception? exception = null;
            
            try
            {
                if (await _client.IsHealthy())
                    isHealthy = true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"{nameof(BloomRuntimeHealthCheck)} threw an exception.");
                exception = e;
            }

            return new HealthCheckResult(isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                exception: exception);
        }
    }
}