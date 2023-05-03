using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Read.Entities;

namespace Adform.Bloom.Application.Abstractions.Persistence
{
    public interface IRuntimeProvider
    {
        Task<IEnumerable<RuntimeResult>> GetSubjectEvaluation(SubjectQueryBase dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<RuntimeResult>> GetSubjectIntersection(SubjectIntersectionQuery dto, CancellationToken cancellationToken = default);
    }
}