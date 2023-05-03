using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.CustomStructures;
using Prometheus;

namespace Adform.Bloom.Runtime.Infrastructure.Metrics
{
    public class RuntimeMetrics
    {
        public static readonly ICustomHistogram QueryDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("query_latency",
                    "Latency for query (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] { "query_name" },
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));

        public static readonly ICustomHistogram NeoDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("neo_latency",
                    "Latency for Neo (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] { "operation_type" },
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));

        public static readonly ICustomHistogram CacheExecutionDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("cache_latency",
                    "Latency for cache operations (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] { "operation_name" },
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));

        public static readonly ICustomHistogram KafkaConsumerExecutionDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("kafka_consumer_latency",
                    "Latency for consuming messages (in ms)", new HistogramConfiguration
                    {
                        LabelNames = new[] { "topic_name" },
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));

        public static readonly ICustomHistogram QueryPreparationDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("query_preparation_latency",
                    "Latency for query preparation (in ms)", new HistogramConfiguration
                    {
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));

        public static readonly ICustomHistogram QueryExecutionDuration =
            new CustomHistogram(
                Prometheus.Metrics.CreateHistogram("query_execution_latency",
                    "Latency for query execution (in ms)", new HistogramConfiguration
                    {
                        Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                    }));
    }
}