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
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestPermissionsCollection))]
    public class PermissionsControllerTests : IClassFixture<TestsFixture>
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private readonly TestsFixture _fixture;

        public PermissionsControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_Permission_Test()
        {
            // Arrange
            const int size = 10;
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsTenantIds(),0, size);
            var permissionId = permissions.Data.ToArray()[Random.Next(0, permissions.Data.ToArray().Length - 1)].Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/permissions/{permissionId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Permission>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(permissionId, result.Id);
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Permission_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/permissions/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Permission_Test()
        {
            // Arrange
            const int size = 10;
            var permissions = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject10], 
                    new QueryParamsTenantIds(),0, size)).Data.ToArray();
            var permissionId = permissions[Random.Next(0, permissions.Length - 1)].Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/permissions/{permissionId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject4]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            await response.Content.ReadAsAsync<Permission>();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Permissions_Test()
        {
            // Arrange
            const int size = 10;
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsTenantIds(),0, size);
            var permissionsCount = permissions.Data.ToArray().Length;

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/permissions?limit=10");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Permission>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(permissionsCount, result.Count);
            Assert.All(result, r => permissions.Data.Any(p => p.Id == r.Id));
        }

        [Theory, Order(0)]
        [InlineData(Graph.Subject0, 3, 2)]
        [InlineData(Graph.Subject0, 1, 4)]
        [InlineData(Graph.Subject0, 2, 3)]
        public async Task Get_Permissions_With_Paging_Test(string sub, int size, int page)
        {
            // Arrange
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[sub],
                    new QueryParamsTenantIds(), page, size);
            var nodesCount = permissions.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/permissions?limit={size}&offset={page}");
            request.SetBearerToken(_fixture.Identities.Token[sub]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Permission>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => permissions.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Permissions_With_ReturnTotalCount_Test()
        {
            // Arrange
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsTenantIds(),2, 4);

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/permissions?limit=4&offset=1");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Contracts.Output.Permission>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(permissions.Data.Count, result.Count);
            Assert.All(result, r => permissions.Data.Any(p => p.Id == r.Id));

            var totalCountHeader = response.Headers.GetValues("total-count").First();
            Assert.Equal(permissions.TotalItems, int.Parse(totalCountHeader));
        }

        [Fact, Order(1)]
        public async Task Create_Permission_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/permissions");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreatePolicy
                {
                    Name = "new_permission"
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Permission>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
            Assert.StartsWith("new_permission", result.Name);
        }

        [Theory, Order(2)]
        [MemberData(nameof(Graph.AllPermissionsData), MemberType = typeof(Graph))]
        public async Task Delete_Permission_Test(string permissionId)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/permissions/{permissionId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var deletedPermission =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Permission>(p => p.Id == Guid.Parse(permissionId));
            Assert.Null(deletedPermission);
        }
    }
}