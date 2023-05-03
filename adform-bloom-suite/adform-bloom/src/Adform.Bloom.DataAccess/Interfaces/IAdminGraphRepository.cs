using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.DataAccess.Interfaces
{
    public interface IAdminGraphRepository : IGraphRepository
    {
        Task<IReadOnlyCollection<TEntity>> GetByIdsAsync<TEntity>(IReadOnlyCollection<Guid> ids);

        Task<EntityPagination<TEntity>> SearchPaginationAsync<TEntity>(Expression<Func<TEntity, bool>> expression,
            int skip = 0, int limit = 100, string? label = default, string? notLabel = default,
            QueryParams? queryParams = null)
            where TEntity : class;

        Task<IReadOnlyList<TChild>> GetConnectedWithIntermediateAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild);

        Task<bool> HasRelationshipAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            Expression<Func<TChild, bool>> targetNodeExpression,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild);

        Task BulkLazyCreateGroupAsync(Guid subjectId, IEnumerable<RoleTenant> assignments);

        Task DeletePermissionAssignmentsForDeassignedFeatureAsync(Guid featureId, Guid tenantId);
        Task DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(Guid permissionId);

        Task<IReadOnlyList<Guid>> AssignPermissionsToRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds);

        Task UnassignPermissionsFromRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds);

        Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughFeatureAssignmentAsync(Guid featureId,
            IReadOnlyCollection<Guid> permissionIds);

        Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds, Guid tenantId, IReadOnlyCollection<string>? roleLabels = null);

        Task UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(Guid featureId,
            IReadOnlyCollection<Guid> permissionIds);

        Task UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds, Guid tenantId, IReadOnlyCollection<string>? roleLabels = null);

        Task AssignLicensedFeaturesToTenantAsync(Guid tenantId,
            IReadOnlyCollection<Guid> licensedFeaturesIds);
        
        Task UnassignLicensedFeaturesFromTenantAsync(Guid tenantId,
            IReadOnlyCollection<Guid> licensedFeaturesIds);

        Task<IReadOnlyList<Dependency>> GetFeaturesDependenciesAsync(IReadOnlyCollection<Guid> featureIds);

        Task BulkUnassignSubjectFromRolesAsync(Guid subjectId, IEnumerable<RoleTenant> assignments);

        Task<bool> IsTenantAssignedToFeatureCoDependenciesAsync(Guid tenantId, Guid featureId);

        Task<bool> IsTenantAssignedToFeatureWithoutDependants(Guid tenantId, Guid featureId);
        Task<Dictionary<string, int>> GetStats();

        Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal principal,
            IReadOnlyCollection<Guid> featureIds, IReadOnlyCollection<Guid>? tenantIds = null);
    }
}