using Adform.Bloom.Runtime.Read.Entities;

namespace Adform.Bloom.Application.Abstractions.Cache
{
    public interface IRuntimeCacheManager
    {
        Task<IEnumerable<RuntimeResult>> FetchAsync(string key, Guid subjectId,
            CancellationToken cancellationToken = default);

        Task RememberAsync(string key, Guid subjectId, IEnumerable<RuntimeResult> data, TimeSpan expiresIn,
            CancellationToken cancellationToken = default);
    }
}