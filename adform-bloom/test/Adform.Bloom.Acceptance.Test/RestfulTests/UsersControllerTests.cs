using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestSubjectsCollection))]
    public class UsersControllerTests : IClassFixture<TestsFixture>
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private readonly TestsFixture _fixture;

        public UsersControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Users_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Can_Read_Other_Users_Assigned_To_The_Same_Tenant()
        {
            // Arrange;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{Graph.Subject2}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject3]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Users_Test()
        {
            // Arrange;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{Graph.Subject0}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject6]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Users_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/users?limit=10");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Contracts.Output.Subject>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_User_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{nodeId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Subject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodeId, result.Id);
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Users_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), page, size);
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users?limit={size}&offset={page}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Contracts.Output.Subject>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Users_With_ReturnTotalCount_Test()
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 1, 4);

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/users?limit=4&offset=1");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Contracts.Output.Subject>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodes.Data.Count(), result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));

            var totalCountHeader = response.Headers.GetValues("total-count").First();
            Assert.Equal(nodes.TotalItems, int.Parse(totalCountHeader));
        }

        [Fact, Order(0)]
        public async Task Get_User_Roles_Test()
        {
            // Arrange
            const int limit = 10;
            var nodes = await _fixture.OngDB.AccessRepositoriesContainer
                .Get<Contracts.Output.Subject, QueryParamsTenantIds, Contracts.Output.Role>()
                .EvaluateAccessAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new Contracts.Output.Subject() {Id = Guid.Parse(Graph.Subject0)}, 0, limit, new QueryParamsTenantIds());
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{Graph.Subject0}/roles?limit={limit}");
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
        public async Task Get_User_Roles_With_Paging_Test()
        {
            // Arrange
            const int limit = 10;
            const int offset = 2;
            var nodes = await _fixture.OngDB.AccessRepositoriesContainer
                .Get<Contracts.Output.Subject, QueryParamsTenantIds, Contracts.Output.Role>()
                .EvaluateAccessAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new Contracts.Output.Subject() {Id = Guid.Parse(Graph.Subject0)}, offset, limit, new QueryParamsTenantIds());
            var nodesCount = nodes.Data.Count();

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/v1/users/{Graph.Subject0}/roles?limit={limit}&offset={offset}");
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
        public async Task Get_User_Roles_With_ReturnTotalCount_Test()
        {
            // Arrange
            const int limit = 10;
            const int offset = 2;
            var nodes = await _fixture.OngDB.AccessRepositoriesContainer
                .Get<Contracts.Output.Subject, QueryParamsTenantIds, Contracts.Output.Role>()
                .EvaluateAccessAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new Contracts.Output.Subject() {Id = Guid.Parse(Graph.Subject0)}, offset, limit, new QueryParamsTenantIds());

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/v1/users/{Graph.Subject0}/roles?limit={limit}&offset={offset}");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodes.Data.Count, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));

            var totalCount = response.Headers.GetValues("total-count").First();
            Assert.Equal(nodes.TotalItems, int.Parse(totalCount));
        }

        // FIX : Add case of system roles
        [Fact, Order(0)]
        public async Task Get_User_Roles_NoAccess_Empty_Roles_Test()
        {
            // Arrange;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/users/{Graph.Subject0}/roles");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject3]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Role>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(result);
        }
    }
}