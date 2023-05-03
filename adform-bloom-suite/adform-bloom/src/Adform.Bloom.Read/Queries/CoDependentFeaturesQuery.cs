using System;
using System.Collections.Generic;
using Adform.Bloom.Contracts.Output;
using MediatR;

namespace Adform.Bloom.Read.Queries
{
    public class CoDependentFeaturesQuery: IRequest<IDictionary<Guid, List<Feature>>>
    {
        public IEnumerable<Guid> FeatureIds { get; }

        public CoDependentFeaturesQuery(IEnumerable<Guid> featureIds)
        {
            FeatureIds = featureIds;
        }
    }
}