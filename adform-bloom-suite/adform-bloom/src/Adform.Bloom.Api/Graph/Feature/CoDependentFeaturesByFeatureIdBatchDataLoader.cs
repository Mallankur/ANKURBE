using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Queries;
using GreenDonut;
using MediatR;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class CoDependentFeaturesByFeatureIdBatchDataLoader : GroupedDataLoader<Guid, Contracts.Output.Feature>
    {
        private readonly IMediator _mediator;

        public CoDependentFeaturesByFeatureIdBatchDataLoader(IMediator mediator,
            IBatchScheduler batchScheduler, DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _mediator = mediator;
        }

        protected override async Task<ILookup<Guid, Contracts.Output.Feature>> LoadGroupedBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
            var result = await _mediator.Send(new CoDependentFeaturesQuery(keys), cancellationToken);
            return result.SelectMany(p => p.Value
                    .Select(x => new { p.Key, Value = x}))
                .ToLookup(pair => pair.Key, pair => pair.Value);
        }
    }
}