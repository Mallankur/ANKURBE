using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.ValueObjects;

namespace Adform.Bloom.Domain.Interfaces
{
    public interface IAccessValidator
    {
        Task<ValidationResult> CanDeleteRoleAsync(ClaimsPrincipal principal, Guid roleId);

        Task<bool> CanCreateSubjectAsync(ClaimsPrincipal principal, Guid subjectId);

        Task<bool> CanDeleteSubjectAsync(ClaimsPrincipal principal, Guid subjectId);

        Task<ValidationResult> CanAssignPermissionToRoleAsync(ClaimsPrincipal principal,
            Guid permissionId, Guid roleId);

        Task<ValidationResult> CanCreateRole(ClaimsPrincipal principal,
            Guid? policyId, Guid tenantId, IReadOnlyCollection<Guid>? featureIds,
            bool isTemplateRole);

        Task<ValidationResult> CanUpdateRole(ClaimsPrincipal principal, Guid roleId);

        Task<ValidationResult> CanAssignSubjectToRolesAsync(ClaimsPrincipal principal,
            IEnumerable<RoleTenant> assignments,
            IEnumerable<RoleTenant>? filteredAssignments,
            IEnumerable<RoleTenant>? filteredUnassignments, Guid subjectId);

        Task<ValidationResult> CanUnassignSubjectFromRolesAsync(ClaimsPrincipal principal,
            IEnumerable<RoleTenant> assignments, Guid subjectId);

        Task<ValidationResult> CanAssignRoleToFeaturesAsync(ClaimsPrincipal principal,
            Guid roleId, IReadOnlyCollection<Guid> featureIds);
        Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal, Guid roleId,
            IReadOnlyCollection<Guid> featureIds);

        Task<ValidationResult> CanAssignLicensedFeatureToTenantAsync(ClaimsPrincipal principal, Guid tenantId,
            IReadOnlyCollection<Guid> licensedFeatureIds);

        Task<ValidationResult> CanCreateFeatureCoDependency(ClaimsPrincipal principal, Guid featureId,
            Guid dependentOnId, bool isAssignment);

        Task<ValidationResult> CanAssignTenantToFeatureAsync(ClaimsPrincipal principal, Guid tenantId,
            Guid featureId);

        Task<ValidationResult> CanUnassignTenantFromFeatureAsync(ClaimsPrincipal principal, Guid tenantId,
            Guid featureId);

    }
}
