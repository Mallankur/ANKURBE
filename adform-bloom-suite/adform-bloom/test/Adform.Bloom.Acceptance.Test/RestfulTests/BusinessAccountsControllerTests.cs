using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.SharedKernel.Entities;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;
using Feature = Adform.Bloom.Domain.Entities.Feature;
using LicensedFeature = Adform.Bloom.Domain.Entities.LicensedFeature;
using Permission = Adform.Bloom.Contracts.Output.Permission;
using Role = Adform.Bloom.Domain.Entities.Role;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestBusinessAccountsCollection))]
    public class BusinessAccountsControllerTests : IClassFixture<TestsFixture>
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public BusinessAccountsControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_BusinessAccount_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsBusinessAccount(),0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/business-accounts/{nodeId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Tenant>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodeId, result.Id);
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_BusinessAccount_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/business-accounts/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_BusinessAccount_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/business-accounts/{Graph.Tenant2}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject6]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_BusinessAccounts_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 0, size);
            var nodesCount = nodes.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/business-accounts?limit={size}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<BusinessAccount>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Theory, Order(0)]
        [InlineData(3, 1)]
        [InlineData(1, 4)]
        [InlineData(2, 2)]
        public async Task Get_BusinessAccounts_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsBusinessAccount(),page, size);
            var nodesCount = nodes.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/business-accounts?limit={size}&offset={page}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Tenant>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_BusinessAccounts_With_ReturnTotalCount_Test()
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 1, 4);
            var totalCount = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsBusinessAccount(),0, 100));
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/business-accounts?limit=4&offset=1");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Contracts.Output.Tenant>>();
            var pagedResult =
                new EntityPagination<Contracts.Output.Tenant>(1, 4, totalCount.Data.Count(), nodes.Data.ToList());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));

            var totalCountHeader = response.Headers.GetValues("total-count").First();
            Assert.Equal(pagedResult.TotalItems, int.Parse(totalCountHeader));
        }

        [Fact]
        public async Task Feature_NotFound()
        {
            // Arrange
            const string tenantId = Graph.Tenant2;
            const string featureId = "885b0e53-e44a-4181-bb79-e8844b655514";

            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/business-accounts/{tenantId}/features/{featureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Permission_NotFound()
        {
            // Arrange
            const string tenantId = "49195836-874c-4db2-b954-a4ee70717eaa";
            const string featureId = Graph.Feature3;

            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/business-accounts/{tenantId}/features/{featureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Unassign_From_LicensedFeature()
        {
            // Arrange
            const string tenantId = Graph.Tenant8;
            const string lFeatureId = Graph.LicensedFeature2;


            const string featureId = Graph.Feature3;
            const string roleId = Graph.CustomRole11;
            const string permissionId = Graph.Permission5;

            const string traffickerId = Graph.Trafficker8Role;

            var featureToPermissionExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Feature, Permission>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            var roleToPermissionExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);

            var traffickerRoleToPermissionUnassignmentExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(traffickerId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);

            Assert.True(featureToPermissionExists);
            Assert.True(roleToPermissionExists);
            Assert.True(traffickerRoleToPermissionUnassignmentExists);


            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"/v1/business-accounts/{tenantId}/licensedFeatures/{lFeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            featureToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Tenant, LicensedFeature>(
                f => f.Id == Guid.Parse(tenantId), p => p.Id == Guid.Parse(lFeatureId), Constants.AssignedLink);
            Assert.False(featureToPermissionExists);

            roleToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            Assert.False(roleToPermissionExists);

            traffickerRoleToPermissionUnassignmentExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(traffickerId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);

            Assert.False(traffickerRoleToPermissionUnassignmentExists);

        }

        [Theory, Order(1)]
        [InlineData(Graph.Tenant7, Graph.Trafficker7Role, true)]
        [InlineData(Graph.Tenant5, Graph.Trafficker5Role, false)]
        public async Task Assign_Feature(string tenantId, string traffickerId, bool hasAccessToTenant)
        {
            // Arrange
            const string lFeatureId = Graph.LicensedFeature3;
            const string permissionId = Graph.Permission9;

            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/business-accounts/{tenantId}/licensedFeatures/{lFeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject4]);
            Assert.False(
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(traffickerId), p => p.Id == Guid.Parse(permissionId),
                    Constants.ContainsLink));

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            var traffickerRelationshipToPermission =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(traffickerId), p => p.Id == Guid.Parse(permissionId),
                    Constants.ContainsLink);
            Assert.Equal(hasAccessToTenant, traffickerRelationshipToPermission);
            Assert.Equal(
                hasAccessToTenant ? HttpStatusCode.NoContent : HttpStatusCode.Forbidden,
                response.StatusCode);

            var exists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Tenant, LicensedFeature>(
                f => f.Id == Guid.Parse(tenantId), p => p.Id == Guid.Parse(lFeatureId), Constants.AssignedLink);

            if (hasAccessToTenant)
                Assert.True(exists);
            else
                Assert.False(exists);
        }

    }
}