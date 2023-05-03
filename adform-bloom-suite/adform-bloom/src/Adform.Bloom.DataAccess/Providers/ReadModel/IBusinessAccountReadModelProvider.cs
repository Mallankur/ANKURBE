using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Providers.ReadModel
{
    public interface IBusinessAccountReadModelProvider
    {
        Task<BusinessAccount?> SearchForResourceAsync(Guid id, CancellationToken ct = default);

        Task<EntityPagination<BusinessAccount>> SearchForResourcesAsync(
            int offset,
            int limit,
            QueryParams queryParams,
            IEnumerable<Guid>? ids = default,
            BusinessAccountType? baType = default,
            CancellationToken ct = default);
    }
}