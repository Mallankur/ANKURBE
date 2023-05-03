using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Providers.ReadModel
{
    public class BusinessAccountReadModelProvider : IBusinessAccountReadModelProvider
    {
        private readonly ICallContextEnhancer _ctxEnhancer;
        private readonly IBusinessAccountService _client;

        public BusinessAccountReadModelProvider(IBusinessAccountService client, ICallContextEnhancer ctxEnhancer)
        {
            _client = client;
            _ctxEnhancer = ctxEnhancer;
        }

        public async Task<BusinessAccount?> SearchForResourceAsync(Guid id, CancellationToken ct = default)
        {
            var context = await _ctxEnhancer.Build(ct);
            var result = await _client.GetBusinessAccount(new GetRequest {Id = id}, context);
            return result.BusinessAccount?.MapFromReadModel();
        }

        public async Task<EntityPagination<BusinessAccount>> SearchForResourcesAsync(
            int offset,
            int limit,
            QueryParams queryParams,
            IEnumerable<Guid>? ids = default,
            BusinessAccountType? baType = default, 
            CancellationToken ct = default)
        {
            var context = await _ctxEnhancer.Build(ct);
            var result =
                await _client.FindBusinessAccounts(
                    new BusinessAccountSearchRequest
                    {
                        Offset = offset,
                        Limit = limit,
                        Search = queryParams.Search,
                        OrderBy = queryParams.OrderBy ?? "Id",
                        SortingOrder = (SortingOrder)queryParams.SortingOrder,
                        Ids = ids?.ToArray(), 
                        Type = (BusinessAccountType?) baType
                    }, context);
            return new EntityPagination<BusinessAccount>(offset, limit, result.TotalItems, result.BusinessAccounts.MapFromReadModel());
        }
    }
}