using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.Ports;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Domain.Validations
{
    public class AccessValidator : IAccessValidator
    {
        private readonly IRoleValidator _roleValidator;
        private readonly ISubjectValidator _subjectValidator;
        private readonly ITenantValidator _tenantValidator;
        private readonly IPolicyValidator _policyValidator;
        private readonly IPermissionValidator _permissionValidator;
        private readonly IFeatureValidator _featureValidator;
        private readonly ILicensedFeatureValidator _licensedFeatureValidator;

        public AccessValidator(
            IRoleValidator roleValidator,
            ISubjectValidator subjectValidator,
            ITenantValidator tenantValidator,
            IPolicyValidator policyValidator,
            IPermissionValidator permissionValidator,
            IFeatureValidator featureValidator,
            ILicensedFeatureValidator licensedFeatureValidator
        )
        {
            _roleValidator = roleValidator;
            _subjectValidator = subjectValidator;
            _tenantValidator = tenantValidator;
            _policyValidator = policyValidator;
            _permissionValidator = permissionValidator;
            _featureValidator = featureValidator;
            _licensedFeatureValidator = licensedFeatureValidator;
        }

        public async Task<ValidationResult> CanDeleteRoleAsync(ClaimsPrincipal principal, Guid roleId)
        {
            var res = new ValidationResult();
            if (!await _roleValidator.DoesRoleExist(roleId))
                res.SetError(ErrorCodes.RoleDoesNotExist);
            else if (!await _roleValidator.CanEditRoleAsync(principal, roleId))
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            return res;
        }

        public async Task<bool> CanCreateSubjectAsync(ClaimsPrincipal principal, Guid subjectId)
        {
            // Context-less validation - no tenant id is needed to delete.
            return !(await _subjectValidator.SubjectExists(subjectId));
        }

        public async Task<bool> CanDeleteSubjectAsync(ClaimsPrincipal principal, Guid subjectId)
        {
            // Context-less validation - no tenant id is needed to delete.
            if (await _subjectValidator.IsSameSubject(principal, subjectId))
                return false;
            return await _subjectValidator.HasVisibilityToSubjectAsync(principal, subjectId);
        }

        public async Task<ValidationResult> CanAssignPermissionToRoleAsync(ClaimsPrincipal principal,
            Guid permissionId, Guid roleId)
        {
            var res = new ValidationResult();

            if (!await _permissionValidator.DoesPermissionExist(permissionId))
                res.SetError(ErrorCodes.PermissionDoesNotExist);

            if (!await _roleValidator.DoesRoleExist(roleId))
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (principal.IsAdformAdmin())
                return res;

            var tenantId = await _roleValidator.GetRoleOwner(roleId);
            
            if (!await _permissionValidator.HasVisibilityToPermissionAsync(
                principal, permissionId, new[] {tenantId}))
                res.SetError(ErrorCodes.SubjectCannotAccessPermission);

            if (!await _roleValidator.CanEditRoleAsync(
                principal, roleId))
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            return res;
        }

        public async Task<ValidationResult> CanCreateRole(ClaimsPrincipal principal, Guid? policyId, Guid tenantId, IReadOnlyCollection<Guid>? featureIds, bool isTemplateRole)
        {
            var res = new ValidationResult();

            if (!policyId.HasValue || !await _policyValidator.DoesPolicyExist(policyId.Value))
                res.SetError(ErrorCodes.PolicyDoesNotExist);

            if (!await _tenantValidator.DoesTenantExist(tenantId))
                res.SetError(ErrorCodes.TenantDoesNotExist);

            if (isTemplateRole && !principal.IsAdformAdmin())
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            if (!principal.IsAdformAdmin() && !principal.GetTenants(limitTo: new []{tenantId}).Any())
                res.SetError(ErrorCodes.SubjectCannotAccessTenant);

            if (!(featureIds is null))
            {
                if (!await _featureValidator.DoFeaturesExist(featureIds))
                    res.SetError(ErrorCodes.FeaturesDoNotExist);

                if (!await _featureValidator.HasVisibilityToFeaturesAsync(principal, featureIds, new[] {tenantId}))
                    res.SetError(ErrorCodes.SubjectCannotAccessFeatures);

                if (!await _featureValidator.CoDependencyFeaturesSelected(featureIds))
                    res.SetError(ErrorCodes.FeatureDependencyMissing);
            }

            return res;
        }

        public async Task<ValidationResult> CanUpdateRole(ClaimsPrincipal principal, Guid roleId)
        {
            var res = new ValidationResult();
            var roleExist = await _roleValidator.DoesRoleExist(roleId);
            if (!roleExist)
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (!await _roleValidator.CanEditRoleAsync(principal, roleId))
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            return res;
        }

        public async Task<ValidationResult> CanAssignSubjectToRolesAsync(ClaimsPrincipal principal,
            IEnumerable<RoleTenant> assignments,
            IEnumerable<RoleTenant>? filteredAssignments,
            IEnumerable<RoleTenant>? filteredUnassignments, Guid subjectId)
        {
            var res = new ValidationResult();
            var tenants = assignments.Select(x => x.TenantId).Distinct().ToArray();
            var roles = assignments.Select(x => x.RoleId).Distinct().ToArray();

            if (!await _tenantValidator.DoTenantsExist(tenants))
                res.SetError(ErrorCodes.TenantDoesNotExist);

            if (!await _subjectValidator.SubjectExists(subjectId))
                res.SetError(ErrorCodes.SubjectDoesNotExist);

            if (await _subjectValidator.IsSameSubject(principal, subjectId))
                res.SetError(ErrorCodes.SubjectCannotModifyAssignmentsForHimself);

            if (!await _roleValidator.DoRolesExist(roles))
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (!principal.IsAdformAdmin() && principal.GetTenants(limitTo: tenants).Count < tenants.Length)
                res.SetError(ErrorCodes.SubjectCannotAccessTenant);

            if (!await _roleValidator.HasVisibilityToRolesAsync(principal, roles, principal.IsAdformAdmin() ? null : tenants))
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            if (!await _subjectValidator.HasEnoughRoleAssignmentCapacityAsync(subjectId, filteredAssignments, filteredUnassignments))
                res.SetError(ErrorCodes.SubjectCannotExceedRoleAssignmentLimit);

            return res;
        }

        public async Task<ValidationResult> CanUnassignSubjectFromRolesAsync(ClaimsPrincipal principal, IEnumerable<RoleTenant> assignments, Guid subjectId)
        {
            var res = await CanAssignSubjectToRolesAsync(principal, assignments, null, assignments, subjectId);

            // 1. If we make this validation for an assign operation, it will always fail for subjects that have no roles
            // for a given tenant.
            if (!await _subjectValidator.HasVisibilityToSubjectAsync(principal, subjectId,
                assignments.Select(x => x.TenantId).ToArray()))
                res.SetError(ErrorCodes.SubjectCannotAccessSubject);

            return res;
        }

        public async Task<ValidationResult> CanAssignRoleToFeaturesAsync(ClaimsPrincipal principal,
            Guid roleId, IReadOnlyCollection<Guid> featureIds)
        {
            var result = new ValidationResult();

            if (!await _roleValidator.DoesRoleExist(roleId))
            {
                result.SetError(ErrorCodes.RoleDoesNotExist);
            }

            var tenantId = await _roleValidator.GetRoleOwner(roleId);

            if (!await _roleValidator.CanEditRoleAsync(principal, roleId))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessRole);
            }

            if (!await _featureValidator.HasVisibilityToFeaturesAsync(principal, featureIds, new[] {tenantId}))
            { 
                result.SetError(ErrorCodes.SubjectCannotAccessFeatures);
            }
            return result;
        }
        public async Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal, Guid roleId, IReadOnlyCollection<Guid> featureIds)
        {
            var tenantId = await _roleValidator.GetRoleOwner(roleId);
            return await _featureValidator.FilterFeatureIdsWithAccessDeniedAsync(principal, featureIds, new[] {tenantId});
        }

        public async Task<ValidationResult> CanAssignLicensedFeatureToTenantAsync(ClaimsPrincipal principal,
            Guid tenantId, IReadOnlyCollection<Guid> licensedFeatureIds)
        {
            var result = new ValidationResult();

            if (!await _tenantValidator.DoTenantsExist(new Guid[] {tenantId}))
            {
                result.SetError(ErrorCodes.TenantDoesNotExist);
            }

            if (!await _licensedFeatureValidator.DoLicensedFeaturesExist(licensedFeatureIds))
            {
                result.SetError(ErrorCodes.LicensedFeaturesDoNotExist);
            }

            if (!principal.IsAdformAdmin() && !principal.GetTenants().Contains(tenantId.ToString()))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessTenant);
            }

            return result;
        }

        public async Task<ValidationResult> CanCreateFeatureCoDependency(ClaimsPrincipal principal, Guid featureId,
            Guid dependentOnId, bool isAssignment)
        {
            var result = new ValidationResult();

            if (!await _featureValidator.DoFeaturesExist(new[] {featureId}) ||
                !await _featureValidator.DoFeaturesExist(new[] {dependentOnId}))
            {
                result.SetError(ErrorCodes.FeaturesDoNotExist);
            }

            if (!await _featureValidator.HasVisibilityToFeaturesAsync(principal, new[] {featureId}))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessFeatures);
            }

            if (!await _featureValidator.HasVisibilityToFeaturesAsync(principal, new[] {dependentOnId}))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessFeatures);
            }

            if (isAssignment && await _featureValidator.IsFeatureDependentOnOtherFeature(featureId, dependentOnId))
            {
                result.SetError(ErrorCodes.CircularDependency);
            }

            return result;
        }

        public async Task<ValidationResult> CanAssignTenantToFeatureAsync(ClaimsPrincipal principal, Guid tenantId,
            Guid featureId)
        {
            var result = new ValidationResult();

            if (!await _tenantValidator.DoesTenantExist(tenantId))
            {
                result.SetError(ErrorCodes.TenantDoesNotExist);
            }

            if (!await _featureValidator.DoFeaturesExist(new[] {featureId}))
            {
                result.SetError(ErrorCodes.FeaturesDoNotExist);
            }

            if (!principal.IsAdformAdmin() && !principal.GetTenants().Contains(tenantId.ToString()))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessTenant);
            }

            if (!await _featureValidator.IsTenantAssignedToFeatureCoDependenciesAsync(tenantId, featureId))
            {
                result.SetError(ErrorCodes.TenantFeatureCoDependenciesConflict);
            }

            return result;
        }

        public async Task<ValidationResult> CanUnassignTenantFromFeatureAsync(ClaimsPrincipal principal, Guid tenantId,
            Guid featureId)
        {
            var result = new ValidationResult();

            if (!await _tenantValidator.DoesTenantExist(tenantId))
            {
                result.SetError(ErrorCodes.TenantDoesNotExist);
            }

            if (!await _featureValidator.DoFeaturesExist(new[] {featureId}))
            {
                result.SetError(ErrorCodes.FeaturesDoNotExist);
            }

            if (!principal.IsAdformAdmin() && !principal.GetTenants().Contains(tenantId.ToString()))
            {
                result.SetError(ErrorCodes.SubjectCannotAccessTenant);
            }

            if (!await _featureValidator.IsTenantAssignedToFeatureWithoutDependants(tenantId, featureId))
            {
                result.SetError(ErrorCodes.TenantFeatureCoDependenciesConflict);
            }

            return result;
        }
    }
}