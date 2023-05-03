using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface IFeatureValidator
    {
        Task<bool> DoFeaturesExist(IReadOnlyCollection<Guid> featureIds);

        Task<bool> HasVisibilityToFeaturesAsync(ClaimsPrincipal principal,
            IReadOnlyCollection<Guid> featureIds,
            IReadOnlyCollection<Guid>? tenantIds = null);

        Task<bool> IsFeatureDependentOnOtherFeature(Guid featureId, Guid dependsOnId);

        Task<bool> CoDependencyFeaturesSelected(IReadOnlyCollection<Guid> featureIds);
        Task<bool> IsTenantAssignedToFeatureCoDependenciesAsync(Guid tenantId, Guid featureId);
        Task<bool> IsTenantAssignedToFeatureWithoutDependants(Guid tenantId, Guid featureId);

        Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal,
            IReadOnlyCollection<Guid> featureIds, IReadOnlyCollection<Guid>? tenantIds = null);
    }
}
