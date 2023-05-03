using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using GreenDonut;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class FeatureByPermissionIdBatchDataLoader : BatchDataLoader<Guid, Contracts.Output.Feature>
    {
        private readonly IDataLoaderRepository _repository;

        public FeatureByPermissionIdBatchDataLoader(
            IDataLoaderRepository repository,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<Guid, Contracts.Output.Feature>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
            var result = await _repository.GetNodesWithConnectedAsync<Domain.Entities.Permission, Domain.Entities.Feature>(
                keys, Constants.ContainsIncomingLink);
            return result.ToDictionary(x => x.StartNodeId,
                x => x.ConnectedNode!.MapFromDomain<Domain.Entities.Feature, Contracts.Output.Feature>());
        }
    }
}