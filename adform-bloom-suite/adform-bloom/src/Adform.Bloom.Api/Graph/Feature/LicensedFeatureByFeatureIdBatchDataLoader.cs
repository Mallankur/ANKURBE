using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using GreenDonut;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class LicensedFeatureByFeatureIdBatchDataLoader : BatchDataLoader<Guid, Contracts.Output.LicensedFeature>
    {
        private readonly IDataLoaderRepository _repository;

        public LicensedFeatureByFeatureIdBatchDataLoader(
            IDataLoaderRepository repository,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<Guid, Contracts.Output.LicensedFeature>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
            var result = await _repository.GetNodesWithConnectedAsync<Domain.Entities.Feature, Domain.Entities.LicensedFeature>(
                keys, Constants.ContainsIncomingLink);
            return result.ToDictionary(x => x.StartNodeId,
                x => x.ConnectedNode!.MapFromDomain<Domain.Entities.LicensedFeature, Contracts.Output.LicensedFeature>());
        }
    }
}