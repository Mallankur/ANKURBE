using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using GreenDonut;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class RolesByPolicyIdBatchDataLoader : GroupedDataLoader<Guid, Contracts.Output.Role>
    {
        private readonly IDataLoaderRepository _repository;

        public RolesByPolicyIdBatchDataLoader(
            IDataLoaderRepository repository,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _repository = repository;
        }

        protected override async Task<ILookup<Guid, Contracts.Output.Role>> LoadGroupedBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
            var result = await _repository.GetNodesWithConnectedAsync<Domain.Entities.Policy,
                Domain.Entities.Role>(keys, Constants.ContainsLink);
            return result.ToLookup(x => x.StartNodeId,
                y => y.ConnectedNode!.MapFromDomain<Domain.Entities.Role, Contracts.Output.Role>());
        }
    }
}