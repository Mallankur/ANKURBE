using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using IdentityModel.Client;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [Collection(nameof(RestAssignUsersCollection))]
    public class RestUsersAssigmentTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public RestUsersAssigmentTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }


        [Theory, Order(1)]
        [MemberData(nameof(Scenarios.Assign_User_To_Role_Results_For_Subject1), MemberType = typeof(Scenarios))]

        public async Task Assign_And_Then_Unassign_User_ToFrom_Role_Test(string businessAccountId, string userId, string roleId, bool canAssign)
        {
            await _fixture.OngDB.GraphRepository.BulkUnassignSubjectFromRolesAsync(
                Guid.Parse(userId),
                new[]
                {
                    new RoleTenant
                    {
                        RoleId =  Guid.Parse(roleId),
                        TenantId = Guid.Parse(businessAccountId)
                    }
                });

            await AssignTest(
                Guid.Parse(businessAccountId), Guid.Parse(userId), Guid.Parse(roleId), canAssign);

            if (canAssign)
                await UnassignTest(
                    Guid.Parse(businessAccountId), Guid.Parse(userId), Guid.Parse(roleId));
        }
        private async Task AssignTest(Guid businessAccountId, Guid userId, Guid roleId, bool canAssign)
        {
            // Arrange
            var requestAssign = new HttpRequestMessage(HttpMethod.Post, $"/v1/users/{userId}/business-accounts/{businessAccountId}/roles/{roleId}");
            requestAssign.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var responseAssign = await _fixture.RestClient.SendAsync(requestAssign);
            var resultAssign = await responseAssign.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(canAssign ? HttpStatusCode.NoContent : HttpStatusCode.Forbidden, responseAssign.StatusCode);
            Assert.NotNull(resultAssign);

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Subject, Group, Role>(s => s.Id == userId,
                p => p.Id == roleId, Constants.MemberOfWithVariableLink, Constants.AssignedLink);

            if (canAssign)
                Assert.True(hasLink);
            else
                Assert.False(hasLink);
        }
        private async Task UnassignTest(Guid businessAccountId, Guid userId, Guid roleId)
        {
            //Arrange
            var requestUnassign = new HttpRequestMessage(HttpMethod.Delete, $"/v1/users/{userId}/business-accounts/{businessAccountId}/roles/{roleId}");
            requestUnassign.SetBearerToken(_fixture.Identities.Token[Graph.Subject0]);

            // Act
            var responseUnassign = await _fixture.RestClient.SendAsync(requestUnassign);
            var resultUnassign = await responseUnassign.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, responseUnassign.StatusCode);
            Assert.NotNull(resultUnassign);

            var hasNoLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Subject, Group, Role>(s => s.Id == userId,
                p => p.Id == roleId, Constants.MemberOfWithVariableLink, Constants.AssignedLink);
            Assert.False(hasNoLink);
        }
    }
}