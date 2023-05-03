using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Contracts.Response;

namespace Adform.Bloom.Infrastructure.Cache
{
    public interface IBloomCacheManager
    {
        Task<IEnumerable<RuntimeResponse>?> FetchAsync(string key,
            CancellationToken cancellationToken = default);

        Task RememberAsync(string key, IEnumerable<RuntimeResponse> data, TimeSpan expiresIn,
            CancellationToken cancellationToken = default);
        Task ForgetBySubjectAsync(string key, CancellationToken cancellationToken = default);
        
        Task ForgetByRoleAsync(string roleName, CancellationToken cancellationToken = default);

        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}