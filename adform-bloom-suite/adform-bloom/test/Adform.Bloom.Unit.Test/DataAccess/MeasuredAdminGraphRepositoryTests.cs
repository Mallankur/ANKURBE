using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Decorators;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.OngDb.Core.Model;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class MeasuredAdminGraphRepositoryTests
    {
        private readonly Mock<ICustomHistogram> _histogramMock = new Mock<ICustomHistogram>();
        private readonly Mock<IAdminGraphRepository> _inner = new Mock<IAdminGraphRepository>();
        private readonly MeasuredAdminGraphRepository _repo;

        public MeasuredAdminGraphRepositoryTests()
        {
            _repo = new MeasuredAdminGraphRepository(_inner.Object, _histogramMock.Object);
        }

        [Fact]
        public async Task Repo_Invokes_GetByIdsAsync()
        {
            await _repo.GetByIdsAsync<Entity>(new List<Guid>());
            _inner.Verify(
                m => m.GetByIdsAsync<Entity>(It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_AssignPermissionsToRoleThroughFeaturesAsync()
        {
            await _repo.AssignPermissionsToRoleThroughFeaturesAsync(Guid.NewGuid(), new List<Guid>());
            _inner.Verify(
                m => m.AssignPermissionsToRoleThroughFeaturesAsync(It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_DeletePermissionAssignmentsForDeassignedFeatureAsync()
        {
            await _repo.DeletePermissionAssignmentsForDeassignedFeatureAsync(Guid.NewGuid(), Guid.NewGuid());
            _inner.Verify(
                m => m.DeletePermissionAssignmentsForDeassignedFeatureAsync(It.IsAny<Guid>(),
                    It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync()
        {
            await _repo.DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(Guid.NewGuid());
            _inner.Verify(
                m => m.DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_SearchPaginationAsync()
        {
            await _repo.SearchPaginationAsync<Entity>(x => true);
            _inner.Verify(
                m => m.SearchPaginationAsync(It.IsAny<Expression<Func<Entity, bool>>>(), 0, 100, null, null, null),
                Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_GetConnectedWithIntermediateAsync()
        {
            await _repo.GetConnectedWithIntermediateAsync<Entity, Intermediate, Child>(x => true, new Link(),
                new Link());
            _inner.Verify(
                m => m.GetConnectedWithIntermediateAsync<Entity, Intermediate, Child>(
                    It.IsAny<Expression<Func<Entity, bool>>>(), It.IsAny<Link>(), It.IsAny<Link>()),
                Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_HasRelationshipAsync()
        {
            await _repo.HasRelationshipAsync<Entity, Intermediate, Child>(x => true, x => true, new Link(), new Link());
            _inner.Verify(
                m => m.HasRelationshipAsync<Entity, Intermediate, Child>(It.IsAny<Expression<Func<Entity, bool>>>(),
                    It.IsAny<Expression<Func<Child, bool>>>(), It.IsAny<Link>(), It.IsAny<Link>()),
                Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_UnassignPermissionsToRoleThroughFeaturesAsync()
        {
            await _repo.UnassignPermissionsFromRoleThroughFeaturesAsync(Guid.NewGuid(), new List<Guid>());
            _inner.Verify(
                m => m.UnassignPermissionsFromRoleThroughFeaturesAsync(It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_GetFeaturesDependenciesAsync()
        {
            await _repo.GetFeaturesDependenciesAsync(new List<Guid>());
            _inner.Verify(
                m => m.GetFeaturesDependenciesAsync(It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_BulkUnassignSubjectFromRolesAsync()
        {
            await _repo.BulkUnassignSubjectFromRolesAsync(Guid.NewGuid(), new List<RoleTenant>());
            _inner.Verify(
                m => m.BulkUnassignSubjectFromRolesAsync(It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<RoleTenant>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_BulkLazyCreateGroupAsync()
        {
            await _repo.BulkLazyCreateGroupAsync(Guid.NewGuid(), new List<RoleTenant>());
            _inner.Verify(
                m => m.BulkLazyCreateGroupAsync(It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<RoleTenant>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_IsTenantAssignedToFeatureCoDependenciesAsync()
        {
            await _repo.IsTenantAssignedToFeatureCoDependenciesAsync(Guid.NewGuid(), Guid.NewGuid());
            _inner.Verify(
                m => m.IsTenantAssignedToFeatureCoDependenciesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_IsTenantAssignedToFeatureWithoutDependants()
        {
            await _repo.IsTenantAssignedToFeatureWithoutDependants(Guid.NewGuid(), Guid.NewGuid());
            _inner.Verify(
                m => m.IsTenantAssignedToFeatureWithoutDependants(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_UnassignPermissionsToRolesAsync()
        {
            await _repo.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(Guid.NewGuid(), new Guid[] { Guid.NewGuid() });
            _inner.Verify(
                m => m.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_AssignPermissionsToRolesAsync()
        {
            await _repo.AssignPermissionsToRolesThroughFeatureAssignmentAsync(Guid.NewGuid(), new Guid[] { Guid.NewGuid() });
            _inner.Verify(
                m => m.AssignPermissionsToRolesThroughFeatureAssignmentAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_AssignLicensedFeaturesFromTenantAsync()
        {
            await _repo.AssignLicensedFeaturesToTenantAsync(Guid.NewGuid(), new Guid[] { Guid.NewGuid() });
            _inner.Verify(
                m => m.AssignLicensedFeaturesToTenantAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_UnassignLicensedFeaturesFromTenantAsync()
        {
            await _repo.UnassignLicensedFeaturesFromTenantAsync(Guid.NewGuid(), new Guid[] { Guid.NewGuid() });
            _inner.Verify(
                m => m.UnassignLicensedFeaturesFromTenantAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }
        [Fact]
        public async Task Repo_Invokes_FilterFeatureIdsWithAccessDeniedAsync()
        {
            var tenantId = Guid.NewGuid();
            var principal = Common.BuildPrincipal(tenantId.ToString());
            await _repo.FilterFeatureIdsWithAccessDeniedAsync(principal, new Guid[] { Guid.NewGuid() }, new Guid[] { tenantId });
            _inner.Verify(
                m => m.FilterFeatureIdsWithAccessDeniedAsync(principal, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync()
        {
            await _repo.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(new[] { Guid.NewGuid() }, Guid.NewGuid(), new[] { string.Empty });
            _inner.Verify(
                m => m.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task Repo_Invokes_UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync()
        {
            await _repo.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(new[] { Guid.NewGuid() }, Guid.NewGuid(), new []{string.Empty});
            _inner.Verify(
                m => m.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        }

        class Entity
        {
        }

        class Child
        {
        }

        class Intermediate
        {
        }
    }
}