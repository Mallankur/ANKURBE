using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.CustomStructures;
using Prometheus;

namespace Adform.Bloom.Read.Infrastructure.Metrics;

public static class CommonMetrics
{
    private const string DurationMetric = "execution_latency";
    private const string CacheDurationMetric = "cache_latency";
    private const string PsgqlDurationMetric = "psgql_latency";
    private const string HandledExceptionsCounterMetric = "handled_exceptions_total";

    public static readonly ICustomHistogram ExecutionDuration =
        new CustomHistogram(
            Prometheus.Metrics.CreateHistogram(DurationMetric,
                "Latency for operations (in ms)", new HistogramConfiguration
                {
                    LabelNames = new[] { "operation_name" },
                    Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                }));

    public static readonly ICustomHistogram CacheExecutionDuration =
        new CustomHistogram(
            Prometheus.Metrics.CreateHistogram(CacheDurationMetric,
                "Latency for cache operations (in ms)", new HistogramConfiguration
                {
                    LabelNames = new[] { "operation_name" },
                    Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                }));

    public static readonly ICustomHistogram PsgqlDuration =
        new CustomHistogram(
            Prometheus.Metrics.CreateHistogram(PsgqlDurationMetric,
                "Latency for Psgql operations (in ms)", new HistogramConfiguration
                {
                    LabelNames = new[] { "type", "operation_name" },
                    Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000 }
                }));

    public static readonly ICustomCounter HandledExceptionsCounter =
        new CustomCounter(
            Prometheus.Metrics.CreateCounter(HandledExceptionsCounterMetric, "Counter of handled exceptions",
                "error_type"));
}