using Adform.Bloom.Application.Abstractions.Cache;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.Aerospike.Configuration;
using Adform.Ciam.Aerospike.Repository;
using Adform.Ciam.Cache.Converters;
using Aerospike.Client;
using Microsoft.Extensions.Options;

namespace Adform.Bloom.Runtime.Infrastructure.Cache
{
    public class RuntimeCacheManager : IRuntimeCacheManager
    {
        private readonly IAerospikeConnection _aerospikeConnection;
        private readonly AerospikeConfiguration _aerospikeConfiguration;

        public RuntimeCacheManager(IAerospikeConnection aerospikeConnection, IOptions<AerospikeConfiguration> options)
        {
            _aerospikeConnection = aerospikeConnection;
            _aerospikeConfiguration = options.Value;
        }

        public async Task<IEnumerable<RuntimeResult>> FetchAsync(string key, Guid subjectId,
            CancellationToken cancellationToken = default)
        {
            var result = Enumerable.Empty<RuntimeResult>();
            try
            {
                var cacheKey = new Key(_aerospikeConfiguration.Namespace, _aerospikeConfiguration.Set, key);
                var record = _aerospikeConnection.Client.Operate(null, cacheKey,
                    Operation.Get(),
                    Operation.Touch()
                );
                result = ((byte[]) record.GetValue(_aerospikeConfiguration.BinName))
                    .FromByteArray<IEnumerable<RuntimeResult>>() ?? Array.Empty<RuntimeResult>();
            }
            catch (AerospikeException ae)
            {
                if (ae.Result == ResultCode.KEY_NOT_FOUND_ERROR)
                {
                    result = Enumerable.Empty<RuntimeResult>();
                }
            }

            return result;
        }

        public async Task RememberAsync(string key, Guid subjectId, IEnumerable<RuntimeResult> data, TimeSpan expiresIn,
            CancellationToken cancellationToken = default)
        {
            var list = data.ToList();
            var flat = list.SelectMany(p => p.Roles).ToList();
            var cacheKey = new Key(_aerospikeConfiguration.Namespace, _aerospikeConfiguration.Set, key);
            var bin0 = new Bin("SubjectId", subjectId);
            var bin1 = new Bin("RoleFlat", flat);
            var bin2 = new Bin(_aerospikeConfiguration.BinName, list.ToByteArray());
            _aerospikeConnection.Client.Put(new WritePolicy
            {
                expiration = expiresIn.Seconds
            }, cacheKey, bin0, bin1, bin2);
        }
    }
}