using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using GreenDonut;

namespace Adform.Bloom.Api.Graph.Role
{
    public class BusinessAccountByRoleIdBatchDataLoader : BatchDataLoader<Guid, Contracts.Output.BusinessAccount>
    {
        private readonly IDataLoaderRepository _repository;
        private readonly IBusinessAccountReadModelProvider _businessAccountReadModelProvider;

        public BusinessAccountByRoleIdBatchDataLoader(
            IDataLoaderRepository repository,
            IBusinessAccountReadModelProvider businessAccountReadModelProvider,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _repository = repository;
            _businessAccountReadModelProvider = businessAccountReadModelProvider;
        }

        protected override async Task<IReadOnlyDictionary<Guid, Contracts.Output.BusinessAccount>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
            var result =
                await _repository.GetNodesWithConnectedAsync<Domain.Entities.Role, Domain.Entities.Tenant>(keys,
                    Constants.OwnsIncomingLink);

            var businessAccounts =
                await _businessAccountReadModelProvider.SearchForResourcesAsync(0, LimitType.MaxValue,new QueryParams(),
                    ids: result.Select(o => o.ConnectedNode!.Id), ct: cancellationToken);
            return result.ToDictionary(x => x.StartNodeId,
                x => businessAccounts.Data.FirstOrDefault(o => o.Id == x.ConnectedNode!.Id))!;
        }
    }
}