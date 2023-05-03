using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestSubjectsCollection))]
    public class SubjectsControllerTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        public SubjectsControllerTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact, Order(0)]
        public async Task Create_Subject_Test()
        {
            // Arrange
            var subjectId = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/subjects");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new CreateSubject
                {
                    Id = subjectId,
                    Email = $"{subjectId}@test",
                    ActorId = Guid.Parse(Graph.Subject0),
                    RoleBusinessAccounts = new List<RoleBusinessAccount>
                    {
                        new() {BusinessAccountId = Guid.Parse(Graph.Tenant2), RoleId = Guid.Parse(Graph.CustomRole6)}
                    }
                }), Encoding.UTF8,
                MediaTypeNames.Application.Json);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);
            var result = await response.Content.ReadAsAsync<Subject>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
        }
        
        [Fact, Order(1)]
        public async Task Delete_Subject_Test()
        {
            // Arrange
            var subjectId = Graph.Subject3;
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/subjects/{subjectId}");
            request.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var response = await _fixture.RestClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}