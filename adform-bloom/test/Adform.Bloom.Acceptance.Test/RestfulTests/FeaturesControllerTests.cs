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
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.SharedKernel.Entities;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using Permission = Adform.Bloom.Contracts.Output.Permission;
using Role = Adform.Bloom.Domain.Entities.Role;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestFeaturesCollection))]
    public class FeaturesControllerTests
    {
        private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public FeaturesControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Order(0)]
        public async Task Get_Feature_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/features/{nodeId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Domain.Entities.Feature>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodeId, result.Id);
            Assert.Equal(node.Name, result.Name);
            Assert.Equal(node.Description, result.Description);
            Assert.Equal(node.Enabled, result.IsEnabled);
        }

        [Fact]
        [Order(0)]
        public async Task Get_NonExistent_Feature_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/features/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Order(0)]
        public async Task Get_Forbidden_Feature_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = (await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size)).Data.Where(f => !new[] { Graph.Feature5, Graph.Feature6 }.Contains(f.Id.ToString())).ToArray();
            var nodeId = nodes[Random.Next(0, nodes.Length - 1)].Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/features/{nodeId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject6]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        [Order(0)]
        public async Task Get_Features_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var nodesCount = nodes.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/features?limit=10");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Domain.Entities.Feature>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
        }

        [Fact]
        [Order(0)]
        public async Task Get_Features_DoesNotReturnDisabled()
        {
            // Arrange
            var cypher = _fixture.OngDB.GraphClient.Cypher.Match("(f:Feature)")
                .Return(f => f.As<Feature>());
            var nodes = (await cypher.ResultsAsync).ToList();
            var nodesCount = nodes.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/features?limit=100");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Domain.Entities.Feature>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.NotEqual(nodesCount, result.Count);
            Assert.True(result.All(f => f.IsEnabled));
        }

        [Theory]
        [Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Features_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), page, size);
            var nodesCount = nodes.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/features?limit={size}&offset={page}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Domain.Entities.Feature>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
        }

        [Fact]
        [Order(0)]
        public async Task Get_Features_With_ReturnTotalCount_Test()
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 1, 4);
            var totalCount = await _fixture.OngDB.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, 100);

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/features?limit=4&offset=1");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Feature>>();
            var pagedResult =
                new EntityPagination<Feature>(1, 4, totalCount.Data.Count, nodes.Data.ToList());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(nodes.Data.Count(), result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));

            var totalCountHeader = response.Headers.GetValues("total-count").First();
            Assert.Equal(pagedResult.TotalItems, int.Parse(totalCountHeader));
        }

        [Theory]
        [Order(1)]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Create_Feature_Test(bool isEnabled)
        {
            // Arrange
            var name = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            // Act
            var result = await CreateFeatureAsync(name, description, isEnabled);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(isEnabled, result.Enabled);
        }

        [Fact]
        [Order(int.MaxValue)]
        public async Task Delete_Feature_Test()
        {
            // Arrange
            var created = await CreateFeatureAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), true);
            var nodes =
                await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Domain.Entities.Feature>(p => true);
            var count = nodes.Data.Count;
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/features/{created.Id}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var deletedNode =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Domain.Entities.Feature>(
                    p => p.Id == created.Id);
            Assert.Null(deletedNode);
            nodes = await _fixture.OngDB.GraphRepository
                .SearchPaginationAsync<Domain.Entities.Feature>(p => true);
            var newCount = nodes.Data.Count;
            Assert.Equal(count - 1, newCount);
        }

        [Fact]
        public async Task Assign_Permission_To_Feature()
        {
            // Arrange
            var featureId = Graph.Feature1;
            var permission = await _fixture.OngDB.GraphRepository.CreateNodeAsync(new Permission
            {
                Name = "TEMPORAL_PERMISSION"
            });
            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/permissions/{permission.Id}");
            
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var exists = await _fixture.OngDB.GraphRepository
                .HasRelationshipAsync<Domain.Entities.Feature, Permission>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == permission.Id, Constants.ContainsLink);
            Assert.True(exists);
        }

        [Fact]
        public async Task Feature_NotFound()
        {
            // Arrange
            var featureId = "98b3bd33-5f69-4dc9-aa89-8c2b0af900da";
            var permissionId = "65085381-8aea-4ab5-a035-21e1740b3d3a";
            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/permissions/{permissionId}");
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
            var featureId = Graph.Feature1;
            var permissionId = "65085381-8aea-4ab5-a035-21e1740b3d3b";
            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/permissions/{permissionId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Unassign_Permission_From_Feature()
        {
            // Arrange
            const string tenantId = Graph.Tenant7;
            const string featureId = Graph.Feature3;

            const string roleId = Graph.CustomRole6;
            const string permissionId = Graph.Permission5;
            const string notUnassignedPermissionId = Graph.Permission6;

            var featureToPermissionExists = await _fixture.OngDB.GraphRepository
                .HasRelationshipAsync<Domain.Entities.Feature, Permission>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            var roleToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            var featureToPermissionNotToUnassignExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(notUnassignedPermissionId),
                    Constants.ContainsLink);

            var request =
                new HttpRequestMessage(HttpMethod.Delete, $"/v1/features/{featureId}/permissions/{permissionId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            Assert.True(featureToPermissionExists);
            Assert.True(roleToPermissionExists);
            Assert.True(featureToPermissionNotToUnassignExists);
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            featureToPermissionExists = await _fixture.OngDB.GraphRepository
                .HasRelationshipAsync<Domain.Entities.Feature, Permission>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            Assert.False(featureToPermissionExists);
            roleToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            Assert.False(roleToPermissionExists);
            featureToPermissionNotToUnassignExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(notUnassignedPermissionId),
                    Constants.ContainsLink);
            Assert.True(featureToPermissionNotToUnassignExists);
        }

        [Fact]
        public async Task Assign_CoFeature_To_Feature()
        {
            // Arrange
            var featureId = Graph.Feature1;
            var cofeatureId = Graph.Feature2;
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/features/{cofeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var exists = await _fixture.OngDB.GraphRepository
                .HasRelationshipAsync<Domain.Entities.Feature, Domain.Entities.Feature>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(cofeatureId), Constants.DependsOnLink);
            Assert.True(exists);
        }

        [Fact]
        public async Task Assign_CoFeature_Feature_NotFound()
        {
            // Arrange
            var featureId = Guid.NewGuid();
            var cofeatureId = Graph.Feature2;
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/features/{cofeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Assign_CoFeature_Permission_NotFound()
        {
            // Arrange
            var featureId = Graph.Feature1;
            var cofeatureId = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/features/{featureId}/features/{cofeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Unassign_CoFeature_From_Feature()
        {
            // Arrange
            var featureId = Guid.Parse(Graph.Feature1);
            var cofeatureId = Guid.Parse(Graph.Feature2);

            await _fixture.OngDB.GraphRepository
                .CreateRelationshipAsync<Domain.Entities.Feature, Domain.Entities.Feature>(
                    f => f.Id == featureId,
                    d => d.Id == cofeatureId, Constants.DependsOnLink);
            var request =
                new HttpRequestMessage(HttpMethod.Delete, $"/v1/features/{featureId}/features/{cofeatureId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var featureToPermissionExists = await _fixture.OngDB.GraphRepository
                .HasRelationshipAsync<Domain.Entities.Feature, Domain.Entities.Feature>(
                    f => f.Id == featureId,
                    p => p.Id == cofeatureId, Constants.DependsOnLink);
            Assert.False(featureToPermissionExists);
        }

        private async Task<Feature> CreateFeatureAsync(string name, string description, bool isEnabled)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/features");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreateFeature
                {
                    Name = name,
                    Description = description,
                    IsEnabled = isEnabled
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);
            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Feature>();

            return result;
        }
    }
}