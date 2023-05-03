using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using IdentityModel.Client;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestRolesCollection))]
    public class RolesControllerTests : IClassFixture<TestsFixture>
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private readonly TestsFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public RolesControllerTests(TestsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        [Fact, Order(0)]
        public async Task Get_Role_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsRoles(), 0, size);
            // var nodeId = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)].Id;
            foreach (var nodeId in nodes.Data.ToArray().Select(x => x.Id))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/roles/{nodeId}");
                request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

                // Act
                var response = await _fixture.RestClient.SendAsync(request);
                var result = await response.Content.ReadAsAsync<Role>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(result);
                Assert.Equal(nodeId, result.Id);
            }
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Role_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/roles/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Role_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/roles/{Graph.CustomRole5}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject6]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Roles_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsRoles(), 0, size);
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/roles?limit=10");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Roles_DoesNotReturnTransitional()
        {
            // Arrange
            var cypher = _fixture.OngDB.GraphClient.Cypher.Match("(r:Role)")
                .Return(r => r.As<Feature>());
            var nodes = (await cypher.ResultsAsync).ToList();
            var nodesCount = nodes.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/roles?limit=100");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.NotEqual(nodesCount, result.Count);
            Assert.True(!result.Select(r => r.Id).Contains(Guid.Parse(Graph.TransitionalRole24)));
        }

        [Fact, Order(0)]
        public async Task Get_Roles_With_Paging_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsRoles(), 0, size);
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/roles?limit={size}&offset=0");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Roles_With_ReturnTotalCount_Test()
        {
            // Arrange
            var roles = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsRoles(), 0, 4);

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/roles?limit=4&offset=0");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(roles.Data.Count, result.Count);
            Assert.All(result, r => roles.Data.Any(p => p.Id == r.Id));

            var totalCount = response.Headers.GetValues("total-count").First();
            Assert.Equal(roles.TotalItems, int.Parse(totalCount));
        }

        [Fact, Order(1)]
        public async Task Create_Role_Test()
        {
            // Arrange
            var policy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();
            var tenant = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 0, 1)).Data.First();
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/roles");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreateRole
                {
                    PolicyId = policy.Id,
                    TenantId = tenant.Id,
                    Name = "new_role"
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Role>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
            Assert.StartsWith("new_role", result.Name);

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Policy, Role>(
                rp => rp.Id == policy.Id,
                p => p.Id == result.Id, Constants.ContainsLink);
            Assert.True(hasLink);
        }

        [Theory, Order(1)]
        [InlineData("Case 0", Graph.Subject0, Graph.Tenant1, true)]
        [InlineData("Case 1", Graph.Subject0, Graph.Tenant3, true)]
        [InlineData("Case 2", Graph.Subject2, Graph.Tenant0, false)]
        [InlineData("Case 3", Graph.Subject10, Graph.Tenant16, false)]
        public async Task Create_Role_And_Tenant_Relation_Test(string caseName, string subjectId, string tenantId,
            bool canCreateRole)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);

            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/roles");
            request.SetBearerToken(_fixture.Identities.Token[subjectId]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreateRole
                {
                    Name = "new_role",
                    PolicyId = policyId,
                    TenantId = Guid.Parse(tenantId)
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Role>();

            // Assert
            Assert.Equal(canCreateRole ? HttpStatusCode.Created : HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(result);

            if (canCreateRole)
                Assert.StartsWith("new_role", result.Name);

            var hasPolicyLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Policy, Role>(
                rp => rp.Id == policyId,
                p => p.Id == result.Id, Constants.ContainsLink);

            if (canCreateRole)
                Assert.True(hasPolicyLink);
            else
                Assert.False(hasPolicyLink);

            var hasTenantLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Tenant, Role>(
                rp => rp.Id == Guid.Parse(tenantId),
                p => p.Id == result.Id, Constants.OwnsLink);

            if (canCreateRole)
                Assert.True(hasTenantLink);
            else
                Assert.False(hasTenantLink);
        }

        [Theory, Order(int.MaxValue)]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_Subject1), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_Subject3), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_AdformAdmin), MemberType = typeof(Scenarios))]
        public async Task Delete_Role_Test(string subjectId, string roleId, HttpStatusCode expectedCode)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/roles/{roleId}");
            request.SetBearerToken(_fixture.Identities.Token[subjectId]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);

            var deletedRole = await _fixture.OngDB.GraphRepository.GetNodeAsync<Role>(p => p.Id == Guid.Parse(roleId));
            if (expectedCode == HttpStatusCode.Forbidden)
                Assert.NotNull(deletedRole);
            else
                Assert.Null(deletedRole);
        }

        [Theory, Order(1)]
        [MemberData(nameof(Scenarios.Assign_Permission_To_Role_Results_For_Subject3), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Assign_Permission_To_Role_Results_For_AdformAdmin),
            MemberType = typeof(Scenarios))]
        public async Task AssignAndUnassign_Permission_To_Role_Test(string subjectId, string roleId,
            string permissionId, bool success)
        {
            var permission =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Permission>(r => r.Id == Guid.Parse(permissionId));

            Assert.NotNull(permission);

            await AssignTest(subjectId, roleId, success, permission);

            if (success)
                await UnassignTest(subjectId, roleId, permission);
        }

        private async Task AssignTest(string subjectId, string roleId, bool success, Permission permission)
        {
            // Arrange
            var requestAssign =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/roles/{roleId}/permissions/{permission.Id}");
            requestAssign.SetBearerToken(_fixture.Identities.Token[subjectId]);

            // Act
            var responseAssign = await _fixture.RestClient.SendAsync(requestAssign);
            var resultAssign = await responseAssign.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(success ? HttpStatusCode.NoContent : HttpStatusCode.Forbidden,
                responseAssign.StatusCode);
            Assert.NotNull(resultAssign);

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                p => p.Id == Guid.Parse(roleId),
                rp => rp.Id == permission.Id, Constants.ContainsLink);

            if (success)
                Assert.True(hasLink);
            else
                Assert.False(hasLink);
        }

        private async Task UnassignTest(string subjectId, string roleId, Permission permission)
        {
            //Arrange
            var requestUnassign =
                new HttpRequestMessage(HttpMethod.Delete, $"/v1/roles/{roleId}/permissions/{permission.Id}");
            requestUnassign.SetBearerToken(_fixture.Identities.Token[subjectId]);

            // Act
            var responseUnassign = await _fixture.RestClient.SendAsync(requestUnassign);
            var resultUnassign = await responseUnassign.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, responseUnassign.StatusCode);
            Assert.NotNull(resultUnassign);

            var hasNoLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                p => p.Id == Guid.Parse(roleId),
                rp => rp.Id == permission.Id, Constants.ContainsLink);
            Assert.False(hasNoLink);
        }

        [Fact, Order(1)]
        public async Task Assign_Permission_To_Role_Without_Feature_Forbidden()
        {
            // Arrange
            var role = await _fixture.OngDB.GraphRepository.GetNodeAsync<Role>(r =>
                r.Id == Guid.Parse(Graph.CustomRole7));
            var permission =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Permission>(
                    r => r.Id == Guid.Parse(Graph.Permission7));

            var requestAssign =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/roles/{role.Id}/permissions/{permission.Id}");
            requestAssign.SetBearerToken(_fixture.Identities.Token[Graph.Subject4]);

            // Act
            var responseAssign = await _fixture.RestClient.SendAsync(requestAssign);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, responseAssign.StatusCode);
        }
    }
}