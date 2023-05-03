using System;
using System.Collections.Generic;
using System.Security.Claims;
using MediatR;

namespace Adform.Bloom.Write.Commands
{
    public class UpdateLicensedFeatureToTenantAssignmentsCommand : IRequest
    {
        public UpdateLicensedFeatureToTenantAssignmentsCommand(ClaimsPrincipal principal, Guid tenantId, IReadOnlyCollection<Guid>? assignLicensedFeaturesIds, IReadOnlyCollection<Guid>? unassignLicensedFeaturesIds)
        {
            Principal = principal;
            TenantId = tenantId;
            AssignLicensedFeaturesIds = assignLicensedFeaturesIds;
            UnassignLicensedFeaturesIds = unassignLicensedFeaturesIds;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid TenantId { get; }
        public IReadOnlyCollection<Guid>? AssignLicensedFeaturesIds { get; }
        public IReadOnlyCollection<Guid>? UnassignLicensedFeaturesIds { get; }
    }
}