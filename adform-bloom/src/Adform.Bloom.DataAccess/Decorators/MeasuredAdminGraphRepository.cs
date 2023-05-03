using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Extensions;
using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Decorators;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Decorators
{
    public class MeasuredAdminGraphRepository : MeasuredGraphRepository, IAdminGraphRepository
    {
        private readonly IAdminGraphRepository _inner;
        private readonly ICustomHistogram _histogram;

        public MeasuredAdminGraphRepository(IAdminGraphRepository inner, ICustomHistogram histogram)
            : base(inner, histogram)
        {
            _inner = inner;
            _histogram = histogram;
        }

        public Task<IReadOnlyCollection<TEntity>> GetByIdsAsync<TEntity>(IReadOnlyCollection<Guid> list) =>
            _histogram.MeasureAsync(() => _inner.GetByIdsAsync<TEntity>(list), "GetByIdsAsync");

        public Task<EntityPagination<TEntity>> SearchPaginationAsync<TEntity>(
            Expression<Func<TEntity, bool>> expression, int skip = 0, int limit = 100, string? label = default,
            string? notLabel = default, QueryParams? queryParams = null)
            where TEntity : class =>
            _histogram.MeasureAsync(
                () => _inner.SearchPaginationAsync(expression, skip, limit, label, notLabel, queryParams),
                "SearchPaginationAsync");

        public Task<IReadOnlyList<TChild>> GetConnectedWithIntermediateAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            ILink linkParentToIntermediate, ILink linkIntermediateToChild) =>
            _histogram.MeasureAsync(
                () => _inner.GetConnectedWithIntermediateAsync<TParent, TIntermediate, TChild>(startNodeExpression,
                    linkParentToIntermediate, linkIntermediateToChild), "GetConnectedWithIntermediateAsync");

        public Task<bool> HasRelationshipAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            Expression<Func<TChild, bool>> targetNodeExpression, ILink linkParentToIntermediate,
            ILink linkIntermediateToChild) => _histogram.MeasureAsync(() =>
            _inner.HasRelationshipAsync<TParent, TIntermediate, TChild>(startNodeExpression, targetNodeExpression,
                linkParentToIntermediate, linkIntermediateToChild), "HasRelationshipAsync");

        public Task BulkLazyCreateGroupAsync(Guid subjectId, IEnumerable<RoleTenant> assignments) =>
            _histogram.MeasureAsync(() => _inner.BulkLazyCreateGroupAsync(subjectId, assignments),
                "BulkLazyCreateGroupAsync");

        public Task DeletePermissionAssignmentsForDeassignedFeatureAsync(Guid featureId, Guid tenantId) =>
            _histogram.MeasureAsync(
                () => _inner.DeletePermissionAssignmentsForDeassignedFeatureAsync(featureId, tenantId),
                "DeletePermissionAssignmentsForDeassignedFeatureAsync");

        public Task DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(Guid permissionId) =>
            _histogram.MeasureAsync(
                () => _inner.DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(permissionId),
                "DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync");

        public Task<IReadOnlyList<Guid>> AssignPermissionsToRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds) =>
            _histogram.MeasureAsync(() => _inner.AssignPermissionsToRoleThroughFeaturesAsync(roleId, featureIds),
                "AssignPermissionsToRoleThroughFeaturesAsync");

        public Task UnassignPermissionsFromRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds) =>
            _histogram.MeasureAsync(() => _inner.UnassignPermissionsFromRoleThroughFeaturesAsync(roleId, featureIds),
                "UnassignPermissionsFromRoleThroughFeaturesAsync");

        public Task AssignLicensedFeaturesToTenantAsync(Guid tenantId, IReadOnlyCollection<Guid> licensedFeaturesIds)
            => _histogram.MeasureAsync(
                () => _inner.AssignLicensedFeaturesToTenantAsync(tenantId, licensedFeaturesIds),
                "AssignLicensedFeaturesToTenantAsync");

        public Task UnassignLicensedFeaturesFromTenantAsync(Guid tenantId,
            IReadOnlyCollection<Guid> licensedFeaturesIds)
            => _histogram.MeasureAsync(() =>
                _inner.UnassignLicensedFeaturesFromTenantAsync(tenantId, licensedFeaturesIds), 
                "UnassignLicensedFeaturesFromTenantAsync");

        public Task<IReadOnlyList<Dependency>> GetFeaturesDependenciesAsync(IReadOnlyCollection<Guid> featureIds) =>
            _histogram.MeasureAsync(() => _inner.GetFeaturesDependenciesAsync(featureIds),
                "GetFeaturesDependenciesAsync");

        public Task BulkUnassignSubjectFromRolesAsync(Guid subjectId, IEnumerable<RoleTenant> assignments) =>
            _histogram.MeasureAsync(() => _inner.BulkUnassignSubjectFromRolesAsync(subjectId, assignments),
                "BulkUnassignSubjectFromRolesAsync");

        public Task<bool> IsTenantAssignedToFeatureCoDependenciesAsync(Guid tenantId, Guid featureId) =>
            _histogram.MeasureAsync(() => _inner.IsTenantAssignedToFeatureCoDependenciesAsync(tenantId, featureId),
                "IsTenantAssignedToFeatureCoDependenciesAsync");

        public Task<bool> IsTenantAssignedToFeatureWithoutDependants(Guid tenantId, Guid featureId) =>
            _histogram.MeasureAsync(() => _inner.IsTenantAssignedToFeatureWithoutDependants(tenantId, featureId),
                "IsTenantNotAssignedToFeatureDependants");

        public Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughFeatureAssignmentAsync(Guid featureId, IReadOnlyCollection<Guid> permissionIds) =>
            _histogram.MeasureAsync(() => _inner.AssignPermissionsToRolesThroughFeatureAssignmentAsync(featureId, permissionIds),
                "AssignPermissionsToRolesThroughFeatureAssignmentAsync");

        public Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds,
            Guid tenantId, IReadOnlyCollection<string>? roleLabels = null) => _histogram.MeasureAsync(() => _inner.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(licensedFeatureIds, tenantId, roleLabels),
            "AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync");
        public Task UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(Guid featureId, IReadOnlyCollection<Guid> permissionIds) =>
            _histogram.MeasureAsync(() => _inner.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(featureId, permissionIds),
                "UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync");

        public Task UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds,
            Guid tenantId, IReadOnlyCollection<string>? roleLabels = null) => _histogram.MeasureAsync(() => _inner.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(licensedFeatureIds, tenantId, roleLabels),
            "UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync");

        public Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal,
            IReadOnlyCollection<Guid> featureIds, IReadOnlyCollection<Guid>? tenantIds = null) =>
            _histogram.MeasureAsync(() => _inner.FilterFeatureIdsWithAccessDeniedAsync(principal, featureIds, tenantIds),
                "FilterFeatureIdsWithAccessDeniedAsync");

        public Task<Dictionary<string, int>> GetStats() => _inner.GetStats();
    }
}