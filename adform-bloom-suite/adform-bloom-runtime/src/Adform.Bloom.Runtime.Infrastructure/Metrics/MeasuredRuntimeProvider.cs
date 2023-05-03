using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Extensions;

namespace Adform.Bloom.Runtime.Infrastructure.Metrics
{
    public class MeasuredRuntimeProvider : IRuntimeProvider
    {
        private readonly IRuntimeProvider _inner;
        private readonly ICustomHistogram _histogram;

        public MeasuredRuntimeProvider(IRuntimeProvider inner, ICustomHistogram histogram)
        {
            _inner = inner;
            _histogram = histogram;
        }

        public Task<IEnumerable<RuntimeResult>> GetSubjectEvaluation(SubjectQueryBase dto, CancellationToken cancellationToken = default) =>
            _histogram.MeasureAsync(() => _inner.GetSubjectEvaluation(dto, cancellationToken), "subject_evaluation");

        public Task<IEnumerable<RuntimeResult>> GetSubjectIntersection(SubjectIntersectionQuery dto, CancellationToken cancellationToken = default) =>
            _histogram.MeasureAsync(() => _inner.GetSubjectIntersection(dto, cancellationToken), "subject_intersection");
    }
}