using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.Aerospike.Configuration;
using Adform.Ciam.Aerospike.Repository;
using Adform.Ciam.Cache.Converters;
using Aerospike.Client;
using Microsoft.Extensions.Options;

namespace Adform.Bloom.Infrastructure.Cache
{
    public class BloomCacheManager : IBloomCacheManager
    {
        private readonly IAerospikeConnection _aerospikeConnection;
        private readonly AerospikeConfiguration _aerospikeConfiguration;

        public BloomCacheManager(IAerospikeConnection aerospikeConnection, IOptions<AerospikeConfiguration> options)
        {
            _aerospikeConnection = aerospikeConnection;
            _aerospikeConfiguration = options.Value;
        }
        
        public async Task<IEnumerable<RuntimeResponse>?> FetchAsync(string key,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<RuntimeResponse>? result = null;
            try
            {
                var cacheKey = new Key(_aerospikeConfiguration.Namespace, _aerospikeConfiguration.Set, key);
                var record = _aerospikeConnection.Client.Operate(null, cacheKey,
                    Operation.Get(),
                    Operation.Touch()
                );
                result = ((byte[]) record.GetValue(_aerospikeConfiguration.BinName))
                    .FromByteArray<IEnumerable<RuntimeResponse>?>();
            }
            catch (AerospikeException ae)
            {
                if (ae.Result == ResultCode.KEY_NOT_FOUND_ERROR)
                {
                    result = null;
                }
            }

            return result;
        }

        public async Task RememberAsync(string key, IEnumerable<RuntimeResponse> data, TimeSpan expiresIn,
            CancellationToken cancellationToken = default)
        {
            var list = data.ToList();
            var flat = list.SelectMany(p => p.Roles).ToList();
            var cacheKey = new Key(_aerospikeConfiguration.Namespace, _aerospikeConfiguration.Set, key);
            var bin0 = new Bin("SubjectId", key);
            var bin1 = new Bin("RoleFlat", flat);
            var bin2 = new Bin(_aerospikeConfiguration.BinName, list.ToByteArray());
            _aerospikeConnection.Client.Put(new WritePolicy
            {
                expiration = expiresIn.Seconds
            }, cacheKey, bin0, bin1, bin2);
        }
        
        public async Task ForgetBySubjectAsync(string key, CancellationToken cancellationToken = default)
        {
            var stmt = new Statement();
            stmt.SetNamespace(_aerospikeConfiguration.Namespace);
            stmt.SetSetName(_aerospikeConfiguration.Set);
            stmt.SetPredExp(
                PredExp.StringValue(key),           
                PredExp.StringBin("SubjectId"),
                PredExp.StringEqual());
            var rs = _aerospikeConnection.Client.Query(null, stmt);
            try
            {
                while (rs.Next())
                {
                    _aerospikeConnection.Client.Delete(new WritePolicy(), rs.Key);
                }
            }
            finally
            {
                rs.Close();
            }
        }

        public async Task ForgetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var stmt = new Statement();
            stmt.SetNamespace(_aerospikeConfiguration.Namespace);
            stmt.SetSetName(_aerospikeConfiguration.Set);
            stmt.SetPredExp(
                PredExp.StringVar("x"),
                PredExp.StringValue(roleName),
                PredExp.StringEqual(),
                PredExp.ListBin("RoleFlat"),
                PredExp.ListIterateOr("x"));
            var rs = _aerospikeConnection.Client.Query(null, stmt);
            try
            {
                while (rs.Next())
                {
                    _aerospikeConnection.Client.Delete(new WritePolicy(), rs.Key);
                }
            }
            finally
            {
                rs.Close();
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _aerospikeConnection.Client.Truncate(new InfoPolicy(), _aerospikeConfiguration.Namespace,
                    _aerospikeConfiguration.Set, null);
            }
            catch (AerospikeException e) when (e.Result == ResultCode.CLIENT_ERROR)
            {
            }
        }
    }
}