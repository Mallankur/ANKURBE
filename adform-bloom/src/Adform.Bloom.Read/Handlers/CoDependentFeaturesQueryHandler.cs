using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Read.Queries;
using MediatR;

namespace Adform.Bloom.Read.Handlers
{
    public class CoDependentFeaturesQueryHandler : IRequestHandler<CoDependentFeaturesQuery,
        IDictionary<Guid, List<Adform.Bloom.Contracts.Output.Feature>>>
    {
        private readonly IDataLoaderRepository _dataLoaderRepository;

        public CoDependentFeaturesQueryHandler(IDataLoaderRepository dataLoaderRepository)
        {
            _dataLoaderRepository = dataLoaderRepository;
        }

        public async Task<IDictionary<Guid, List<Adform.Bloom.Contracts.Output.Feature>>> Handle(
            CoDependentFeaturesQuery request,
            CancellationToken cancellationToken)
        {
            var result =
                await _dataLoaderRepository.GetNodesWithConnectedAsync<Feature, Feature>(
                    request.FeatureIds, Constants.DependsOnLink);
            return result.ToGraphQlFriendlyDictionary(x => new Adform.Bloom.Contracts.Output.Feature
            {
                Id = x!.Id,
                Name = x!.Name,
                Description = x!.Description,
                Enabled = x!.IsEnabled
            });
        }
    }
}