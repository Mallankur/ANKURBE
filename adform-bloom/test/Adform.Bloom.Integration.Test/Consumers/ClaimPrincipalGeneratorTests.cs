using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Services;
using Adform.Bloom.Client.Contracts;
using Adform.Bloom.Client.Contracts.Services;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.SharedKernel.Extensions;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Xunit;
using Guid = System.Guid;

namespace Adform.Bloom.Integration.Test.Consumers
{
    [Collection(nameof(ConsumerCollection))]
    public class ClaimPrincipalGeneratorTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        private IConfigurationRoot _configuration;
        private Dictionary<string, string> _tokens;
        private Dictionary<string, ClaimsPrincipal> _principals;

        public ClaimPrincipalGeneratorTests(TestsFixture fixture)
        {
            _fixture = fixture;
            _configuration = fixture.Configuration;
            _tokens = fixture.Identities.Token;
            _principals = fixture.BloomApiPrincipal;
        }

        [Theory]
        [InlineData(Graph.Subject0)]
        [InlineData(Graph.Subject1)]
        [InlineData(Graph.Subject3)]
        public async Task GenerateAsync_Creates_BloomIdentity_If_Subject_Exist(string subjectId)
        {
            // Arrange
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetSection("BloomRuntimeApi")["Host"])
            };
            httpClient.SetBearerToken(_tokens[subjectId]);

            var client = new BloomRuntimeClient(httpClient);
            var generator = new ClaimPrincipalGenerator(client);
            // Act
            var result = await generator.GenerateAsync(Guid.Parse(subjectId), null, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var bloomGeneratedIdentity =
                result.Identities.FirstOrDefault(o => o.AuthenticationType == Authentication.Bloom);
            var bloomEnhancedIdentity = _principals[subjectId].Identities
                .FirstOrDefault(o => o.AuthenticationType == Authentication.Bloom);
            Assert.Equal(_principals[subjectId].Identities.Count(), result.Identities.Count());
            Assert.Equal(_principals[subjectId].GetActorId(), result.GetActorId());
            Assert.Equal(bloomEnhancedIdentity.Claims.Count(), bloomGeneratedIdentity.Claims.Count());
        }

        [Fact]
        public async Task GenerateAsync_Creates_EmptyBloomIdentity_If_Subject_DoesntExist()
        {
            // Arrange
            var subject = Guid.NewGuid();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetSection("BloomRuntimeApi")["Host"])
            };
            httpClient.SetBearerToken(_tokens[Graph.Subject0]);

            var client = new BloomRuntimeClient(httpClient);
            var generator = new ClaimPrincipalGenerator(client);
            // Act
            var result = await generator.GenerateAsync(subject, null, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var bloomGeneratedIdentity =
                result.Identities.FirstOrDefault(o => o.AuthenticationType == Authentication.Bloom);
            Assert.Equal(2, result.Identities.Count());
            Assert.Equal(subject.ToString(), result.GetActorId());
            Assert.Equal(0, bloomGeneratedIdentity.Claims.Count());
        }

        [Theory]
        [MemberData(nameof(CreateScenario), MemberType = typeof(ClaimPrincipalGeneratorTests))]
        public async Task GenerateAsync_Creates_BloomIdentity_FilteredBy_ActorPrincipal(string caseName, Guid subjectId, Guid actorId,
            RuntimeResponse[] expectedResult)
        {
            // Arrange
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetSection("BloomRuntimeApi")["Host"])
            };
            httpClient.SetBearerToken(_tokens[subjectId.ToString()]);

            var client = new BloomRuntimeClient(httpClient);
            var generator = new ClaimPrincipalGenerator(client);
            var actor = await generator.GenerateAsync(actorId, null);
            var isAdmin = actor.IsAdformAdmin();
            // Act
            var result = await generator.GenerateAsync(subjectId, actor, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var bloomGeneratedIdentity =
                result.Identities.FirstOrDefault(o => o.AuthenticationType == Authentication.Bloom);
            var bloomEnhancedIdentity = _principals[subjectId.ToString()].Identities
                .FirstOrDefault(o => o.AuthenticationType == Authentication.Bloom);
            Assert.Equal(_principals[subjectId.ToString()].Identities.Count(), result.Identities.Count());
            Assert.Equal(_principals[subjectId.ToString()].GetActorId(), result.GetActorId());
            if (isAdmin)
                Assert.Equal(bloomEnhancedIdentity.Claims.Count(), bloomGeneratedIdentity.Claims.Count());
            else
                Assert.NotEqual(bloomEnhancedIdentity.Claims.Count(), bloomGeneratedIdentity.Claims.Count());
            Assert.Equal(expectedResult.Sum(o => o.Roles.Count() + o.Permissions.Count()),
                bloomGeneratedIdentity.Claims.Count());
        }

        public static TheoryData<string, Guid, Guid, RuntimeResponse[]> CreateScenario()
        {
            var data = new TheoryData<string, Guid, Guid, RuntimeResponse[]>();
            data.Add("Case 0", Guid.Parse(Graph.Subject0), Guid.Parse(Graph.Subject0), new RuntimeResponse[]
            {
                new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant9),
                    Permissions = new[] {""},
                    Roles = new[] {Graph.AdformAdmin}
                }
            });
            data.Add("Case 1", Guid.Parse(Graph.Subject4), Guid.Parse(Graph.Subject2), new RuntimeResponse[]{});
            return data;
        }
    }
}