using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.CustomStructures;
using Prometheus;

namespace Adform.Bloom.Api.Metrics
{
    public static class BloomMetrics
    {
        private const string CacheDurationMetric = "cache_latency";
        private const string KafkaProducerDurationMetric = "kafka_producer_latency";
        private const string OngAclDurationMetric = "ong_acl_latency";
        private const string CqrsDurationMetric = "cqrs_latency";
        private const string OngDurationMetric = "ong_latency";
        private const string HandledExceptionsCounterMetric = "handled_exceptions_total";

        public static readonly ICustomHistogram CacheExecutionDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram(CacheDurationMetric,
                    "Latency for cache operations (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] {"operation_name"},
                        Buckets = new double[] {10, 25, 50, 100, 250, 500, 1000, 2500, 5000}
                    }));

        public static readonly ICustomHistogram KafkaProducerExecutionDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram(KafkaProducerDurationMetric,
                    "Latency for producing messages (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] {"topic_name"},
                        Buckets = new double[] {10, 25, 50, 100, 250, 500, 1000, 2500, 5000}
                    }));

        public static readonly ICustomHistogram OngAclDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram(OngAclDurationMetric,
                    "Latency for ONgDB ACL operations per node type (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] {"node_type", "operation_name"},
                        Buckets = new double[] {10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000}
                    }));

        public static readonly ICustomHistogram CqrsDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram(CqrsDurationMetric,
                    "Latency for commands and queries per operation type (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] {"operation_type"},
                        Buckets = new double[] {10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000}
                    }));

        public static readonly ICustomHistogram OngDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram(OngDurationMetric,
                    "Latency for ONgDB operations (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] {"operation_name"},
                        Buckets = new double[] {10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000}
                    }));

        public static readonly ICustomCounter HandledExceptionsCounter =
            new CustomCounter(
                Prometheus.Metrics.CreateCounter(HandledExceptionsCounterMetric, "Counter of handled exceptions",
                    "error_type"));
    }
}