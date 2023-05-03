using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Integration.Test.VisibilityProviders;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Repositories
{
    [Collection(nameof(RepositoriesCollection))]
    public class AdminGraphRepositoryTests
    {
        private readonly AdminGraphRepository _repository;

        public AdminGraphRepositoryTests(TestsFixture fixture)
        {
            _repository = new AdminGraphRepository(fixture.GraphClient);
        }

        [Fact, Order(0)]
        public async Task GetByIdsAsync_Return_Entities()
        {
            // Arrange
            var features = await _repository.GetNodesAsync<Feature>(o => true);
            var count = 3;
            var featureIds = features.Take(count).OrderBy(o=>o.Id).Select(i => i.Id).ToList();
            // Act
            var result = await _repository.GetByIdsAsync<Feature>(featureIds);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(count, result.Count);
            var item = result.OrderBy(o => o.Id).Select(i => i.Id).ToList();
            Assert.True(item.SequenceEqual(featureIds));
        }


        [Fact, Order(1)]
        public async Task BulkLazyCreateGroupAsync_Assigns_New_Roles()
        {
            // Arrange
            var sub = await _repository.CreateNodeAsync(new Subject());
            var roles = await Task.WhenAll(
                _repository.CreateNodeAsync(new Role("BulkLazyCreateGroupAsync_Assigns_Roles_1")),
                _repository.CreateNodeAsync(new Role("BulkLazyCreateGroupAsync_Assigns_Roles_2")));
            var tenants = await _repository.GetNodesAsync<Tenant>(x =>
                x.Id == Guid.Parse(Graph.Tenant8) || x.Id == Guid.Parse(Graph.Tenant6));
            var assignments = roles.Select((x, i) => new RoleTenant
            {
                RoleId = x.Id,
                TenantId = tenants.Skip(i).First().Id
            });

            // Act
            await _repository.BulkLazyCreateGroupAsync(sub.Id, assignments);

            // Assert
            var tenantsConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Tenant>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.BelongsLink);
            Assert.True(tenantsConnected.All(x => tenants.Select(y => x.Id).Contains(x.Id)));
            var rolesConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink);
            Assert.True(rolesConnected.All(x => roles.Select(y => x.Id).Contains(x.Id)));
        }

        [Fact, Order(1)]
        public async Task BulkLazyCreateGroupAsync_Assigns_ExistingRoles_Roles()
        {
            // Arrange
            var sub = await _repository.CreateNodeAsync(
                new Subject());
            var roles = await _repository.GetNodesAsync<Role>(x =>
                x.Id == Guid.Parse(Graph.CustomRole9) || x.Id == Guid.Parse(Graph.CustomRole8));
            var tenants = await _repository.GetNodesAsync<Tenant>(x =>
                x.Id == Guid.Parse(Graph.Tenant0) || x.Id == Guid.Parse(Graph.Tenant7));
            var assignments = roles.Select((x, i) => new RoleTenant
            {
                RoleId = x.Id,
                TenantId = tenants.Skip(i).First().Id
            });

            // Act
            await _repository.BulkLazyCreateGroupAsync(sub.Id, assignments);

            // Assert
            var tenantsConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Tenant>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.BelongsLink);
            Assert.True(tenantsConnected.All(x => tenants.Select(y => x.Id).Contains(x.Id)));
            var rolesConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink);
            Assert.True(rolesConnected.All(x => roles.Select(y => x.Id).Contains(x.Id)));
        }

        [Fact, Order(1)]
        public async Task BulkLazyCreateGroupAsync_Assigns_InheritedRoles_Roles()
        {
            // Arrange
            var sub = await _repository.CreateNodeAsync(
                new Subject());
            var roles = await _repository.GetNodesAsync<Role>(x =>
                x.Id == Guid.Parse(Graph.CustomRole14) || x.Id == Guid.Parse(Graph.CustomRole15));
            var tenants = await _repository.GetNodesAsync<Tenant>(x =>
                x.Id == Guid.Parse(Graph.Tenant1) || x.Id == Guid.Parse(Graph.Tenant8));
            var assignments = roles.Select((x, i) => new RoleTenant
            {
                RoleId = x.Id,
                TenantId = tenants.Skip(i).First().Id
            });

            // Act
            await _repository.BulkLazyCreateGroupAsync(sub.Id, assignments);

            // Assert
            var tenantsConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Tenant>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.BelongsLink);
            Assert.True(tenantsConnected.All(x => tenants.Select(y => x.Id).Contains(x.Id)));
            var rolesConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink);
            Assert.True(rolesConnected.All(x => roles.Select(y => x.Id).Contains(x.Id)));
        }

        [Fact, Order(1)]
        public async Task AssignPermissionsFromRoleThroughFeaturesAsync_Assigns_Permissions()
        {
            // Arrange
            var role = await _repository.CreateNodeAsync(new Role("AssignPermissionsToRoleThroughFeaturesAsync"));
            var features = await _repository.GetConnectedAsync<Permission, Feature>(x => true,
                Constants.ContainsIncomingLink);

            // Act
            await _repository.AssignPermissionsToRoleThroughFeaturesAsync(role.Id,
                features.Select(x => x.Id).ToArray());

            // Assert
            var permissionsAssigned =
                await _repository.GetConnectedAsync<Role, Permission>(x => x.Id == role.Id, Constants.ContainsLink);
            Assert.NotEmpty(permissionsAssigned);
        }

        [Fact, Order(1)]
        public async Task UnassignPermissionsFromRoleThroughFeaturesAsync_Unassigns_Permissions()
        {
            // Arrange
            var role = await _repository.CreateNodeAsync(new Role("UnassignPermissionsFromRoleThroughFeaturesAsync"));
            var features = await _repository.GetConnectedAsync<Permission, Feature>(x => true,
                Constants.ContainsIncomingLink);
            await _repository.AssignPermissionsToRoleThroughFeaturesAsync(role.Id,
                features.Select(x => x.Id).ToArray());
            var permissionsAssigned =
                await _repository.GetConnectedAsync<Role, Permission>(x => x.Id == role.Id, Constants.ContainsLink);
            Assert.NotEmpty(permissionsAssigned);

            // Act
            await _repository.UnassignPermissionsFromRoleThroughFeaturesAsync(role.Id,
                features.Select(x => x.Id).ToArray());

            // Assert
            permissionsAssigned =
                await _repository.GetConnectedAsync<Role, Permission>(x => x.Id == role.Id, Constants.ContainsLink);
            Assert.Empty(permissionsAssigned);
        }

        [Fact, Order(1)]
        public async Task AssignAndUnassignLicensedFeaturesToTenantAsync_Unassigns_Features()
        {
            // Arrange
            var tenant = await _repository.CreateNodeAsync(new Tenant("AssignLicensedFeaturesToTenantAsync"));
            var licensedFeatures = await _repository.GetNodesAsync<LicensedFeature>(o => true, 0, 1);


            // Act
            await _repository.AssignLicensedFeaturesToTenantAsync(tenant.Id,
                licensedFeatures.Select(p => p.Id).ToList());
            // Assert
            var licensedFeaturesAssigned =
                await _repository.GetConnectedAsync<Tenant, LicensedFeature>(x => x.Id == tenant.Id, Constants.AssignedLink);
            Assert.NotEmpty(licensedFeaturesAssigned);
            Assert.Equal(licensedFeatures.Count, licensedFeaturesAssigned.Count);
            Assert.True(licensedFeaturesAssigned.OrderBy(o=>o.Id).Select(o => o.Id)
                .SequenceEqual(licensedFeatures.OrderBy(o => o.Id).Select(o => o.Id)));

            // Act
            await _repository.UnassignLicensedFeaturesFromTenantAsync(tenant.Id,
                licensedFeatures.Select(x => x.Id).ToArray());

            // Assert
            var licensedFeaturesUnassigned =
                await _repository.GetConnectedAsync<Tenant, LicensedFeature>(x => x.Id == tenant.Id, Constants.AssignedLink);
            Assert.Empty(licensedFeaturesUnassigned);
        }

        [Fact, Order(0)]
        public async Task GetFeaturesDependenciesAsync_Return_Dependencies()
        {
            // Arrange
            var features = await _repository.GetNodesAsync<Feature>(o => true);
            var featureIds = features.Select(i => i.Id).ToList();
            // Act
            var result = await _repository.GetFeaturesDependenciesAsync(featureIds);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(9, result.Count);
            var item = result.First(o => o.Id == Guid.Parse(Graph.Feature8));
            Assert.Equal(Guid.Parse(Graph.Feature8), item.Id);
            Assert.Contains(Guid.Parse(Graph.Feature7), item.Dependencies);
        }

        [Fact, Order(100)]
        public async Task GetFeaturesDependenciesAsync_Return_NestedDependencies()
        {
            // Arrange
            await _repository.CreateRelationshipAsync<Tenant, Feature>(f => f.Id == Guid.Parse(Graph.Tenant6),
                d => d.Id == Guid.Parse(Graph.Feature8), Constants.AssignedLink);
            var features = await _repository.GetNodesAsync<Feature>(x =>
                x.Id == Guid.Parse(Graph.Feature4) || x.Id == Guid.Parse(Graph.Feature5) ||
                x.Id == Guid.Parse(Graph.Feature8));
            await _repository.CreateRelationshipAsync<Feature, Feature>(f => f.Id == Guid.Parse(Graph.Feature4),
                d => d.Id == Guid.Parse(Graph.Feature5), Constants.DependsOnLink);
            await _repository.CreateRelationshipAsync<Feature, Feature>(f => f.Id == Guid.Parse(Graph.Feature5),
                d => d.Id == Guid.Parse(Graph.Feature8), Constants.DependsOnLink);

            var featureIds = features.Select(i => i.Id).ToList();
            // Act
            var result = await _repository.GetFeaturesDependenciesAsync(featureIds);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);
            var item = result.First(o => o.Id == Guid.Parse(Graph.Feature4));
            Assert.Equal(Guid.Parse(Graph.Feature4), item.Id);
            Assert.Contains(Guid.Parse(Graph.Feature5), item.Dependencies);
            Assert.Contains(Guid.Parse(Graph.Feature8), item.Dependencies);
        }

        [Fact, Order(1)]
        public async Task BulkUnassignSubjectFromRolesAsync_Unassigns_Subject_From_Roles()
        {
            // Arrange
            var sub = await _repository.CreateNodeAsync(new Subject());
            var r1 = await _repository.CreateNodeAsync(new Role("r1"));
            var r2 = await _repository.CreateNodeAsync(new Role("r2"));
            await _repository.BulkLazyCreateGroupAsync(sub.Id, new[]
            {
                new RoleTenant
                {
                    RoleId = r1.Id,
                    TenantId = Guid.Parse(Graph.Tenant8)
                },
                new RoleTenant
                {
                    RoleId = r2.Id,
                    TenantId = Guid.Parse(Graph.Tenant7)
                }
            });
            var rolesConnected = (await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink)).Select(x => x.Id);
            Assert.Contains(r1.Id, rolesConnected);
            Assert.Contains(r2.Id, rolesConnected);

            // Act
            await _repository.BulkUnassignSubjectFromRolesAsync(sub.Id, new[]
            {
                new RoleTenant
                {
                    RoleId = r1.Id,
                    TenantId = Guid.Parse(Graph.Tenant8)
                },
                new RoleTenant
                {
                    RoleId = r2.Id,
                    TenantId = Guid.Parse(Graph.Tenant7)
                }
            });

            // Assert
            rolesConnected = (await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink)).Select(x => x.Id);
            Assert.Empty(rolesConnected);
        }

        [Fact, Order(1)]
        public async Task BulkUnassignSubjectFromRolesAsync_Unassigns_Subject_From_InheritedRoles()
        {
            // Arrange
            var sub = await _repository.CreateNodeAsync(
                new Subject());
            var roles = await _repository.GetNodesAsync<Role>(x =>
                x.Id == Guid.Parse(Graph.CustomRole14) || x.Id == Guid.Parse(Graph.CustomRole15));
            var tenants = await _repository.GetNodesAsync<Tenant>(x =>
                x.Id == Guid.Parse(Graph.Tenant1) || x.Id == Guid.Parse(Graph.Tenant8));
            var assignments = roles.Select((x, i) => new RoleTenant
            {
                RoleId = x.Id,
                TenantId = tenants.Skip(i).First().Id
            });
            await _repository.BulkLazyCreateGroupAsync(sub.Id, assignments);
            var tenantsConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Tenant>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.BelongsLink);
            Assert.True(tenantsConnected.All(x => tenants.Select(y => x.Id).Contains(x.Id)));
            var rolesConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink);
            Assert.True(rolesConnected.All(x => roles.Select(y => x.Id).Contains(x.Id)));

            // Act
            await _repository.BulkUnassignSubjectFromRolesAsync(sub.Id, assignments);

            // Assert
            rolesConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.AssignedLink);
            tenantsConnected = await _repository.GetConnectedWithIntermediateAsync<Subject, Group, Tenant>(
                x => x.Id == sub.Id,
                Constants.MemberOfLink, Constants.BelongsLink);
            Assert.Empty(rolesConnected);
            Assert.Empty(tenantsConnected);
        }

        [Theory, Order(1)]
        [MemberData(nameof(Scenarios.IsTenantAssignedToFeatureCoDependenciesAsyncInitScenarios),
            MemberType = typeof(Scenarios))]
        public async Task IsTenantAssignedToFeatureCoDependenciesAsync_Returns_Expected_Result(
            Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>> init, bool expectedResult)
        {
            // Arrange
            var (tenantId, featureId) = await init(_repository);

            // Act
            var result = await _repository.IsTenantAssignedToFeatureCoDependenciesAsync(tenantId, featureId);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory, Order(1)]
        [MemberData(nameof(Scenarios.IsTenantAssignedToFeatureWithoutDependantsInitScenarios),
            MemberType = typeof(Scenarios))]
        public async Task IsTenantAssignedToFeatureWithoutDependants_Returns_Expected_Result(
            Func<AdminGraphRepository, Task<(Guid tenantId, Guid featureId)>> init, bool expectedResult)
        {
            // Arrange
            var (tenantId, featureId) = await init(_repository);

            // Act
            var result = await _repository.IsTenantAssignedToFeatureWithoutDependants(tenantId, featureId);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact, Order(1)]
        public async Task AssignPermissionsToRolesAsync_Returns_Expected_Result()
        {
            // Arrange
            var role = await _repository.CreateNodeAsync(new Role());
            var permission0 = await _repository.CreateNodeAsync(new Permission());
            var permission1 = await _repository.CreateNodeAsync(new Permission());
            var feature = await _repository.CreateNodeAsync(new Feature());

            await _repository.CreateRelationshipAsync<Role, Permission>(t => t.Id == role.Id,
                f => f.Id == permission0.Id, Constants.ContainsLink);
            await _repository.CreateRelationshipAsync<Feature, Permission>(t => t.Id == feature.Id,
                f => f.Id == permission0.Id, Constants.ContainsLink);
            await _repository.CreateRelationshipAsync<Feature, Permission>(t => t.Id == feature.Id,
                f => f.Id == permission1.Id, Constants.ContainsLink);
            // Act
            var result = await _repository.AssignPermissionsToRolesThroughFeatureAssignmentAsync(feature.Id, new Guid[] { permission1.Id });

            // Assert
            Assert.True(result.Contains(role.Id));
            var edge = await _repository.HasRelationshipAsync<Role, Permission>(p => p.Id == role.Id, f => f.Id == permission1.Id, Constants.ContainsLink);
            Assert.True(edge);
        }

        [Fact, Order(1)]
        public async Task UnassignPermissionsToRolesAsync_Returns_Expected_Result()
        {
            // Arrange
            var role = await _repository.CreateNodeAsync(new Role());
            var permission0 = await _repository.CreateNodeAsync(new Permission());
            var permission1 = await _repository.CreateNodeAsync(new Permission());
            var feature = await _repository.CreateNodeAsync(new Feature());

            await _repository.CreateRelationshipAsync<Role, Permission>(t => t.Id == role.Id,
                f => f.Id == permission0.Id, Constants.ContainsLink);
            await _repository.CreateRelationshipAsync<Role, Permission>(t => t.Id == role.Id,
                f => f.Id == permission1.Id, Constants.ContainsLink);
            await _repository.CreateRelationshipAsync<Feature, Permission>(t => t.Id == feature.Id,
                f => f.Id == permission0.Id, Constants.ContainsLink);
            await _repository.CreateRelationshipAsync<Feature, Permission>(t => t.Id == feature.Id,
                f => f.Id == permission1.Id, Constants.ContainsLink);
            // Act
            await _repository.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(feature.Id, new Guid[] { permission1.Id });

            // Assert
            var edge = await _repository.HasRelationshipAsync<Role, Permission>(p => p.Id == role.Id, f => f.Id == permission1.Id, Constants.ContainsLink);
            Assert.False(edge);
        }
        
        [Theory]
        [MemberData(nameof(FeatureVisibilityProviderTests.CreateResult), MemberType = typeof(FeatureVisibilityProviderTests))]
        public async Task FilterFeatureIdsWithAccessDeniedAsync_Has_Access_To_Features_InHierarchy(string caseName, 
            ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            if (!features.Any())
                return;

            var random = new Random(Guid.NewGuid().GetHashCode());

            // Arrange
            var index = random.Next(features.Length);
            var feature = features.ToList()[index];

            // Act
            var response = await _repository.FilterFeatureIdsWithAccessDeniedAsync(identity, features.Select(f => f.Id).ToArray(), tenantIds);

            // Assert
            Assert.Equal(0, response.Count);
        }

        [Theory]
        [MemberData(nameof(Scenarios.FilterFeatureIdsWithAccessDeniedFailureScenario), MemberType = typeof(Scenarios))]
        public async Task FilterFeatureIdsWithAccessDeniedAsync_Fails_On_Inaccessible_Features_InHierarchy(string caseName, Scenarios.FilterFeatureIdsWithAccessDeniedScenario s)
        {

            // Act
            var response = await _repository.FilterFeatureIdsWithAccessDeniedAsync(s.Identity,
                s.Features.Select(f => f.Id).ToArray(), s.TenantIds);

            // Assert
            Assert.True(s.Callback(response));
        }

    }
}