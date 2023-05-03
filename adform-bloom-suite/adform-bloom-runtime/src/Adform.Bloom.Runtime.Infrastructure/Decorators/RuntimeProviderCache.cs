using Adform.Bloom.Application.Abstractions.Cache;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Infrastructure.Cache;
using Adform.Bloom.Runtime.Read.Entities;

namespace Adform.Bloom.Runtime.Infrastructure.Decorators
{
    public class RuntimeProviderCache : IRuntimeProvider
    {
        private readonly IKeyGenerator<SubjectQueryBase> _keyGenerator;
        private readonly IRuntimeProvider _inner;
        private readonly IRuntimeCacheManager _cacheManager;

        public RuntimeProviderCache(IRuntimeProvider inner, IKeyGenerator<SubjectQueryBase> keyGenerator,
            IRuntimeCacheManager cacheManager)
        {
            _keyGenerator = keyGenerator;
            _cacheManager = cacheManager;
            _inner = inner;
        }

        public async Task<IEnumerable<RuntimeResult>> GetSubjectEvaluation(SubjectQueryBase dto,
            CancellationToken cancellationToken = default)
        {
            return await GetFromCache<SubjectQueryBase>(dto, cancellationToken);
        }

        public async Task<IEnumerable<RuntimeResult>> GetSubjectIntersection(SubjectIntersectionQuery dto,
            CancellationToken cancellationToken = default)
        {
            return await _inner.GetSubjectIntersection(dto, cancellationToken);
        }

        private async Task<IEnumerable<RuntimeResult>> GetFromCache<T>(T dto, CancellationToken cancellationToken)
            where T : SubjectQueryBase
        {
            var key = _keyGenerator.GenerateKey(dto);
            var results = await _cacheManager.FetchAsync(key, dto.SubjectId, cancellationToken);
            if (results.Any()) return results;
            results = await _inner.GetSubjectEvaluation(dto, cancellationToken);
            await _cacheManager.RememberAsync(key, dto.SubjectId, results, TimeSpan.FromMinutes(1), cancellationToken);
            return results;
        }
    }
}