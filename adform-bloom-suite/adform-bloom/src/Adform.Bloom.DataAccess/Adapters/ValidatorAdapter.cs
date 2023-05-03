using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Ports;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.SharedKernel.Extensions;
using Microsoft.Extensions.Options;
using Neo4jClient.Extensions;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.DataAccess.Adapters
{
    public class ValidatorAdapter :
        IRoleValidator,
        ITenantValidator,
        ISubjectValidator,
        IPolicyValidator,
        IPermissionValidator,
        IFeatureValidator,
        ILicensedFeatureValidator

    {
        private readonly IAdminGraphRepository _graphRepository;
        private readonly IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role> _roleVisibilityProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Subject> _subjectVisibilityProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Permission> _permissionVisibilityProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> _featureVisibilityProvider;
        private readonly IOptions<ValidationConfiguration> _validationConfiguration;

        public ValidatorAdapter(
            IAdminGraphRepository graphRepository,
            IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role> roleVisibilityProvider,
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Subject> subjectVisibilityProvider,
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Permission> permissionVisibilityProvider,
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> featureVisibilityProvider,
            IOptions<ValidationConfiguration> validationConfiguration)
        {
            _graphRepository = graphRepository;
            _roleVisibilityProvider = roleVisibilityProvider;
            _subjectVisibilityProvider = subjectVisibilityProvider;
            _permissionVisibilityProvider = permissionVisibilityProvider;
            _featureVisibilityProvider = featureVisibilityProvider;
            _validationConfiguration = validationConfiguration;
        }

        public async Task<bool> DoesRoleExist(Guid roleId, string? label = null)
        {
            return await _graphRepository.GetCountAsync<Role>(n => n.Id == roleId, label) > 0;
        }

        public async Task<bool> DoRolesExist(IReadOnlyCollection<Guid> roleIds)
        {
            return await _graphRepository.GetCountAsync<Role>(n => n.Id.In(roleIds)) == roleIds.Count;
        }

        public async Task<Guid> GetRoleOwner(Guid roleId)
        {
            var result = await _graphRepository.GetConnectedAsync<Role, Tenant>(
                r => r.Id == roleId,
                Constants.OwnsIncomingLink);

            if (result.Count != 1)
                throw new BloomDataInconsistencyException(
                    $"A role must have exactly 1 owner! The role '{roleId}' has {result.Count}.");

            return result.Single().Id;
        }

        public async Task<bool> DoesTenantExist(Guid tenantId)
        {
            return await _graphRepository.GetCountAsync<Tenant>(n => n.Id == tenantId) > 0;
        }

        public async Task<bool> DoTenantsExist(IReadOnlyCollection<Guid> tenantIds)
        {
            return await _graphRepository.GetCountAsync<Tenant>(n => n.Id.In(tenantIds)) == tenantIds.Count;
        }

        public Task<bool> IsSameSubject(ClaimsPrincipal principal, Guid subjectId)
        {
            return Task.FromResult(principal != null && principal.GetActorId() == subjectId.ToString());
        }

        public async Task<bool> SubjectExists(Guid subjectId)
        {
            return await _graphRepository.GetCountAsync<Subject>(n => n.Id == subjectId) > 0;
        }

        public async Task<bool> DoesPermissionExist(Guid permissionId)
        {
            return await _graphRepository.GetCountAsync<Permission>(n => n.Id == permissionId) > 0;
        }

        public async Task<bool> DoesPolicyExist(Guid policyId)
        {
            return await _graphRepository.GetCountAsync<Policy>(n => n.Id == policyId) > 0;
        }

        public async Task<bool> DoFeaturesExist(IReadOnlyCollection<Guid> featureIds)
        {
            var count = await _graphRepository.GetCountAsync<Feature>(x => x.Id.In(featureIds));
            return count == featureIds.Count;
        }

        public async Task<bool> DoLicensedFeaturesExist(IReadOnlyCollection<Guid> licensedFeatureIds)
        {
            var count = await _graphRepository.GetCountAsync<LicensedFeature>(x => x.Id.In(licensedFeatureIds));
            return count == licensedFeatureIds.Count;
        }

        public Task<bool> HasVisibilityToRoleAsync(ClaimsPrincipal principal, Guid roleId,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return _roleVisibilityProvider.HasVisibilityAsync(principal, new QueryParamsRoles
            {
                ResourceIds = new[] {roleId},
                TenantIds = tenantIds
            });
        }

        public Task<bool> HasVisibilityToRolesAsync(ClaimsPrincipal principal, IReadOnlyCollection<Guid> roleIds,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return _roleVisibilityProvider.HasVisibilityAsync(principal, new QueryParamsRoles
                {
                   ResourceIds = roleIds,
                   TenantIds = tenantIds
                });
        }

        public Task<bool> HasVisibilityToSubjectAsync(ClaimsPrincipal principal, Guid subjectId,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return _subjectVisibilityProvider.HasVisibilityAsync(principal, new QueryParamsTenantIds {
                ResourceIds = new []{subjectId},
                TenantIds = tenantIds
            });
        }

        public async Task<bool> HasEnoughRoleAssignmentCapacityAsync(Guid subjectId, IEnumerable<RoleTenant>? assignments, IEnumerable<RoleTenant>? unassignments)
        {
            if (assignments == null || !assignments.Any()) return true;
            var rolesToAddCount = (assignments?.Count() ?? 0) - (unassignments?.Count() ?? 0);
            var currentCount = (await _graphRepository.GetConnectedAsync<Subject, Group>(s => s.Id == subjectId, Constants.MemberOfLink)).Count;
            return currentCount + rolesToAddCount <= _validationConfiguration.Value.RoleLimitPerSubject;
        }

        public Task<bool> HasVisibilityToPermissionAsync(ClaimsPrincipal principal, Guid permissionId,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return _permissionVisibilityProvider.HasVisibilityAsync(principal,new QueryParamsTenantIds
            {
                ResourceIds = new[] {permissionId},
                TenantIds = tenantIds
            });
        }

        public Task<bool> HasVisibilityToFeaturesAsync(ClaimsPrincipal principal, IReadOnlyCollection<Guid> featureIds,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return _featureVisibilityProvider.HasVisibilityAsync(principal, new QueryParamsTenantIds
            {
                ResourceIds = featureIds,
                TenantIds = tenantIds
            });
        }

        public async Task<bool> CanEditRoleAsync(ClaimsPrincipal principal, Guid roleId)
        {
            if (principal.IsAdformAdmin())
                return true;
            var label = Constants.Label.CUSTOM_ROLE;
            return await _roleVisibilityProvider.HasVisibilityAsync(principal, new QueryParamsRoles
                {
                    ResourceIds = new[] {roleId},
                    TenantIds = new[] {await GetRoleOwner(roleId)}
                }, label);
        }

        public async Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal,
            IReadOnlyCollection<Guid> featureIds,
            IReadOnlyCollection<Guid>? tenantIds = null)
        {
            return await _graphRepository.FilterFeatureIdsWithAccessDeniedAsync(principal, featureIds, tenantIds);
        }

        public Task<bool> IsFeatureDependentOnOtherFeature(Guid featureId, Guid dependentOnId) =>
            _graphRepository.HasRelationshipAsync<Feature, Feature>(d => d.Id == dependentOnId, f => f.Id == featureId,
                Constants.DependsOnRecursiveLink);

        public async Task<bool> CoDependencyFeaturesSelected(IReadOnlyCollection<Guid> featureIds)
        {
            var result = await _graphRepository.GetFeaturesDependenciesAsync(featureIds);
            if (!result.Any())
                return true;
            foreach (var feature in featureIds)
            {
                var item = result.FirstOrDefault(p => p.Id == feature);
                if (item == null) continue;
                if (item.Dependencies.Count > 0 && !item.Dependencies.All(x => featureIds.Any(y => x == y)))
                    return false;
            }

            return true;
        }

        public Task<bool> IsTenantAssignedToFeatureCoDependenciesAsync(Guid tenantId, Guid featureId)
            => _graphRepository.IsTenantAssignedToFeatureCoDependenciesAsync(tenantId, featureId);

        public Task<bool> IsTenantAssignedToFeatureWithoutDependants(Guid tenantId, Guid featureId)
            => _graphRepository.IsTenantAssignedToFeatureWithoutDependants(tenantId, featureId);
    }
}