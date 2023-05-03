using System.Diagnostics;
using System.Threading.Tasks;
using Adform.Bloom.Read.Infrastructure.Metrics;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Read.Host.Metrics;

public class MetricsInterceptor : Interceptor
{
    private readonly ICustomHistogram _histogram;
    private readonly ILogger<MetricsInterceptor> _logger;

    public MetricsInterceptor(IMetricsProvider metrics, ILogger<MetricsInterceptor> logger)
    {
        _logger = logger;
        _histogram = metrics.GetHistogram(CommonMetrics.ExecutionDuration.Name);
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // Metric
        var sw = Stopwatch.StartNew();
        try
        {
            return await continuation(request, context);
        }
        finally
        {
            sw.Stop();
            _histogram.Observe(sw.ElapsedMilliseconds, context.Method);
        }
    }
}