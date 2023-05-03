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
using Adform.Bloom.Infrastructure;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestPoliciesCollection))]
    public class PoliciesControllerTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        public PoliciesControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_Policy_Test()
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1);
            var policy = policies.Data.First();
            var policyId = policy.Id;
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/policies/{policyId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Policy>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(policyId, result.Id);
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Policy_Test()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/policies/{Guid.NewGuid()}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(0)]
        public async Task Get_Policies_Test()
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 10);
            var policiesCount = policies.Data.Count;

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/policies?limit=10");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Policy>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(policiesCount, result.Count);
            Assert.All(result, r => policies.Data.Any(p => p.Id == r.Id));
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Policies_With_Paging_Test(int size, int page)
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, page, size);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/policies?limit={size}&offset={page}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Policy>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(policies.Data.Count, result.Count);
            Assert.All(result, r => policies.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Policies_With_ReturnTotalCount_Test()
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 1, 4);

            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/policies?limit=4&offset=1");
            request.Headers.Add("Return-Total-Count", "true");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<IReadOnlyCollection<Policy>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(policies.Data.Count, result.Count);
            Assert.All(result, r => policies.Data.Any(p => p.Id == r.Id));

            var totalCount = response.Headers.GetValues("total-count").First();
            Assert.Equal(policies.TotalItems, int.Parse(totalCount));
        }

        // Add test of what is returned when there are no policies. But it needs correct DELETE implementation

        [Fact, Order(1)]
        public async Task Create_Policy_Test()
        {
            // Arrange
            var rootPolicy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();

            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/policies");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreatePolicy
                {
                    Name = "new_policy"
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Policy>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
            Assert.StartsWith("new_policy", result.Name);
        }

        [Fact, Order(1)]
        public async Task Create_Policy_And_Relation_Returns_Success_When_Parent_Is_Present()
        {
            // Arrange
            var rootPolicy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();

            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/policies");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreatePolicy
                {
                    Name = "new_policy",
                    ParentId = rootPolicy.Id
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Policy>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
            Assert.StartsWith("new_policy", result.Name);

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Policy, Policy>(p => p.Id == result.Id,
                rp => rp.Id == rootPolicy.Id, Constants.ChildOfLink);
            Assert.True(hasLink);
        }

        [Fact, Order(1)]
        public async Task Create_Policy_And_Relation_Returns_NotFound_When_Parent_Is_Null()
        {
            // Arrange
            var rootPolicy = Guid.Empty;
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/policies");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreatePolicy
                {
                    Name = "new_policy",
                    ParentId = rootPolicy
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Order(1)]
        public async Task Delete_Policy_Test()
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 5);
            var policy = policies.Data.Last();
            var policyId = policy.Id;
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/policies/{policyId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var deletedPolicy = await _fixture.OngDB.GraphRepository.GetNodeAsync<Policy>(p => p.Id == policyId);
            Assert.Null(deletedPolicy);
        }
    }
}