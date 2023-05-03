using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.CustomStructures;

namespace Adform.Bloom.Api.Metrics
{
    public class BusinessMetrics
    {
        private const string NumberOfNodesGaugeMetric = "number_of_nodes";
        public static readonly ICustomGauge NumberOfNodesGauge =
            new CustomGauge(
                Prometheus.Metrics.CreateGauge(NumberOfNodesGaugeMetric, "Number of nodes", "node_type"));
    }
}